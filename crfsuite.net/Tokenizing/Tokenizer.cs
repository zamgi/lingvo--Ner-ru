using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using lingvo.core;
using lingvo.ner;
using lingvo.postagger;
using lingvo.sentsplitting;
using lingvo.urls;

namespace lingvo.tokenizing
{
    /// <summary>
    /// 
    /// </summary>
    unsafe sealed public class Tokenizer : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        [Flags]
        private enum CRFCharType : byte
        {
            __UNDEFINE__                = 0x0,

            InterpreteAsWhitespace      = 0x1,
            BetweenLetterOrDigit        = (1 << 1),
            BetweenDigit                = (1 << 2),
            TokenizeDifferentSeparately = (1 << 3),
            DotChar                     = (1 << 4),
        }

        /// <summary>
        /// 
        /// </summary>
        unsafe private sealed class UnsafeConst
        {
            #region [.static & xlat table's.]
            public  static readonly char*  MAX_PTR                          = (char*) (0xffffffffFFFFFFFF);
            private const string           INCLUDE_INTERPRETE_AS_WHITESPACE = "¤¦§¶"; //"¥©¤¦§®¶€™<>";
            private const char             DOT                              = '\u002E'; /* 0x2E, 46, '.' */
            private static readonly char[] BETWEEN_LETTER_OR_DIGIT          = new char[] { 
                                                                                        '\u0026', /* 0x26  , 38  , '&' */
                                                                                        '\u0027', /* 0x27  , 39  , ''' */
                                                                                        '\u002D', /* 0x2D  , 45  , '-' */
                                                                                        '\u005F', /* 0x5F  , 95  , '_' */
                                                                                        '\u00AD', /* 0xAD  , 173 , '­' */
                                                                                        '\u055A', /* 0x55A , 1370, '՚' */
                                                                                        '\u055B', /* 0x55B , 1371, '՛' */
                                                                                        '\u055D', /* 0x55D , 1373, '՝' */
                                                                                        '\u2012', /* 0x2012, 8210, '‒' */
                                                                                        '\u2013', /* 0x2013, 8211, '–' */
                                                                                        '\u2014', /* 0x2014, 8212, '—' */
                                                                                        '\u2015', /* 0x2015, 8213, '―' */
                                                                                        '\u2018', /* 0x2018, 8216, '‘' */
                                                                                        '\u2019', /* 0x2019, 8217, '’' */
                                                                                        '\u201B', /* 0x201B, 8219, '‛' */
                                                                                      };
            private static readonly char[] BETWEEN_LETTER_OR_DIGIT_EN       = new char[] { 
                                                                                        '\u0026', /* 0x26  , 38  , '&' */
                                                                                      //'\u0027', /* 0x27  , 39  , ''' */
                                                                                        '\u002D', /* 0x2D  , 45  , '-' */
                                                                                        '\u005F', /* 0x5F  , 95  , '_' */
                                                                                        '\u00AD', /* 0xAD  , 173 , '­' */
                                                                                      //'\u055A', /* 0x55A , 1370, '՚' */
                                                                                      //'\u055B', /* 0x55B , 1371, '՛' */
                                                                                      //'\u055D', /* 0x55D , 1373, '՝' */
                                                                                        '\u2012', /* 0x2012, 8210, '‒' */
                                                                                        '\u2013', /* 0x2013, 8211, '–' */
                                                                                        '\u2014', /* 0x2014, 8212, '—' */
                                                                                        '\u2015', /* 0x2015, 8213, '―' */
                                                                                        '\u2018', /* 0x2018, 8216, '‘' */
                                                                                      //'\u2019', /* 0x2019, 8217, '’' */
                                                                                        '\u201B', /* 0x201B, 8219, '‛' */
                                                                                      };
            private static readonly char[] BETWEEN_DIGIT                 = new char[] { 
                                                                                         '\u0022', /* 0x22   , 34   , '"'  */
                                                                                         '\u002C', /* 0x2C   , 44   , ','  */
                                                                                         '\u003A', /* 0x3A   , 58   , ':'  */
                                                                                         '\u3003', /* 0x3003 , 12291, '〃' */
                                                                                         //-ERROR-!!!-DOT, /* и  0x2E   , 46   , '.' - хотя это и так работает */
                                                                                      };
            private static readonly char[] TOKENIZE_DIFFERENT_SEPARATELY = new char[] {             
                                                                                        '\u2012', /* 0x2012 , 8210 , '‒' */
                                                                                        '\u2013', /* 0x2013 , 8211 , '–' */
                                                                                        '\u2014', /* 0x2014 , 8212 , '—' */
                                                                                        '\u2015', /* 0x2015 , 8213 , '―' */
                                                                                        '\u2018', /* 0x2018 , 8216 , '‘' */
                                                                                        '\u2019', /* 0x2019 , 8217 , '’' */
                                                                                        '\u201B', /* 0x201B , 8219 , '‛' */
                                                                                        '\u201C', /* 0x201C , 8220 , '“' */
                                                                                        '\u201D', /* 0x201D , 8221 , '”' */
                                                                                        '\u201E', /* 0x201E , 8222 , '„' */
                                                                                        '\u201F', /* 0x201F , 8223 , '‟' */
                                                                                        '\u2026', /* 0x2026 , 8230 , '…' */
                                                                                        '\u0021', /* 0x21   , 33   , '!' */
                                                                                        '\u0022', /* 0x22   , 34   , '"' */
                                                                                        '\u0026', /* 0x26   , 38   , '&' */
                                                                                        '\u0027', /* 0x27   , 39   , ''' */
                                                                                        '\u0028', /* 0x28   , 40   , '(' */
                                                                                        '\u0029', /* 0x29   , 41   , ')' */
                                                                                        '\u002C', /* 0x2C   , 44   , ',' */
                                                                                        '\u002D', /* 0x2D   , 45   , '-' */
                                                                                        //DOT, //'\u002E', /* 0x2E   , 46   , '.' */
                                                                                        '\u3003', /* 0x3003 , 12291, '〃' */
                                                                                        '\u003A', /* 0x3A   , 58   , ':' */
                                                                                        '\u003B', /* 0x3B   , 59   , ';' */
                                                                                        '\u003F', /* 0x3F   , 63   , '?' */
                                                                                        '\u055A', /* 0x55A  , 1370 , '՚' */
                                                                                        '\u055B', /* 0x55B  , 1371 , '՛'  */
                                                                                        '\u055D', /* 0x55D  , 1373 , '՝' */
                                                                                        '\u005B', /* 0x5B   , 91   , '[' */
                                                                                        '\u005D', /* 0x5D   , 93   , ']' */
                                                                                        '\u005F', /* 0x5F   , 95   , '_' */
                                                                                        '\u05F4', /* 0x5F4  , 1524 , '״' */
                                                                                        '\u007B', /* 0x7B   , 123  , '{' */
                                                                                        '\u007D', /* 0x7D   , 125  , '}' */
                                                                                        '\u00A1', /* 0xA1   , 161  , '¡' */
                                                                                        '\u00AB', /* 0xAB   , 171  , '«' */
                                                                                        '\u00AD', /* 0xAD   , 173  , '­' */
                                                                                        '\u00BB', /* 0xBB   , 187  , '»' */
                                                                                        '\u00BF', /* 0xBF   , 191  , '¿' */
                                                                                        '/',
                                                                                        '¥', '©', '®', '€', '™', '°', '№', '$', '%',
                                                                                        '<', '>',
                                                                                      };
            #endregion

