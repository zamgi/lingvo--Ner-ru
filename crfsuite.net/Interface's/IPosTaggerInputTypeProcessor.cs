using lingvo.tokenizing;

namespace lingvo.postagger
{
    /// <summary>
    /// 
    /// </summary>
    public struct PosTaggerInputTypeResult
    {
        private PosTaggerInputTypeResult( PosTaggerInputType ptt )
        {
            posTaggerInputType                                   = ptt;
            posTaggerExtraWordType                               = PosTaggerExtraWordType.__DEFAULT__;
            posTaggerLastValueUpperInNumeralChain                = null;
            posTaggerLastValueUpperInNumeralChainIsValueOriginal = false;
        }
        private PosTaggerInputTypeResult( PosTaggerInputType ptt, PosTaggerExtraWordType extraWordType )
        {
            posTaggerInputType                                   = ptt;
            posTaggerExtraWordType                               = extraWordType;
            posTaggerLastValueUpperInNumeralChain                = null;
            posTaggerLastValueUpperInNumeralChainIsValueOriginal = false;
        }

        public string                 posTaggerLastValueUpperInNumeralChain;
        public bool                   posTaggerLastValueUpperInNumeralChainIsValueOriginal;
        public PosTaggerInputType     posTaggerInputType;
        public PosTaggerExtraWordType posTaggerExtraWordType;

        public static readonly PosTaggerInputTypeResult Num    = new PosTaggerInputTypeResult( PosTaggerInputType.Num    );
        public static readonly PosTaggerInputTypeResult AllLat = new PosTaggerInputTypeResult( PosTaggerInputType.AllLat );
        public static readonly PosTaggerInputTypeResult Col    = new PosTaggerInputTypeResult( PosTaggerInputType.Col,  PosTaggerExtraWordType.Punctuation );
        public static readonly PosTaggerInputTypeResult Com    = new PosTaggerInputTypeResult( PosTaggerInputType.Com,  PosTaggerExtraWordType.Punctuation );
        public static readonly PosTaggerInputTypeResult Dush   = new PosTaggerInputTypeResult( PosTaggerInputType.Dush, PosTaggerExtraWordType.Punctuation );
        public static readonly PosTaggerInputTypeResult FstC   = new PosTaggerInputTypeResult( PosTaggerInputType.FstC  );
        public static readonly PosTaggerInputTypeResult OneCP  = new PosTaggerInputTypeResult( PosTaggerInputType.OneCP );
        public static readonly PosTaggerInputTypeResult O      = new PosTaggerInputTypeResult( PosTaggerInputType.O     );
        public static readonly PosTaggerInputTypeResult IsAbbreviation = new PosTaggerInputTypeResult( PosTaggerInputType.O, PosTaggerExtraWordType.Abbreviation );
        public static readonly PosTaggerInputTypeResult IsPunctuation  = new PosTaggerInputTypeResult( PosTaggerInputType.O, PosTaggerExtraWordType.Punctuation  );

        public static PosTaggerInputTypeResult CreateNum( string _posTaggerLastValueUpperInNumeralChain ) => new PosTaggerInputTypeResult( PosTaggerInputType.Num ) { posTaggerLastValueUpperInNumeralChain = _posTaggerLastValueUpperInNumeralChain, };
        private static readonly PosTaggerInputTypeResult _Num_1 = new PosTaggerInputTypeResult( PosTaggerInputType.Num ) { posTaggerLastValueUpperInNumeralChainIsValueOriginal = true };
        public static PosTaggerInputTypeResult CreateNum() => _Num_1;
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IPosTaggerInputTypeProcessor
    {
        /// <summary>
        /// word.valueOriginal & word.valueUpper are used
        /// </summary>
        /// <param name="word">word.valueOriginal & word.valueUpper are used</param>
        unsafe PosTaggerInputTypeResult GetResult( char* _base, int length, word_t word ); //string valueUpper );
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IPosTaggerInputTypeProcessorFactory
    {
        IPosTaggerInputTypeProcessor CreateInstance();
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class Dummy_PosTaggerInputTypeProcessor : IPosTaggerInputTypeProcessor
    {
        public static Dummy_PosTaggerInputTypeProcessor Inst { get; } = new Dummy_PosTaggerInputTypeProcessor();
        private Dummy_PosTaggerInputTypeProcessor() { }

        public unsafe PosTaggerInputTypeResult GetResult( char* _base, int length, word_t word ) => PosTaggerInputTypeResult.O;
    }
}
