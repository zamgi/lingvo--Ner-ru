using lingvo.sentsplitting;
using lingvo.tokenizing;

namespace lingvo.ner
{
    /// <summary>
    /// 
    /// </summary>
    public interface INerEnvironmentConfig
    {
        string URL_DETECTOR_RESOURCES_XML_FILENAME  { get; }
        string SENT_SPLITTER_RESOURCES_XML_FILENAME { get; }
        string TOKENIZER_RESOURCES_XML_FILENAME     { get; }
        string NER_MODEL_FILENAME                   { get; }
        string NER_TEMPLATE_FILENAME                { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class NerEnvironmentConfigBase : INerEnvironmentConfig
    {
        public abstract string URL_DETECTOR_RESOURCES_XML_FILENAME  { get; }
        public abstract string SENT_SPLITTER_RESOURCES_XML_FILENAME { get; }
        public abstract string TOKENIZER_RESOURCES_XML_FILENAME     { get; }
        public abstract string NER_MODEL_FILENAME                   { get; }
        public abstract string NER_TEMPLATE_FILENAME                { get; }


        public (NerProcessorConfig config, SentSplitterConfig ssc) CreateNerProcessorConfig( LanguageTypeEnum languageType )
        {
            var sentSplitterConfig = new SentSplitterConfig( SENT_SPLITTER_RESOURCES_XML_FILENAME, URL_DETECTOR_RESOURCES_XML_FILENAME );
            var config = new NerProcessorConfig( TOKENIZER_RESOURCES_XML_FILENAME, languageType, sentSplitterConfig )
            {
                ModelFilename    = NER_MODEL_FILENAME,
                TemplateFilename = NER_TEMPLATE_FILENAME,
            };
            return (config, sentSplitterConfig);
        }

        //public (NerProcessorConfig config, SentSplitterConfig ssc) CreateNerProcessorConfig_Ru() => CreateNerProcessorConfig( LanguageTypeEnum.Ru );
        //public (NerProcessorConfig config, SentSplitterConfig ssc) CreateNerProcessorConfig_En() => CreateNerProcessorConfig( LanguageTypeEnum.En );
    }
}