            public readonly CRFCharType* _CRF_CHARTYPE_MAP;

            private UnsafeConst( LanguageTypeEnum languageType )
            {
                //-1-//
                var CRF_CHARTYPE_MAP = new byte/*CRFCharType*/[ char.MaxValue + 1 ];
                fixed ( /*CRFCharType*/byte* cctm = CRF_CHARTYPE_MAP )        
                {
                    for ( var c = char.MinValue; /*c <= char.MaxValue*/; c++ )
                    {
                        if ( /*char.IsWhiteSpace( c ) ||*/ char.IsPunctuation( c ) )
                        {
                            *(cctm + c) = (byte) CRFCharType.InterpreteAsWhitespace;
                        }

                        if ( c == char.MaxValue )
                        {
                            break;
                        }
                    }

                    foreach ( var c in INCLUDE_INTERPRETE_AS_WHITESPACE )
                    {
                        *(cctm + c) = (byte) CRFCharType.InterpreteAsWhitespace;
                    }

                    foreach ( var c in TOKENIZE_DIFFERENT_SEPARATELY )
                    {
                        *(cctm + c) = (byte) CRFCharType.TokenizeDifferentSeparately;
                    }

                    var between_letter_or_digit = (languageType == LanguageTypeEnum.En)
                                                  ? BETWEEN_LETTER_OR_DIGIT_EN 
                                                  : BETWEEN_LETTER_OR_DIGIT;
                    foreach ( var c in between_letter_or_digit )
                    {
                        *(cctm + c) |= (byte) CRFCharType.BetweenLetterOrDigit;
                    }

                    foreach ( var c in BETWEEN_DIGIT )
                    {
                        *(cctm + c) |= (byte) CRFCharType.BetweenDigit;
                    }

                    #region commented
                    /*
                    foreach ( var c in EXCLUDE_INTERPRETE_AS_WHITESPACE )
                    {
                        var cct = *(cctm + c);
                        if ( (cct & CRFCharType.BetweenNonWhitespace) == CRFCharType.BetweenNonWhitespace )
                            *(cctm + c) ^= CRFCharType.BetweenNonWhitespace;
                        else
                        if ( (cct & CRFCharType.InterpreteAsWhitespace) == CRFCharType.InterpreteAsWhitespace )
                            *(cctm + c) ^= CRFCharType.InterpreteAsWhitespace;
                    }
                    */
                    #endregion

                    //-ERROR-!!!-*(cctm + DOT) |= (byte) CRFCharType.DotChar;
                    //-ONLY-SO--!!!-
                    *(cctm + DOT) = (byte) CRFCharType.DotChar;
                }

                var CRF_CHARTYPE_MAP_GCHandle = GCHandle.Alloc( CRF_CHARTYPE_MAP, GCHandleType.Pinned );
                _CRF_CHARTYPE_MAP = (CRFCharType*) CRF_CHARTYPE_MAP_GCHandle.AddrOfPinnedObject().ToPointer();
            }

