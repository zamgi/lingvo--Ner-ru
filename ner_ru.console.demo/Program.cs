using System;
using System.Configuration;

using lingvo.sentsplitting;
using lingvo.tokenizing;

namespace lingvo
{
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
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class Ext
    {
        public static T ToEnum< T >( this string value ) where T : struct
        {
            var e = (T) Enum.Parse( typeof( T ), value, true );
            return (e);
        }
    }
}

namespace lingvo.ner
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class Program
    {
        private static void Main( string[] args )
        {
            try
            {
                var text = @"Сергей Собянин напомнил, что в 2011 году в Москве были 143 млрд. руб. приняты масштабные программы развития города, в том числе программа ""Безопасный город"" на пять лет, на которую будет выделено финансирование в размере 143 млрд. рублей.";
                //---var text = System.IO.File.ReadAllText( @"C:\1.txt" );

                ProcessText( text );
            }
            catch ( Exception ex )
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( Environment.NewLine + ex + Environment.NewLine );
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine( "  [.......finita fusking comedy.......]" );
            Console.ReadLine();
        }

        private static NerProcessorConfig CreateNerProcessorConfig()
        {
            var sentSplitterConfig = new SentSplitterConfig( Config.SENT_SPLITTER_RESOURCES_XML_FILENAME,
                                                             Config.URL_DETECTOR_RESOURCES_XML_FILENAME );
            var config = new NerProcessorConfig( Config.TOKENIZER_RESOURCES_XML_FILENAME, 
                                                 Config.LANGUAGE_TYPE, sentSplitterConfig )
            {
                ModelFilename    = Config.NER_MODEL_FILENAME,
                TemplateFilename = Config.NER_TEMPLATE_FILENAME,
            };
            return (config);
        }

        private static void ProcessText( string text, bool splitBySmiles = true )
        {
            var config = CreateNerProcessorConfig();

            using ( var nerProcessor = new NerProcessor( config ) )
            {
                Console.WriteLine( "\r\n-------------------------------------------------\r\n text: '" + text + '\'' );

                var result = nerProcessor.Run( text, splitBySmiles );

                Console.WriteLine( "-------------------------------------------------\r\n ner-entity-count: " + result.Count + Environment.NewLine );
                foreach ( var word in result )
                {
                    Console.WriteLine( word );
                }
                Console.WriteLine();
                
                Console.WriteLine( "-------------------------------------------------\r\n" );
            }
        }
    }
}
