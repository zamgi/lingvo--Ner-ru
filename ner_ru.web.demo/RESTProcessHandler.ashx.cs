using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;

using lingvo.sentsplitting;
using lingvo.tokenizing;
using Newtonsoft.Json;

namespace lingvo
{
    using lingvo.ner;

    /// <summary>
    /// 
    /// </summary>
    internal static class Config
    {
        public static readonly string URL_DETECTOR_RESOURCES_XML_FILENAME  = ConfigurationManager.AppSettings[ "URL_DETECTOR_RESOURCES_XML_FILENAME"  ];
        public static readonly string SENT_SPLITTER_RESOURCES_XML_FILENAME = ConfigurationManager.AppSettings[ "SENT_SPLITTER_RESOURCES_XML_FILENAME" ];
        public static readonly string TOKENIZER_RESOURCES_XML_FILENAME     = ConfigurationManager.AppSettings[ "TOKENIZER_RESOURCES_XML_FILENAME" ];
        public static readonly string NER_MODEL_FILENAME                   = ConfigurationManager.AppSettings[ "NER_MODEL_FILENAME" ];
        public static readonly string NER_TEMPLATE_FILENAME                = ConfigurationManager.AppSettings[ "NER_TEMPLATE_FILENAME" ];
        public static readonly LanguageTypeEnum LANGUAGE_TYPE              = ConfigurationManager.AppSettings[ "LANGUAGE_TYPE" ].ToEnum< LanguageTypeEnum >();

        public static readonly int    MAX_INPUTTEXT_LENGTH                 = int.Parse( ConfigurationManager.AppSettings[ "MAX_INPUTTEXT_LENGTH" ] );
        public static readonly int    CONCURRENT_FACTORY_INSTANCE_COUNT    = int.Parse( ConfigurationManager.AppSettings[ "CONCURRENT_FACTORY_INSTANCE_COUNT" ] );
        public static readonly int    SAME_IP_INTERVAL_REQUEST_IN_SECONDS  = int.Parse( ConfigurationManager.AppSettings[ "SAME_IP_INTERVAL_REQUEST_IN_SECONDS" ] );
        public static readonly int    SAME_IP_MAX_REQUEST_IN_INTERVAL      = int.Parse( ConfigurationManager.AppSettings[ "SAME_IP_MAX_REQUEST_IN_INTERVAL" ] );        
        public static readonly int    SAME_IP_BANNED_INTERVAL_IN_SECONDS   = int.Parse( ConfigurationManager.AppSettings[ "SAME_IP_BANNED_INTERVAL_IN_SECONDS" ] );
    }
}

namespace lingvo.ner
{
    /// <summary>
    /// Summary description for RESTProcessHandler
    /// </summary>
    public sealed class RESTProcessHandler : IHttpHandler
    {
        /// <summary>
        /// 
        /// </summary>
        private abstract class result_base
        {
            protected result_base()
            {
            }
            protected result_base( Exception ex )
            {
                exceptionMessage = ex.ToString();
            }