            //public static readonly UnsafeConst Inst = new UnsafeConst();

            //private readonly static Lazy< UnsafeConst > _Inst_Ru = new Lazy< UnsafeConst >(
            private static UnsafeConst _Inst_Ru;
            private static UnsafeConst _Inst_En;

            public static UnsafeConst GetInstanceByLanguage( LanguageTypeEnum languageType )
            {
                switch ( languageType )
                {                    
                    case LanguageTypeEnum.En:
                        if ( _Inst_En == null )
                        {
                            lock ( typeof(UnsafeConst) )
                            {
                                if ( _Inst_En == null )
                                {
                                    _Inst_En = new UnsafeConst( languageType );
                                }
                            }
                        }
                        return (_Inst_En);

                    default: //case LanguageTypeEnum.Ru:
                        if ( _Inst_Ru == null )
                        {
                            lock ( typeof(UnsafeConst) )
                            {
                                if ( _Inst_Ru == null )
                                {
                                    _Inst_Ru = new UnsafeConst( languageType );
                                }
                            }
                        }
                        return (_Inst_Ru);
                }
            }
        }

        #region [.private field's.]
        private const int DEFAULT_WORDSLIST_CAPACITY = 100;
        private const int DEFAULT_WORDTOUPPERBUFFER  = 100;

        private readonly SentSplitter                 _SentSplitter;
        private readonly List< word_t >               _Words;
        private readonly IPosTaggerInputTypeProcessor _PosTaggerInputTypeProcessor;
        private readonly INerInputTypeProcessor       _NerInputTypeProcessor;        
        private readonly HashSet< string >            _ParticleThatExclusion;
        private readonly CharType*                    _CTM;
        private readonly char*                        _UIM;
        private readonly CRFCharType*                 _CCTM;
        //private readonly bool                       _Make_PosTagger;
        private char*                                 _BASE;
        private char*                                 _Ptr;        
        private int                                   _StartIndex;
        private int                                   _Length;
        private ProcessSentCallbackDelegate           _OuterProcessSentCallback_Delegate;
        private char*                                 _StartPtr;
        private char*                                 _EndPtr;
        //private char[]                                _WordToUpperBuffer;
        private int                                   _WordToUpperBufferSize;
        private GCHandle                              _WordToUpperBufferGCHandle;
        private char*                                 _WordToUpperBufferPtrBase;
        private bool                                  _NotSkipNonLetterAndNonDigitToTheEnd; //need for NER-model-builder
        private SentSplitter.ProcessSentCallbackDelegate _SentSplitterProcessSentCallback_Delegate;
        #endregion

        #region [.ctor().]
        public Tokenizer( TokenizerConfig config )
        {
            _SentSplitter          = new SentSplitter( config.SentSplitterConfig );
            _Words                 = new List< word_t >( DEFAULT_WORDSLIST_CAPACITY );
            _ParticleThatExclusion = config.Model.ParticleThatExclusion;
            _SentSplitterProcessSentCallback_Delegate = new SentSplitter.ProcessSentCallbackDelegate( SentSplitterProcessSentCallback );

            _UIM  = xlat_Unsafe.Inst._UPPER_INVARIANT_MAP;
            _CTM  = xlat_Unsafe.Inst._CHARTYPE_MAP;
            _CCTM = UnsafeConst.GetInstanceByLanguage( config.LanguageType )._CRF_CHARTYPE_MAP; //UnsafeConst.Inst._CRF_CHARTYPE_MAP;

            //--//
            ReAllocWordToUpperBuffer( DEFAULT_WORDTOUPPERBUFFER );

            if ( (config.TokenizeMode & TokenizeMode.PosTagger) == TokenizeMode.PosTagger )
            {
                _PosTaggerInputTypeProcessor = config.PosTaggerInputTypeProcessorFactory.CreateInstance();
                //_Make_PosTagger = true;
            }
            else
            {
                _PosTaggerInputTypeProcessor = Dummy_PosTaggerInputTypeProcessor.Instance;
            }

            if ( (config.TokenizeMode & TokenizeMode.Ner) == TokenizeMode.Ner )
            {
                _NerInputTypeProcessor = config.NerInputTypeProcessorFactory.CreateInstance();
                //_Make_Ner = true;
            }
            else
            {
                _NerInputTypeProcessor = Dummy_NerInputTypeProcessor.Instance;
            }
        }

