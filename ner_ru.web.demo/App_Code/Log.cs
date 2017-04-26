using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using log4net;
using Newtonsoft.Json;

namespace lingvo
{
    /// <summary>
    /// 
    /// </summary>
    public static class Log
    {
        static Log()
        {
            #region [.config 'log4net'.]
            try
            {
                log4net.Config.XmlConfigurator.Configure();
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
            #endregion
        }

        private static void SendTextJson( this HttpResponse response, string text )
        {
            response.ContentType = "application/json";
            var json = JsonConvert.SerializeObject( new { text = text } );
            response.Write( json );
        }
        private static void SendTextFile( this HttpResponse response, string filename )
        {
            response.ContentType = "text/plain; charset=utf-8";
            response.ContentEncoding = Encoding.UTF8;
            response.WriteFile( filename );
        }
        private static bool TryToBool( this string value, bool defaultValue )
        {
            bool result;
            return (bool.TryParse( value, out result ) ? result : defaultValue);
        }
        public static bool ProcessViewCommand( HttpContext context )
        {
            #region [.view.]
            var viewLog = context.Request[ "viewLog" ].TryToBool( false );
            if ( viewLog )
            {
                try
                {
                    context.Response.SendTextFile( context.Server.MapPath( "~/(logs)/all.txt" ) );
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }                
                return (true);
            }
            #endregion

            #region [.delete.]
            var deleteLog = context.Request[ "deleteLog" ].TryToBool( false );
            if ( deleteLog )
            {
                try
                {
                    Directory.Delete( context.Server.MapPath( "~/(logs)" ), true );
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
                context.Response.SendTextJson( "bull-shit-mother-fucka" );
                return (true);
            }
            #endregion

            return (false);
        }

        public static void Info( HttpContext context, string text )
        {
            if ( string.Compare( AntiBotHelper.LOAD_MODEL_DUMMY_TEXT, text, true ) == 0 )
            {
                return;
            }

            try
            {
                var message = string.Format( "IP: '{0}', TEXT: '{1}'", (context.Request.UserHostName ?? context.Request.UserHostAddress), text );
                LogManager.GetLogger( string.Empty ).Info( message );
            }
            catch
            {
                ;
            }
        }
        public static void Error( HttpContext context, string text, Exception ex )
        {
            try
            {
                var message = string.Format( "IP: '{0}', TEXT: '{1}'", (context.Request.UserHostName ?? context.Request.UserHostAddress), text );
                LogManager.GetLogger( string.Empty ).Error( message, ex );
            }
            catch
            {
                ;
            }
        }

        public static void Info( string message )
        {
            LogManager.GetLogger( string.Empty ).Info( message );
        }
        public static void Error( string message, Exception ex )
        {
            LogManager.GetLogger( string.Empty ).Error( message, ex );
        }
    }
}