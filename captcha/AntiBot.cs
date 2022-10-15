using System;
using System.Runtime.Caching;
using System.Threading;

using Microsoft.AspNetCore.Http;

namespace captcha
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAntiBotConfig
    {
        public const int    SAME_IP_BANNED_INTERVAL_IN_SECONDS  = 120;
		public const int    SAME_IP_INTERVAL_REQUEST_IN_SECONDS = 10;
		public const int    SAME_IP_MAX_REQUEST_IN_INTERVAL     = 3;

        public int?   SameIpBannedIntervalInSeconds  { get; }
        public int?   SameIpIntervalRequestInSeconds { get; }
        public int?   SameIpMaxRequestInInterval     { get; }
        public string CaptchaPageTitle               { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct AntiBot
    {
        /// <summary>
        /// 
        /// </summary>
        private sealed class RequestMarker
        {
            private const int FALSE = 0;
            private const int TRUE  = ~FALSE;

            public RequestMarker()
            {
                _DateTimeTicks = DateTime.Now.Ticks;
                _Count         = 1;
                _IsBanned      = FALSE; 
            }

            private int  _Count;
            private int  _IsBanned;
            private long _DateTimeTicks;

            public DateTime DateTime => new DateTime( _DateTimeTicks );
            public int      Count    => _Count;
            public bool     IsBanned => (_IsBanned != FALSE); 

            public void CountIncrement() => Interlocked.Increment( ref _Count );
            public void Banned()
            {
                Interlocked.Exchange( ref _IsBanned, TRUE );
                Interlocked.Exchange( ref _DateTimeTicks, DateTime.Now.Ticks );
            }
            public int  GetWaitRemainSeconds( in Config config )
            {
                var passSeconds = (DateTime.Now - this.DateTime).TotalSeconds;
                int result;
                if ( this.IsBanned )
                {
                    result = config.SameIpBannedIntervalInSeconds - Convert.ToInt32( passSeconds ); // +1;
                }
                else
                {
                    result = config.SameIpIntervalRequestInSeconds - Convert.ToInt32( passSeconds ) + 1;
                }
                return ((0 < result) ? result : 0);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        internal readonly struct Config
        {
            public string RemoteIpAddress                { get; init; }
            public int    SameIpBannedIntervalInSeconds  { get; init; }
            public int    SameIpIntervalRequestInSeconds { get; init; }
            public int    SameIpMaxRequestInInterval     { get; init; }
        }

        private const string KEY_CACHE_KEY = "SAME_IP_REQUEST";

        private Config _Config;
        internal AntiBot( in Config config )
        {
            _Config  = config;
            CacheKey = (KEY_CACHE_KEY + ',' + config.RemoteIpAddress);
        }        

        private string CacheKey { get; }
        private bool TryGetCurrentRequestMarker( out RequestMarker requestMarker ) 
        {
            requestMarker = MemoryCache.Default.Get( CacheKey ) as RequestMarker;
            return (requestMarker != null);
        }
        private void AddCurrentRequestMarker2Cache( RequestMarker requestMarker, int absoluteExpirationInCacheInSeconds )
        {
            if ( requestMarker == null ) throw (new ArgumentNullException( nameof(requestMarker) ));

            MemoryCache.Default.Remove( CacheKey );
            MemoryCache.Default.Add( CacheKey, requestMarker, new CacheItemPolicy()
            {
                AbsoluteExpiration = requestMarker.DateTime.AddSeconds( Convert.ToDouble( absoluteExpirationInCacheInSeconds ) ),
                SlidingExpiration  = TimeSpan.Zero,
                Priority           = CacheItemPriority.NotRemovable,

                //RemovedCallback    = e => { System.Diagnostics.Debug.WriteLine( e ); },
                //UpdateCallback     = e => { System.Diagnostics.Debug.WriteLine( e ); },
            });
        }
        private void RemoveCurrentRequestMarkerFromCache() => MemoryCache.Default.Remove( CacheKey );
        private void BannedRequest( RequestMarker requestMarker )
        {
            requestMarker.Banned();

            AddCurrentRequestMarker2Cache( requestMarker, _Config.SameIpBannedIntervalInSeconds + 1 );
        }

        public bool IsRequestValid() => TryGetCurrentRequestMarker( out var requestMarker ) ? (requestMarker.Count < _Config.SameIpMaxRequestInInterval) : true; 
        public bool IsNeedRedirectOnCaptchaIfRequestNotValid()
        {
            if ( !IsRequestValid() && TryGetCurrentRequestMarker( out var requestMarker ) )
            {
                var waitRemainSeconds = requestMarker.GetWaitRemainSeconds( _Config );
                if ( 3 < waitRemainSeconds )  
                {
                    BannedRequest( requestMarker );

                    return (true);
                }
                else
                {
                    Thread.Sleep( Math.Max( 1, waitRemainSeconds ) * 1000 );
                }
            }
            return (false);
        }
        public void MarkRequest()
        {
            if ( TryGetCurrentRequestMarker( out var requestMarker ) )
            {
                requestMarker.CountIncrement();
            }
            else
            {
                requestMarker = new RequestMarker();

                AddCurrentRequestMarker2Cache( requestMarker, _Config.SameIpIntervalRequestInSeconds );
            }
        }
        public void MakeAllowRequests() => RemoveCurrentRequestMarkerFromCache();
        public int GetWaitRemainSeconds() => TryGetCurrentRequestMarker( out var requestMarker ) ? requestMarker.GetWaitRemainSeconds( _Config ) : 0;

        public static object CreateGotoOnCaptchaResponseObj() => new { err = "goto-on-captcha" };
    }

    /// <summary>
    /// 
    /// </summary>
    public static class AntiBotHelper
    {
        private const string LOAD_MODEL_DUMMY_TEXT = "_dummy_";

        public static AntiBot ToAntiBot( this HttpContext httpContext, IAntiBotConfig opt )
        {
            var cfg = new AntiBot.Config() 
            { 
                RemoteIpAddress                = httpContext.Connection.RemoteIpAddress?.ToString(),
                SameIpBannedIntervalInSeconds  = opt.SameIpBannedIntervalInSeconds .GetValueOrDefault( IAntiBotConfig.SAME_IP_BANNED_INTERVAL_IN_SECONDS  ),
                SameIpIntervalRequestInSeconds = opt.SameIpIntervalRequestInSeconds.GetValueOrDefault( IAntiBotConfig.SAME_IP_INTERVAL_REQUEST_IN_SECONDS ),
                SameIpMaxRequestInInterval     = opt.SameIpMaxRequestInInterval    .GetValueOrDefault( IAntiBotConfig.SAME_IP_MAX_REQUEST_IN_INTERVAL     ),
            };
            var antiBot = new AntiBot( cfg );
            return (antiBot);
        }

        public static void MarkRequestEx( this in AntiBot antiBot, string text )
        {
            if ( text != LOAD_MODEL_DUMMY_TEXT )
            {
                antiBot.MarkRequest();
            }
        }
    }
}