        private void ReAllocWordToUpperBuffer( int newBufferSize )
        {
            DisposeNativeResources();

            _WordToUpperBufferSize = newBufferSize;
            var wordToUpperBuffer  = new char[ _WordToUpperBufferSize ];
            _WordToUpperBufferGCHandle = GCHandle.Alloc( wordToUpperBuffer, GCHandleType.Pinned );
            _WordToUpperBufferPtrBase  = (char*) _WordToUpperBufferGCHandle.AddrOfPinnedObject().ToPointer();
        }

        ~Tokenizer()
        {
            DisposeNativeResources();
        }
        public void Dispose()
        {
            DisposeNativeResources();

            GC.SuppressFinalize( this );
        }
        private void DisposeNativeResources()
        {
            if ( _WordToUpperBufferPtrBase != null )
            {
                _WordToUpperBufferGCHandle.Free();
                _WordToUpperBufferPtrBase = null;
            }
        }
        #endregion

        public delegate void ProcessSentCallbackDelegate( List< word_t > words );

        public void Run( string text, bool splitBySmiles, ProcessSentCallbackDelegate processSentCallback )
        {
            _OuterProcessSentCallback_Delegate = processSentCallback;

            fixed ( char* _base = text )
            {
                _BASE = _base;

                _SentSplitter.SplitBySmiles = splitBySmiles;
                _SentSplitter.AllocateSents( text, _SentSplitterProcessSentCallback_Delegate );
            }

            _OuterProcessSentCallback_Delegate = null;
        }

