using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using lingvo.sentsplitting;
using lingvo.tokenizing;
using Newtonsoft.Json;

namespace lingvo.ner
{
    /// <summary>
    /// 
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
        private static class ConcurrentFactoryHelper
        {
            private static readonly object _SyncLock = new object();

            private static ConcurrentFactory _ConcurrentFactory;

            public static ConcurrentFactory GetConcurrentFactory()
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
            #region [.log.]
            if ( Log.ProcessViewCommand( context ) )
            {
                return;
            }
            #endregion

            var text = default(string);
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

                    text          = context.GetRequestStringParam( "text", Config.MAX_INPUTTEXT_LENGTH );
                var splitBySmiles = context.Request[ "splitBySmiles" ].Try2Bool( true );
                var html          = context.Request[ "html"          ].Try2Bool( false );                

                #region [.anti-bot.]
                antiBot.MarkRequestEx( text );
                #endregion

                var words = ConcurrentFactoryHelper.GetConcurrentFactory().Run( text, splitBySmiles );

                Log.Info( context, text );
                SendJsonResponse( context, words, text, html );
            }
            catch ( Exception ex )
            {
                Log.Error( context, text, ex );
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
}