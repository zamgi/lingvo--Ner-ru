using System.Configuration;

using lingvo.ner;
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

        public static readonly int    MAX_INPUTTEXT_LENGTH                 = int.Parse( ConfigurationManager.AppSettings[ "MAX_INPUTTEXT_LENGTH" ] );
        public static readonly int    CONCURRENT_FACTORY_INSTANCE_COUNT    = int.Parse( ConfigurationManager.AppSettings[ "CONCURRENT_FACTORY_INSTANCE_COUNT" ] );
        public static readonly int    SAME_IP_INTERVAL_REQUEST_IN_SECONDS  = int.Parse( ConfigurationManager.AppSettings[ "SAME_IP_INTERVAL_REQUEST_IN_SECONDS" ] );
        public static readonly int    SAME_IP_MAX_REQUEST_IN_INTERVAL      = int.Parse( ConfigurationManager.AppSettings[ "SAME_IP_MAX_REQUEST_IN_INTERVAL" ] );        
        public static readonly int    SAME_IP_BANNED_INTERVAL_IN_SECONDS   = int.Parse( ConfigurationManager.AppSettings[ "SAME_IP_BANNED_INTERVAL_IN_SECONDS" ] );
    }
}