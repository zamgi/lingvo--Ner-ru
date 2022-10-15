using lingvo.sentsplitting;
using lingvo.tokenizing;
using lingvo.urls;

namespace lingvo.ner
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class NerProcessorConfigBase
    {
        public string TemplateFilename { get; set; }
        public abstract LanguageTypeEnum LanguageType { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class NerProcessorConfig : NerProcessorConfigBase
    {
        public NerProcessorConfig( string             tokenizerResourcesXmlFilename, 
                                   LanguageTypeEnum   languageType,
                                   SentSplitterConfig sentSplitterConfig )
        {
            TokenizerConfig = new TokenizerConfig( tokenizerResourcesXmlFilename )
            {
                TokenizeMode                 = TokenizeMode.Ner,
                SentSplitterConfig           = sentSplitterConfig,
                NerInputTypeProcessorFactory = new NerInputTypeProcessorFactory( languageType ),
                LanguageType                 = languageType,
            };
        }
        public NerProcessorConfig( TokenizerConfig  tokenizerConfig )
        {
            TokenizerConfig = tokenizerConfig;

            //set ner specially
            TokenizerConfig.TokenizeMode |= TokenizeMode.Ner;
            if ( TokenizerConfig.NerInputTypeProcessorFactory == null )
            {
                TokenizerConfig.NerInputTypeProcessorFactory = new NerInputTypeProcessorFactory( TokenizerConfig.LanguageType );
            }
        }

        public string          ModelFilename   { get; set; }
        public TokenizerConfig TokenizerConfig { get; }

        public override LanguageTypeEnum LanguageType => TokenizerConfig.LanguageType;
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class NerModelBuilderConfig : NerProcessorConfigBase
    {
        public NerModelBuilderConfig( string            tokenizerResourcesXmlFilename,
                                      LanguageTypeEnum  languageType,
                                      UrlDetectorConfig urlDetectorConfig )
        {
            TokenizerConfig4NerModelBuilder = new TokenizerConfig4NerModelBuilder( tokenizerResourcesXmlFilename )
            {
                UrlDetectorConfig            = urlDetectorConfig,
                NerInputTypeProcessorFactory = new NerInputTypeProcessorFactory( languageType ),
                LanguageType                 = languageType,
            };
        }

        public TokenizerConfig4NerModelBuilder TokenizerConfig4NerModelBuilder { get; }
        public bool                            IgnoreXmlError                  { get; set; }

        public override LanguageTypeEnum LanguageType => TokenizerConfig4NerModelBuilder.LanguageType;
    }
}
