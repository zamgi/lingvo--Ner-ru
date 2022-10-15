using System;
using System.Diagnostics;

using lingvo.sentsplitting;
using lingvo.tokenizing;

namespace lingvo.ner
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class NerEnvironment : IDisposable
    {
        private NerEnvironment() { }
        public void Dispose()
        {
            if ( SentSplitterConfig != null )
            {
                SentSplitterConfig.Dispose();
                SentSplitterConfig = null;
            }
        }

        public NerEnvironmentConfigBase NerEnvironmentConfig { get; private set; }
        public  NerProcessorConfig NerProcessorConfig { get; private set; }
        private SentSplitterConfig SentSplitterConfig { get; set; }

        public NerProcessor CreateNerProcessor() => new NerProcessor( NerProcessorConfig );

        public static NerEnvironment Create( NerEnvironmentConfigBase opts, LanguageTypeEnum languageType, bool print2Console = true )
        {
            var sw = default(Stopwatch);
            if ( print2Console )
            {
                sw = Stopwatch.StartNew();
                Console.Write( "init ner-environment..." );
            }

            var (config, ssc) = opts.CreateNerProcessorConfig( languageType );

            var env = new NerEnvironment()
            {
                SentSplitterConfig   = ssc,
                NerProcessorConfig   = config,
                NerEnvironmentConfig = opts,
            };

            if ( print2Console )
            {
                sw.Stop();
                Console.WriteLine( $"end, (elapsed: {sw.Elapsed}).\r\n----------------------------------------------------\r\n" );
            }

            return (env);
        }
        public static NerEnvironment Create( LanguageTypeEnum languageType, bool print2Console = true ) => Create( new NerEnvironmentConfigImpl(), languageType, print2Console );
    }
}