        private void SentSplitterProcessSentCallback( sent_t sent )
        {
            _Words.Clear();
            _StartIndex = sent.startIndex;
            _Length     = 0;
            _StartPtr   = _BASE + _StartIndex;
            _EndPtr     = _StartPtr + sent.length - 1;

            var urls        = sent.urls;
            var urlIndex    = 0;
            var startUrlPtr = (urls != null) ? (_BASE + urls[ 0 ].startIndex) : UnsafeConst.MAX_PTR;

            #region [.main.]
            var realyEndPtr = _EndPtr;
            _EndPtr = SkipNonLetterAndNonDigitToTheEnd();

            for ( _Ptr = _StartPtr; _Ptr <= _EndPtr; _Ptr++ )
            {
                #region [.process allocated url's.]
                if ( startUrlPtr <= _Ptr )
                {
                    #region [.code.]
                    TryCreateWordAndPut2List();

                    var lenu = urls[ urlIndex ].length;
                    #region [.skip-ignore url's.]
                    /*
                    #region [.create word. url.]
                    var lenu = urls[ urlIndex ].length;
                    var vu = new string( startUrlPtr, 0, lenu );
                    var wu = new word_t()
                    {
                        startIndex         = urls[ urlIndex ].startIndex, 
                        length             = lenu, 
                        valueOriginal      = vu,
                        valueUpper         = vu,
                        posTaggerInputType = PosTaggerInputType.Url
                    };
                    _Words.Add( wu );
                    #endregion
                    //*/
                    #endregion

                    _Ptr = startUrlPtr + lenu - 1;
                    urlIndex++;
                    startUrlPtr = (urlIndex < urls.Count) ? (_BASE + urls[ urlIndex ].startIndex) : UnsafeConst.MAX_PTR;

                    _StartIndex = (int) (_Ptr - _BASE + 1);
                    _Length     = 0;
                    continue;

                    #endregion
                }
                #endregion

                var ch = *_Ptr;
                var ct = *(_CTM + ch);
                #region [.whitespace.]
                if ( (ct & CharType.IsWhiteSpace) == CharType.IsWhiteSpace )
                {
                    TryCreateWordAndPut2List();

                    _StartIndex++;
                    continue;
                }
                #endregion
                
                var pct = *(_CCTM + ch);
                #region [.dot.]
                if ( (pct & CRFCharType.DotChar) == CRFCharType.DotChar &&
                     IsUpperNextChar()
                   )
                {
                    _Length++;
                    TryCreateWordAndPut2List();
                    continue;
                }
                #endregion

                #region [.between-letter-or-digit.]
                if ( (pct & CRFCharType.BetweenLetterOrDigit) == CRFCharType.BetweenLetterOrDigit )
                {
                    if ( IsBetweenLetterOrDigit() )
                    {
                        _Length++;
                    }
                    else
                    {
                        TryCreateWordAndPut2List();

                        #region [.merge punctuation (with white-space's).]
                        if ( !MergePunctuation( ch ) )
                            break;
                        #endregion

                        //punctuation word
                        TryCreateWordAndPut2List();
                    }

                    continue;
                }
                //с учетом того, что списки 'BetweenLetterOrDigit' и 'BetweenDigit' не пересекаются
                else                 
                if ( (pct & CRFCharType.BetweenDigit) == CRFCharType.BetweenDigit )
                {
                    if ( IsBetweenDigit() )
                    {
                        _Length++;
                    }
                    else
                    {
                        TryCreateWordAndPut2List();

                        #region [.merge punctuation (with white-space's).]
                        if ( !MergePunctuation( ch ) )
                            break;
                        #endregion

                        //punctuation word
                        TryCreateWordAndPut2List();
                    }

                    continue;                    
                }
                #endregion

                #region [.tokenize-different-separately.]
                if ( (pct & CRFCharType.TokenizeDifferentSeparately) == CRFCharType.TokenizeDifferentSeparately )
                {
                    TryCreateWordAndPut2List();

                    #region [.merge punctuation (with white-space's).]
                    if ( !MergePunctuation( ch ) )
                        break;
                    #region 
                    /*
                    _Length = 1;
                    _Ptr++;
                    for ( ; _Ptr <= _EndPtr; _Ptr++ ) 
                    {
                        var ch_next = *_Ptr;
                        if ( ch_next != ch )
                            break;

                        _Length++;
                    }
                    if ( _EndPtr < _Ptr )
                    {
                        if ( (_Length == 1) && (*_EndPtr == '\0') )
                            _Length = 0;
                        break;
                    }
                    _Ptr--;
                    */
                    #endregion
                    #endregion

                    //punctuation word
                    TryCreateWordAndPut2List();

                    continue;
                }
                #endregion

                #region [.interprete-as-whitespace.]
                if ( (pct & CRFCharType.InterpreteAsWhitespace) == CRFCharType.InterpreteAsWhitespace )
                {
                    TryCreateWordAndPut2List();

                    _StartIndex++;
                    continue;
                }
                #endregion

                #region [.increment length.]
                _Length++;
                #endregion
            }
            #endregion

            #region [.last word.]
            TryCreateWordAndPut2List();
            #endregion

            #region [.tail punctuation.]
            for ( _EndPtr = realyEndPtr; _Ptr <= _EndPtr; _Ptr++ )
            {
                var ch = *_Ptr;
                var ct = *(_CTM + ch);
                #region [.whitespace.]
                if ( (ct & CharType.IsWhiteSpace) == CharType.IsWhiteSpace )
                {
                    TryCreateWordAndPut2List();

                    _StartIndex++;
                    continue;
                }
                #endregion
                
                var nct = *(_CCTM + ch);
                #region [.tokenize-different-separately.]
                if ( (nct & CRFCharType.TokenizeDifferentSeparately) == CRFCharType.TokenizeDifferentSeparately )
                {
                    TryCreateWordAndPut2List();

                    #region [.merge punctuation (with white-space's).]
                    if ( !MergePunctuation( ch ) )
                        break;
                    #endregion

                    //punctuation word
                    TryCreateWordAndPut2List();

                    continue;
                }
                #endregion

                #region [.interprete-as-whitespace.]
                if ( (nct & CRFCharType.InterpreteAsWhitespace) == CRFCharType.InterpreteAsWhitespace )
                {
                    TryCreateWordAndPut2List();

                    _StartIndex++;
                    continue;
                }
                #endregion

                #region [.increment length.]
                _Length++;
                #endregion
            }
            #endregion

            #region [.last punctuation.]
            TryCreateWordAndPut2List();
            #endregion
            
            _OuterProcessSentCallback_Delegate( _Words );
        }