            [JsonProperty(PropertyName="err")]
            public string exceptionMessage
            {
                get;
                private set;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class result_json : result_base
        {
            /// <summary>
            /// 
            /// </summary>
            public sealed class word_info
            {
                [JsonProperty(PropertyName="i")]   public int    startIndex
                {
                    get;
                    set;
                }
                [JsonProperty(PropertyName="l")]   public int    length
                {
                    get;
                    set;
                }
                [JsonProperty(PropertyName="ner")] public string ner
                {
                    get;
                    set;
                }
                [JsonProperty(PropertyName="v")]   public string value
                {
                    get;
                    set;
                }
            }

            public result_json( Exception ex ) : base( ex )
            {
            }
            public result_json( IList< word_t > _words )
            {
                var word_sb = new StringBuilder();

                words = (from word in _words
                            //let isWordInNerChain = word.IsWordInNerChain
                            //where ( !isWordInNerChain || (isWordInNerChain && word.IsFirstWordInNerChain))
                            where (!word.HasNerPrevWord)
                            select
                                new word_info()
                                {
                                    startIndex = word.startIndex,
                                    length     = word.GetNerLength(),
                                    ner        = word.nerOutputType.ToString(),
                                    value      = word.GetNerValue( word_sb ),
                                }
                        ).ToArray();
            }

            public word_info[] words
            {
                get;
                private set;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private sealed class result_html : result_base
        {
            public result_html( Exception ex ) : base( ex )
            {
            }
            public result_html( IList< word_t > _words, string originalText )
            {
                var sb = new StringBuilder( originalText );

                for ( var i = _words.Count - 1; 0 <= i; i-- )
                {
                    var word = _words[ i ];
                    sb.Insert( word.startIndex + word.length, "</span>" );
                    sb.Insert( word.startIndex, string.Format( "<span class='{0}'>", word.nerOutputType ) );
                }

                sb.Replace( "\r\n", "<br/>" ).Replace( "\n", "<br/>" ).Replace( "\t", "&nbsp;&nbsp;&nbsp;&nbsp;" );

                html = sb.ToString();
            }

            public string html
            {
                get;
                private set;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private struct http_context_data
        {
            private static readonly object _SyncLock = new object();
            private readonly HttpContext _Context;

            public http_context_data( HttpContext context )
            {
                _Context = context;
            }

            /*private ConcurrentFactory _ConcurrentFactory
            {
                get { return ((ConcurrentFactory) _Context.Cache[ "_ConcurrentFactory" ]); }
                set
                {
                    _Context.Cache.Remove( "_ConcurrentFactory" );
                    if ( value != null )
                        _Context.Cache[ "_ConcurrentFactory" ] = value;
                }
            }*/

            private static ConcurrentFactory _ConcurrentFactory;

            public ConcurrentFactory GetConcurrentFactory()
            {
                var f = _ConcurrentFactory;
                if ( f == null )
                {
                    lock ( _SyncLock )
                    {
                        f = _ConcurrentFactory;
                        if ( f == null )
                        {
                            var sentSplitterConfig = new SentSplitterConfig( Config.SENT_SPLITTER_RESOURCES_XML_FILENAME,
                                                                             Config.URL_DETECTOR_RESOURCES_XML_FILENAME );
                            var config = new NerProcessorConfig( Config.TOKENIZER_RESOURCES_XML_FILENAME,
                                                                 Config.LANGUAGE_TYPE,
                                                                 sentSplitterConfig )
                            {
                                ModelFilename    = Config.NER_MODEL_FILENAME,
                                TemplateFilename = Config.NER_TEMPLATE_FILENAME,
                            };
                            f = new ConcurrentFactory( config, Config.CONCURRENT_FACTORY_INSTANCE_COUNT );
                            _ConcurrentFactory = f;
                        }
                    }
                }
                return (f);
            }
        }

        static RESTProcessHandler()
        {
            Environment.CurrentDirectory = HttpContext.Current.Server.MapPath( "~/" );
        }

        public bool IsReusable
        {
            get { return (true); }
        }

        public void ProcessRequest( HttpContext context )
        {
            try
            {
                #region [.anti-bot.]
                var antiBot = context.ToAntiBot();
                if ( antiBot.IsNeedRedirectOnCaptchaIfRequestNotValid() )
                {
                    antiBot.SendGotoOnCaptchaJsonResponse();
                    return;
                }                
                #endregion

                var text          = context.GetRequestStringParam( "text", Config.MAX_INPUTTEXT_LENGTH );
                var splitBySmiles = context.Request[ "splitBySmiles" ].Try2Boolean( true );
                var html          = context.Request[ "html"          ].Try2Boolean( false );                

                #region [.anti-bot.]
                antiBot.MarkRequestEx( text );
                #endregion

                var hcd = new http_context_data( context );
                var words = hcd.GetConcurrentFactory().Run( text, splitBySmiles );

                SendJsonResponse( context, words, text, html );
            }
            catch ( Exception ex )
            {
                SendJsonResponse( context, ex );
            }
        }

        private static void SendJsonResponse( HttpContext context, IList< word_t > words, string originalText, bool html )
        {
            if ( html )
            {
                SendJsonResponse( context, new result_html( words, originalText ) );
            }
            else
            {
                SendJsonResponse( context, new result_json( words ) );
            }
        }
        private static void SendJsonResponse( HttpContext context, Exception ex )
        {
            SendJsonResponse( context, new result_json( ex ) );
        }
        private static void SendJsonResponse( HttpContext context, result_base result )
        {
            context.Response.ContentType = "application/json";
            //---context.Response.Headers.Add( "Access-Control-Allow-Origin", "*" );

            var json = JsonConvert.SerializeObject( result );
            context.Response.Write( json );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        public static bool Try2Boolean( this string value, bool defaultValue )
        {
            if ( value != null )
            {
                var result = default(bool);
                if ( bool.TryParse( value, out result ) )
                    return (result);
            }
            return (defaultValue);
        }

        public static T ToEnum< T >( this string value ) where T : struct
        {
            var result = (T) Enum.Parse( typeof(T), value, true );
            return (result);
        }

        public static string GetRequestStringParam( this HttpContext context, string paramName, int maxLength )
        {
            var value = context.Request[ paramName ];
            if ( (value != null) && (maxLength < value.Length) && (0 < maxLength) )
            {
                return (value.Substring( 0, maxLength ));
            }
            return (value);
        }
    }
}