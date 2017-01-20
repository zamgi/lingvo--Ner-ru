using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;

namespace captcha
{
    /// <summary>
    /// 
    /// </summary>
    public struct AntiBotConfig
    {
        public const string CAPTCHA_PAGE_URL = "~/Captcha.aspx";
        public const int    SAME_IP_BANNED_INTERVAL_IN_SECONDS  = 120;
		public const int    SAME_IP_INTERVAL_REQUEST_IN_SECONDS = 10;
		public const int    SAME_IP_MAX_REQUEST_IN_INTERVAL     = 3;

        public HttpContext HttpContext;
        public string      CaptchaPageUrl;
        public int         SameIpBannedIntervalInSeconds;
        public int         SameIpIntervalRequestInSeconds;
        public int         SameIpMaxRequestInInterval;

        public static AntiBotConfig CreateDefault( HttpContext httpContext )
        {
            var config = new AntiBotConfig()
            {
                HttpContext                    = httpContext,
                CaptchaPageUrl                 = CAPTCHA_PAGE_URL,
                SameIpBannedIntervalInSeconds  = SAME_IP_BANNED_INTERVAL_IN_SECONDS,
                SameIpIntervalRequestInSeconds = SAME_IP_INTERVAL_REQUEST_IN_SECONDS,
                SameIpMaxRequestInInterval     = SAME_IP_MAX_REQUEST_IN_INTERVAL,
            };
            return (config);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct AntiBot
    {
        /// <summary>
        /// 
        /// </summary>
        private class RequestMarker
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

            public DateTime DateTime
            {
                get { return (new DateTime( _DateTimeTicks )); }
            }
            public int      Count
            {
                get { return (_Count); }
            }
            public bool     IsBanned
            {
                get { return (_IsBanned != FALSE); }
            }

            public void CountIncrement()
            {
                Interlocked.Increment( ref _Count );
            }
            public void Banned()
            {
                Interlocked.Exchange( ref _IsBanned, TRUE );
                Interlocked.Exchange( ref _DateTimeTicks, DateTime.Now.Ticks );
                //_DateTime = DateTime.Now;
            }
            public int  GetWaitRemainSeconds( ref AntiBotConfig config )
            {
                var passSeconds = (DateTime.Now - this.DateTime).TotalSeconds;
                var result = 0;
                if ( this.IsBanned )
                {
                    result = config.SameIpBannedIntervalInSeconds - Convert.ToInt32( passSeconds ); // +1;
                }
                else
                {
                    result = config.SameIpIntervalRequestInSeconds - Convert.ToInt32( passSeconds ) + 1;
                }
                return ((result > 0) ? result : 0);
            }
        }

        private const string KEY_CACHE_KEY = "SAME_IP_REQUEST";

        private AntiBotConfig _Config;

        public AntiBot( AntiBotConfig config ) : this()
        {
            _Config = config;

            if ( string.IsNullOrEmpty( _Config.CaptchaPageUrl ) )
                _Config.CaptchaPageUrl = AntiBotConfig.CAPTCHA_PAGE_URL;

            if ( _Config.SameIpBannedIntervalInSeconds <= 0 )
                _Config.SameIpBannedIntervalInSeconds = AntiBotConfig.SAME_IP_BANNED_INTERVAL_IN_SECONDS;

            if ( _Config.SameIpIntervalRequestInSeconds <= 0 )
                _Config.SameIpIntervalRequestInSeconds = AntiBotConfig.SAME_IP_INTERVAL_REQUEST_IN_SECONDS;

            if ( _Config.SameIpMaxRequestInInterval <= 0 )
                _Config.SameIpMaxRequestInInterval = AntiBotConfig.SAME_IP_MAX_REQUEST_IN_INTERVAL;
        }

        private string CACHE_KEY
        {
            get 
            {
                return (KEY_CACHE_KEY + ',' + _Config.HttpContext.Request.UserHostAddress); 
            }
        }
        private RequestMarker GetCurrentRequestMarker()
        {
            var obj = _Config.HttpContext.Cache[ CACHE_KEY ];
            if ( obj != null )
            {
                return ((RequestMarker) obj);
            }
            return (null);
        }
        private void PutCurrentRequestMarker2Cache( RequestMarker requestMarker, int absoluteExpirationInCacheInSeconds )
        {
            if ( requestMarker == null )
                throw (new ArgumentNullException("requestMarker"));

            _Config.HttpContext.Cache.Remove( CACHE_KEY );
            _Config.HttpContext.Cache.Add
                (
                CACHE_KEY,
                requestMarker, 
                null,
                requestMarker.DateTime.AddSeconds( Convert.ToDouble( absoluteExpirationInCacheInSeconds ) ),
                TimeSpan.Zero, 
                CacheItemPriority.NotRemovable, 
                null
                );
        }
        private void ClearCurrentRequestMarkerInCache()
        {
            _Config.HttpContext.Cache.Remove( CACHE_KEY );
        }
        private void BannedRequest( RequestMarker requestMarker )
        {
            requestMarker.Banned();

            PutCurrentRequestMarker2Cache( requestMarker, _Config.SameIpBannedIntervalInSeconds + 1 );
        }

        public bool IsRequestValid()
        {
            var requestMarker = GetCurrentRequestMarker();
            if ( requestMarker != null )
            {
                return (_Config.SameIpMaxRequestInInterval > requestMarker.Count);
            }
            return (true);
        }
        public bool IsNeedRedirectOnCaptchaIfRequestNotValid()
        {
            if ( !IsRequestValid() )
            {
                var requestMarker = GetCurrentRequestMarker();
                if ( requestMarker != null )
                {
                    //if ( requestMarker.IsBanned )
                    //{
                        //---------------------------//
                        //requestMarker.CountIncrement();
                        //BannedRequest( requestMarker );
                        //---------------------------//

                        //_Config.HttpContext.Response.Redirect( _Config.CaptchaPageUrl, true );
                    //}
                    //else
                    //{
                        var waitRemainSeconds = requestMarker.GetWaitRemainSeconds( ref _Config );
                        if ( waitRemainSeconds > 3 )  
                        {
                            BannedRequest( requestMarker );

                            return (true); // _Config.HttpContext.Response.Redirect( _Config.CaptchaPageUrl, true );
                        }
                        else
                        {
                            Thread.Sleep( Math.Max( 1, waitRemainSeconds ) * 1000 );
                        }
                    //}
                }
            }
            return (false);
        }
        public void RedirectOnCaptchaIfRequestNotValid()
        {
            if ( !IsRequestValid() )
            {
                var requestMarker = GetCurrentRequestMarker();
                if ( requestMarker != null )
                {
                    //if ( requestMarker.IsBanned )
                    //{
                        //---------------------------//
                        //requestMarker.CountIncrement();
                        //BannedRequest( requestMarker );
                        //---------------------------//

                        //_Config.HttpContext.Response.Redirect( _Config.CaptchaPageUrl, true );
                    //}
                    //else
                    //{
                        var waitRemainSeconds = requestMarker.GetWaitRemainSeconds( ref _Config );
                        if ( waitRemainSeconds > 3 )  
                        {
                            BannedRequest( requestMarker );

                            _Config.HttpContext.Response.Redirect( _Config.CaptchaPageUrl, true );
                        }
                        else
                        {
                            Thread.Sleep( Math.Max( 1, waitRemainSeconds ) * 1000 );
                        }
                    //}
                }
            }
        }
        public void MarkRequest()
        {
            var requestMarker = GetCurrentRequestMarker();
            if ( requestMarker != null )
            {
                requestMarker.CountIncrement();
            }
            else
            {
                requestMarker = new RequestMarker();

                PutCurrentRequestMarker2Cache( requestMarker, _Config.SameIpIntervalRequestInSeconds );
            }
        }
        public void MakeAllowRequests()
        {
            ClearCurrentRequestMarkerInCache();
        }
        public int  GetWaitRemainSeconds()
        {
            var requestMarker = GetCurrentRequestMarker();
            if ( requestMarker != null )
            {
                return (requestMarker.GetWaitRemainSeconds( ref _Config ));
            }
            return (0);
        }

        public void SendGotoOnCaptchaJsonResponse()
        {
            _Config.HttpContext.Response.ContentType = "application/json";
            _Config.HttpContext.Response.Write( "{ \"err\": \"goto-on-captcha\" }" );
            //_Config.HttpContext.Response.End();
        }
    }
}
