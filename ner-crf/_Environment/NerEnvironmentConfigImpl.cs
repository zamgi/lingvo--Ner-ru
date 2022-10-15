using System.Configuration;

namespace lingvo.ner
{
    /// <summary>
    /// 
    /// </summary>
    public class NerEnvironmentConfigImpl : NerEnvironmentConfigBase
    {
        public NerEnvironmentConfigImpl()
        {
            URL_DETECTOR_RESOURCES_XML_FILENAME  = ConfigurationManager.AppSettings[ "URL_DETECTOR_RESOURCES_XML_FILENAME"  ];
            SENT_SPLITTER_RESOURCES_XML_FILENAME = ConfigurationManager.AppSettings[ "SENT_SPLITTER_RESOURCES_XML_FILENAME" ];
            TOKENIZER_RESOURCES_XML_FILENAME     = ConfigurationManager.AppSettings[ "TOKENIZER_RESOURCES_XML_FILENAME"     ];

            NER_MODEL_FILENAME                   = ConfigurationManager.AppSettings[ "NER_MODEL_FILENAME"    ];
            NER_TEMPLATE_FILENAME                = ConfigurationManager.AppSettings[ "NER_TEMPLATE_FILENAME" ];
        }

        public override string URL_DETECTOR_RESOURCES_XML_FILENAME  { get; }
        public override string SENT_SPLITTER_RESOURCES_XML_FILENAME { get; }
        public override string TOKENIZER_RESOURCES_XML_FILENAME     { get; }

        public override string NER_MODEL_FILENAME                   { get; }
        public override string NER_TEMPLATE_FILENAME                { get; }

    }
}