        private void TryCreateWordAndPut2List()
        {            
            const int    THAT_LENGTH = 2;
            const int    DUSH_LENGTH = 1;
            const int    PARTICLE_THAT_LENGTH = DUSH_LENGTH + THAT_LENGTH;
            const string THAT_UPPER = "ТО";
            const string THAT_LOWER = "то";
            const string DUSH       = "-";
            const char   T_RU       = 'Т';
            const char   O_RU       = 'О';

            if ( _Length != 0 )
            {
                #region [.to upper invariant & pos-tagger-list & etc.]
                var startPtr = _BASE + _StartIndex;

                if ( _WordToUpperBufferSize < _Length )
                {
                    ReAllocWordToUpperBuffer( _Length );
                }                
                for ( int i = 0; i < _Length; i++ )
                {
                    *(_WordToUpperBufferPtrBase + i) = *(_UIM + *(startPtr + i));
                }

                var valueUpper = new string( _WordToUpperBufferPtrBase, 0, _Length );

                #region [.detect particle-that.]
                var len = _Length - PARTICLE_THAT_LENGTH;
                if ( 0 < len )
                {
                    //ends-with "-ТО"
                    if ( (*(_CTM + *(_WordToUpperBufferPtrBase + len)) & CharType.IsHyphen) == CharType.IsHyphen &&
                         (*(_WordToUpperBufferPtrBase + ++len) == T_RU) &&
                         (*(_WordToUpperBufferPtrBase + ++len) == O_RU)
                       )
                    {
                        //need split particle-that
                        if ( !_ParticleThatExclusion.Contains( valueUpper ) )
                        {
                            #region [.create word with split particle-that.]
                            len = _Length - PARTICLE_THAT_LENGTH;
                            valueUpper = new string( _WordToUpperBufferPtrBase, 0, len );

                            #region [.create word without particle-that.]
                            var _valueOriginal = new string( _BASE, _StartIndex, len );
                            var _word = new word_t()
                            {
                                startIndex    = _StartIndex,
                                length        = len,
                                valueOriginal = _valueOriginal,
                                valueUpper    = valueUpper,
                            };
                            #endregion

                            #region [.nerInputType.]
                            _word.nerInputType = _NerInputTypeProcessor.GetNerInputType( startPtr, len );
                            #endregion

                            #region [.posTaggerInputType.]
                            //if ( _Make_PosTagger )
                            {
                                var _result = _PosTaggerInputTypeProcessor.GetResult( startPtr, len, _word );
                                _word.posTaggerInputType        = _result.posTaggerInputType;
                                _word.posTaggerExtraWordType    = _result.posTaggerExtraWordType;
                                _word.posTaggerFirstCharIsUpper = ((*(_CTM + *startPtr) & CharType.IsUpper) == CharType.IsUpper);
                                _word.posTaggerLastValueUpperInNumeralChain = (_result.posTaggerLastValueUpperInNumeralChainIsValueOriginal)
                                                                              ? _word.valueUpper : _result.posTaggerLastValueUpperInNumeralChain;                          
                            }
                            #endregion

                            #region [.put-2-list.]
                            _Words.Add( _word );
                            #endregion

                            #region [.create word particle-that.]
                            var startIndex = _StartIndex + len;
                            //_valueOriginal = new string( _BASE, startIndex, 1 );
                            _word = new word_t()
                            {
                                startIndex             = startIndex,
                                length                 = DUSH_LENGTH,
                                valueOriginal          = DUSH,
                                valueUpper             = DUSH,
                                posTaggerInputType     = PosTaggerInputType.Dush,
                                posTaggerExtraWordType = PosTaggerExtraWordType.Punctuation,
                                //nerInputType           = NerInputType.O,
                            };
                            _Words.Add( _word );

                            startIndex++;
                            //valueUpper     = new string( chars_ptr, len, 2 );
                            //_valueOriginal = new string( _BASE, startIndex, 2 );
                            _word = new word_t()
                            {
                                startIndex         = startIndex,
                                length             = THAT_LENGTH,
                                valueOriginal      = THAT_LOWER,
                                valueUpper         = THAT_UPPER,
                                posTaggerInputType = PosTaggerInputType.O,
                                //nerInputType           = NerInputType.O,
                            };
                            _Words.Add( _word );
                            #endregion

                            #region [.inctement start-index.]
                            _StartIndex += _Length;
                            _Length      = 0;
                            #endregion

                            #region [.return.]
                            return;
                            #endregion
                            #endregion
                        }
                    }
                }
                #endregion
                #endregion

                #region [.create word.]
                var valueOriginal = new string( _BASE, _StartIndex, _Length );
                var word = new word_t()
                {
                    startIndex    = _StartIndex, 
                    length        = _Length, 
                    valueOriginal = valueOriginal,
                    valueUpper    = valueUpper,
                };                
                #endregion

                #region [.nerInputType.]
                word.nerInputType = _NerInputTypeProcessor.GetNerInputType( _BASE + _StartIndex, _Length );
                #endregion

                #region [.posTaggerInputType.]
                //if ( _Make_PosTagger )
                {
                    var result = _PosTaggerInputTypeProcessor.GetResult( startPtr, _Length, word );
                    word.posTaggerInputType        = result.posTaggerInputType;
                    word.posTaggerExtraWordType    = result.posTaggerExtraWordType;
                    word.posTaggerFirstCharIsUpper = ((*(_CTM + *startPtr) & CharType.IsUpper) == CharType.IsUpper);
                    word.posTaggerLastValueUpperInNumeralChain = (result.posTaggerLastValueUpperInNumeralChainIsValueOriginal)
                                                                 ? word.valueUpper : result.posTaggerLastValueUpperInNumeralChain;
                }
                #endregion

                #region [.put-2-list.]
                _Words.Add( word );
                #endregion

                #region [.inctement start-index.]
                _StartIndex += _Length;
                _Length      = 0;
                #endregion
            }
        }

