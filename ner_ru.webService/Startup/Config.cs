using System.Configuration;

using captcha;
using lingvo.sentsplitting;
using lingvo.tokenizing;

namespace lingvo.ner.webService
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConfig : IAntiBotConfig
    {
        int CONCURRENT_FACTORY_INSTANCE_COUNT { get; }

        string TOKENIZER_RESOURCES_XML_FILENAME     { get; }
        string NER_MODEL_FILENAME                   { get; }
        string NER_TEMPLATE_FILENAME                { get; }
        string SENT_SPLITTER_RESOURCES_XML_FILENAME { get; }
        string URL_DETECTOR_RESOURCES_XML_FILENAME  { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class Config : IConfig, IAntiBotConfig
    {
        public Config() { }

        public int? SameIpBannedIntervalInSeconds  { get; } = int.TryParse( ConfigurationManager.AppSettings[ "SAME_IP_BANNED_INTERVAL_IN_SECONDS"  ], out var i ) ? i : null;
        public int? SameIpIntervalRequestInSeconds { get; } = int.TryParse( ConfigurationManager.AppSettings[ "SAME_IP_INTERVAL_REQUEST_IN_SECONDS" ], out var i ) ? i : null;
        public int? SameIpMaxRequestInInterval     { get; } = int.TryParse( ConfigurationManager.AppSettings[ "SAME_IP_MAX_REQUEST_IN_INTERVAL"     ], out var i ) ? i : null;
        public string CaptchaPageTitle => "Определение именованных сущностей (NER) в тексте на русском языке";


        public int CONCURRENT_FACTORY_INSTANCE_COUNT { get; } = int.Parse( ConfigurationManager.AppSettings[ "CONCURRENT_FACTORY_INSTANCE_COUNT" ] );
        //public int MAX_INPUTTEXT_LENGTH { get; } = ConfigurationManager.AppSettings[ "MAX_INPUTTEXT_LENGTH" ].ToInt32();


        public string TOKENIZER_RESOURCES_XML_FILENAME     { get; } = ConfigurationManager.AppSettings[ "TOKENIZER_RESOURCES_XML_FILENAME" ];
        public string NER_MODEL_FILENAME                   { get; } = ConfigurationManager.AppSettings[ "NER_MODEL_FILENAME" ];
        public string NER_TEMPLATE_FILENAME                { get; } = ConfigurationManager.AppSettings[ "NER_TEMPLATE_FILENAME" ];
        public string SENT_SPLITTER_RESOURCES_XML_FILENAME { get; } = ConfigurationManager.AppSettings[ "SENT_SPLITTER_RESOURCES_XML_FILENAME" ];
        public string URL_DETECTOR_RESOURCES_XML_FILENAME  { get; } = ConfigurationManager.AppSettings[ "URL_DETECTOR_RESOURCES_XML_FILENAME" ];

        public (NerProcessorConfig config, SentSplitterConfig ssc) CreateNerProcessorConfig()
        {
            var sentSplitterConfig = new SentSplitterConfig( SENT_SPLITTER_RESOURCES_XML_FILENAME, URL_DETECTOR_RESOURCES_XML_FILENAME );
            var config = new NerProcessorConfig( TOKENIZER_RESOURCES_XML_FILENAME, LanguageTypeEnum.Ru, sentSplitterConfig )
            {
                ModelFilename    = NER_MODEL_FILENAME,
                TemplateFilename = NER_TEMPLATE_FILENAME,
            };
            return (config, sentSplitterConfig);
        }
    }
}
