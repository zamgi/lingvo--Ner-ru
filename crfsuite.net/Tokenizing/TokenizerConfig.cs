using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using lingvo.core;
using lingvo.ner;
using lingvo.postagger;
using lingvo.sentsplitting;
using lingvo.urls;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace lingvo.tokenizing
{
    /// <summary>
    /// 
    /// </summary>
    [Flags] public enum TokenizeMode
    {
        __UNKNOWN__ = 0x0,

        PosTagger = 0x1,
        Ner       = (1 << 1),
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class TokenizerModel
    {
        public TokenizerModel( string tokenizerResourcesXmlFilename )
        {
            var xdoc = XDocument.Load( tokenizerResourcesXmlFilename );

            const string PARTICLE_THAT = "ТО";
            var hyphenChars = xlat.CHARTYPE_MAP
                                  .Select( (ct, ch) => ( ct, ch: (char) ch ) )
                                  .Where( t => (t.ct & CharType.IsHyphen) == CharType.IsHyphen )
                                  .Select( t => t.ch );
            var particleThats = hyphenChars.Select( ch => ch + PARTICLE_THAT )
                                           .ToArray();
            var particleThatLength = (PARTICLE_THAT.Length + 1);
            var particleThatExclusion = from xe in xdoc.Root.Element( "particle-that-exclusion-list" ).Elements()
                                          let _v = xe.Value.ToUpperInvariant().Trim()
                                          where particleThats.Any( _pt => _v.EndsWith( _pt ) )
                                          let v = _v.Substring( 0, _v.Length - particleThatLength ).Trim()                                        
                                          //let v = xe.Value.Trim().ToUpperInvariant().Replace( " ", string.Empty )
                                          where !v.IsNullOrEmpty()
                                        from pt in particleThats
                                        select 
                                            (v + pt);
            ParticleThatExclusion = new HashSet< string >( particleThatExclusion );
        }

        public HashSet< string > ParticleThatExclusion { [M(O.AggressiveInlining)] get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class TokenizerConfig
    {
        public TokenizerConfig( string tokenizerResourcesXmlFilename ) => Model = new TokenizerModel( tokenizerResourcesXmlFilename );

        public SentSplitterConfig SentSplitterConfig { get; set; }
        public TokenizerModel     Model              { get; set; }

        public TokenizeMode     TokenizeMode { get; set; }
        public LanguageTypeEnum LanguageType { get; set; }

        public IPosTaggerInputTypeProcessorFactory PosTaggerInputTypeProcessorFactory { get; set; }
        public INerInputTypeProcessorFactory       NerInputTypeProcessorFactory       { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class TokenizerConfig4NerModelBuilder
    {
        public TokenizerConfig4NerModelBuilder( string tokenizerResourcesXmlFilename ) => Model = new TokenizerModel( tokenizerResourcesXmlFilename );

        public TokenizerModel    Model             { get; }
        public UrlDetectorConfig UrlDetectorConfig { get; set; }
        public LanguageTypeEnum  LanguageType      { get; set; }
        public INerInputTypeProcessorFactory NerInputTypeProcessorFactory { get; set; }
    }
}