        private char* SkipNonLetterAndNonDigitToTheEnd()
        {
            //need for NER-model-builder
            if ( _NotSkipNonLetterAndNonDigitToTheEnd )
                return (_EndPtr);

            for ( char* ptr = _EndPtr; _StartPtr <= ptr; ptr-- )
            {
                var ct = *(_CTM + *ptr);
                if ( (ct & CharType.IsLetter) == CharType.IsLetter ||
                     (ct & CharType.IsDigit ) == CharType.IsDigit
                    )
                {
                    #region [.если на конце предложения одиночная буква большая, то точку не отрывать.]
                    if ( (ct & CharType.IsUpper) == CharType.IsUpper )
                    {
                        var p = ptr - 1;
                        if ( (_StartPtr == p) || ((_StartPtr < p) && (*(_CTM + *p) & CharType.IsWhiteSpace) == CharType.IsWhiteSpace) )
                        {
                            p = ptr + 1;
                            if ( (p == _EndPtr) || ((p < _EndPtr) && (*(_CTM + *(p + 1)) & CharType.IsWhiteSpace) == CharType.IsWhiteSpace) )
                            {
                                if ( xlat.IsDot( *p ) )
                                return (p);
                            }
                        }
                    }
                    #endregion

                    return (ptr);
                }
            }
            return (_StartPtr - 1);
        }

        private bool IsBetweenLetterOrDigit()
        {
            if ( _Ptr <= _StartPtr )
                return (false);

            var ch = *(_Ptr - 1);
            var ct = *(_CTM + ch);
            if ( (ct & CharType.IsLetter) != CharType.IsLetter &&
                 (ct & CharType.IsDigit ) != CharType.IsDigit
               )
            {
                return (false);
            }

            var p = _Ptr + 1;
            if ( _EndPtr <= p )
            {
                if ( _EndPtr < p )
                    return (false);
                ch = *p;
                if ( ch == '\0' )
                    return (false);
            }
            else
            {
                ch = *p;
            }
            ct = *(_CTM + ch);
            if ( (ct & CharType.IsLetter) != CharType.IsLetter &&
                 (ct & CharType.IsDigit ) != CharType.IsDigit
               )
            {
                return (false);
            }

            return (true);
        }
        private bool IsBetweenDigit()
        {
            if ( _Ptr <= _StartPtr )
                return (false);

            var ch = *(_Ptr - 1);
            var ct = *(_CTM + ch);
            if ( (ct & CharType.IsDigit ) != CharType.IsDigit )
            {
                return (false);
            }

            var p = _Ptr + 1;
            if ( _EndPtr <= p )
            {
                if ( _EndPtr < p )
                    return (false);
                ch = *p;
                if ( ch == '\0' )
                    return (false);
            }
            else
            {
                ch = *p;
            }
            ct = *(_CTM + ch);
            if ( (ct & CharType.IsDigit ) != CharType.IsDigit )
            {
                return (false);
            }

            return (true);
        }
        private bool IsUpperNextChar()
        {
            var p = _Ptr + 1;
            var ch = default(char);
            if ( _EndPtr <= p )
            {
                if ( _EndPtr < p )
                    return (false);
                ch = *p;
                if ( ch == '\0' )
                    return (false);
            }
            else
            {
                ch = *p;
            }

            var ct = *(_CTM + ch);
            if ( (ct & CharType.IsUpper) != CharType.IsUpper )
            {
                return (false);
            }

            return (true);
        }

        private bool MergePunctuation( char begining_ch )
        {
            _Length = 1;
            _Ptr++;
            var whitespace_length = 0;
            for ( ; _Ptr <= _EndPtr; _Ptr++ ) 
            {                
                var ch_next = *_Ptr;
                var ct = *(_CTM  + ch_next);
                if ( (ct & CharType.IsWhiteSpace) == CharType.IsWhiteSpace )
                {
                    whitespace_length++;
                    continue;
                }

                var nct = *(_CCTM + ch_next);
                if ( (nct & CRFCharType.InterpreteAsWhitespace) == CRFCharType.InterpreteAsWhitespace )
                {
                    whitespace_length++;
                    continue;
                }

                if ( ch_next == begining_ch )
                {
                    _Length += whitespace_length + 1;
                    whitespace_length = 0;
                    continue;
                }

                break;
            }
            if ( _EndPtr < _Ptr )
            {
                if ( (_Length == 1) && (*_EndPtr == '\0') )
                    _Length = 0;
                return (false);
            }
            _Ptr -= whitespace_length + 1;

            return (true);
        }        

        #region [.ner-model-builder.]
        private readonly sent_t                    _BuildModelSent;
        private readonly UrlDetector               _UrlDetector;
        private readonly List< buildmodel_word_t > _BuildModelWords;
        private BuildModelNerInputType             _BuildModelNerInputTypeB;
        private BuildModelNerInputType             _BuildModelNerInputTypeI;

        private Tokenizer( TokenizerConfig4NerModelBuilder config )
        {
            config.UrlDetectorConfig.UrlExtractMode = UrlDetector.UrlExtractModeEnum.Position;

            _UrlDetector           = new UrlDetector( config.UrlDetectorConfig );
            _BuildModelSent        = sent_t.CreateEmpty();
            _Words                 = new List< word_t >( DEFAULT_WORDSLIST_CAPACITY );
            _BuildModelWords       = new List< buildmodel_word_t >( DEFAULT_WORDSLIST_CAPACITY );
            _ParticleThatExclusion = config.Model.ParticleThatExclusion;

            _UIM  = xlat_Unsafe.Inst._UPPER_INVARIANT_MAP;
            _CTM  = xlat_Unsafe.Inst._CHARTYPE_MAP;
            _CCTM = UnsafeConst.GetInstanceByLanguage( config.LanguageType )._CRF_CHARTYPE_MAP; //UnsafeConst.Inst._CRF_CHARTYPE_MAP;

            //--//
            ReAllocWordToUpperBuffer( DEFAULT_WORDTOUPPERBUFFER );

            _PosTaggerInputTypeProcessor = Dummy_PosTaggerInputTypeProcessor.Instance;
            _NerInputTypeProcessor       = config.NerInputTypeProcessorFactory.CreateInstance();
        }

        public static Tokenizer Create4NerModelBuilder( TokenizerConfig4NerModelBuilder config )
        {
            var tokenizer = new Tokenizer( config );
            return (tokenizer);
        }

        public List< buildmodel_word_t > run4ModelBuilder( 
                string        partOfSentText, 
                bool          isLastPartOfSentText,
                NerOutputType nerOutputType,
                bool          prevPartOfSentTextSameNerOutputType )
        {
            _BuildModelWords.Clear();
            if ( prevPartOfSentTextSameNerOutputType )
            {
                _BuildModelNerInputTypeI = nerOutputType.ToBuildModelNerInputTypeI();
                _BuildModelNerInputTypeB = _BuildModelNerInputTypeI;
            }
            else
            {
                _BuildModelNerInputTypeI = nerOutputType.ToBuildModelNerInputTypeI();
                _BuildModelNerInputTypeB = nerOutputType.ToBuildModelNerInputTypeB();
            }

            _OuterProcessSentCallback_Delegate = ProcessSentCallbackModelBuilder;

            fixed ( char* _base = partOfSentText )            
            {
                _BASE = _base;               
                _NotSkipNonLetterAndNonDigitToTheEnd = !isLastPartOfSentText;

                var urls = _UrlDetector.AllocateUrls( partOfSentText );
                _BuildModelSent.Set4ModelBuilder( 0, partOfSentText.Length, (0 < urls.Count) ? urls : null );

                SentSplitterProcessSentCallback( _BuildModelSent );

                _NotSkipNonLetterAndNonDigitToTheEnd = false;
            }

            _OuterProcessSentCallback_Delegate = null;

            return (_BuildModelWords);
        }

        private void ProcessSentCallbackModelBuilder( List< word_t > words )
        {
            var len = words.Count;
            if ( 0 < len )
            {
                _BuildModelWords.Add( new buildmodel_word_t() { word = words[ 0 ], buildModelNerInputType = _BuildModelNerInputTypeB } );
                for ( int i = 1; i < len; i++ )
                {
                    _BuildModelWords.Add( new buildmodel_word_t() { word = words[ i ], buildModelNerInputType = _BuildModelNerInputTypeI } );
                }
            }
        }
        #endregion
    }
}
