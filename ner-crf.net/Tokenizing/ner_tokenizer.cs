using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ilook.core;
using ilook.core.sentsplitting;

namespace ner
{
    /// <summary>
    /// 
    /// </summary>
    unsafe internal class ner_tokenizer
    {
        /// <summary>
        /// 
        /// </summary>
        [Flags]
        private enum NERCharType : byte
        {
            __UNDEFINE__                = 0x0,

            InterpreteAsWhitespace      = 0x1,
            BetweenLetterOrDigit        = (1 << 1),
            TokenizeDifferentSeparately = (1 << 2),
            DotChar                     = (1 << 3),
        }

        #region [.static .ctor() & xlat table's.]
        private static readonly char*         MAX_PTR                          = (char*) (0xffffffffFFFFFFFF);
        private const int                     DEFAULT_WORDSLIST_CAPACITY       = 100;
        private static readonly NERCharType[] NER_CHARTYPE_MAP                 = new NERCharType[ char.MaxValue ];
        private const string                  INCLUDE_INTERPRETE_AS_WHITESPACE = "¥©¤¦§®¶€™<>";
        private const char                    DOT                              = '\u002E'; /* 0x2E, 46, '.' */
        private static readonly char[]        BETWEEN_LETTER_OR_DIGIT          = new char[] { 
                                                                                    '\u0026', /* 0x26  , 38  , '&' */
                                                                                    '\u0027', /* 0x27  , 39  , ''' */
                                                                                    '\u002D', /* 0x2D  , 45  , '-' */
                                                                                    //DOT, //'\u002E', /* 0x2E  , 46  , '.' */
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
                                                                                  };

        unsafe static ner_tokenizer()
        {
            fixed ( NERCharType* nctm = NER_CHARTYPE_MAP )        
            {
                for ( var c = char.MinValue; c < char.MaxValue; c++ )
                {
                    if ( /*char.IsWhiteSpace( c ) ||*/ char.IsPunctuation( c ) )
                    {
                        *(nctm + c) = NERCharType.InterpreteAsWhitespace;
                    }
                }

                foreach ( var c in INCLUDE_INTERPRETE_AS_WHITESPACE )
                {
                    *(nctm + c) = NERCharType.InterpreteAsWhitespace;
                }

                foreach ( var c in TOKENIZE_DIFFERENT_SEPARATELY )
                {
                    *(nctm + c) = NERCharType.TokenizeDifferentSeparately;
                }

                foreach ( var c in BETWEEN_LETTER_OR_DIGIT )
                {
                    *(nctm + c) |= NERCharType.BetweenLetterOrDigit;
                }

                /*
                foreach ( var c in EXCLUDE_INTERPRETE_AS_WHITESPACE )
                {
                    var nct = *(nctm + c);
                    if ( (nct & NERCharType.BetweenNonWhitespace) == NERCharType.BetweenNonWhitespace )
                        *(nctm + c) ^= NERCharType.BetweenNonWhitespace;
                    else
                    if ( (nct & NERCharType.InterpreteAsWhitespace) == NERCharType.InterpreteAsWhitespace )
                        *(nctm + c) ^= NERCharType.InterpreteAsWhitespace;
                }
                */

                *(nctm + DOT) = NERCharType.DotChar;
            }
        }
        #endregion

        #region [.private field's.]
        private readonly SentSplitter       _SentSplitter;
        private readonly List< word_t >     _Words;
        private char*                       _BASE;
        private char*                       _Ptr;
        private CharType*                   _CTM;
        private NERCharType*                _NCTM;
        private int                         _StartIndex;
        private int                         _Length;
        private ProcessSentCallbackDelegate _ProcessSentCallback;
        private char*                       _StartPtr;
        private char*                       _EndPtr;
        private bool                        _NotSkipNonLetterAndNonDigitToTheEnd; //need for NER-model-builder
        #endregion

        public ner_tokenizer( SentSplitterConfig config )
        {
            _SentSplitter = new SentSplitter( config );
            _Words        = new List< word_t >( DEFAULT_WORDSLIST_CAPACITY );
        }
        public ner_tokenizer( SentSplitterConfig config, int wordCapacity )
        {
            _SentSplitter = new SentSplitter( config );
            _Words        = new List< word_t >( wordCapacity );
        }

        public delegate void ProcessSentCallbackDelegate( List< word_t > words );

        public void run( string text, bool splitBySmiles, ProcessSentCallbackDelegate processSentCallback )
        {
            _ProcessSentCallback = processSentCallback;

            fixed ( char*        _base = text              )
            fixed ( CharType*    ctm   = xlat.CHARTYPE_MAP )
            fixed ( NERCharType* nctm  = NER_CHARTYPE_MAP  )
            {
                _BASE = _base;
                _CTM  = ctm;
                _NCTM = nctm;

                _SentSplitter.SplitBySmiles = splitBySmiles;
                _SentSplitter.AllocateSents( text, ProcessSentSplitterCallback );
            }

            _ProcessSentCallback = null;
        }

        private void ProcessSentSplitterCallback( sent_t sent )
        {
            _Words.Clear();
            _StartIndex = sent.startIndex;
            _Length     = 0;
            _StartPtr   = _BASE + _StartIndex;
            _EndPtr     = _StartPtr + sent.length - 1;

            var urls        = sent.urls;
            var urlIndex    = 0;
            var startUrlPtr = (urls != null) ? (_BASE + urls[ 0 ].startIndex) : MAX_PTR;

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

                    #region [.create word. url.]
                    var lenu = urls[ urlIndex ].length;
                    var vu = new string( startUrlPtr, 0, lenu );
                    var wu = new word_t()
                    {
                        startIndex       = urls[ urlIndex ].startIndex, 
                        length           = lenu, 
                        valueOriginal            = vu,
                        nerInputType = NerInputType.O, //URL
                    };
                    _Words.Add( wu );
                    #endregion

                    _Ptr = startUrlPtr + lenu - 1;
                    urlIndex++;
                    startUrlPtr = (urlIndex < urls.Count) ? (_BASE + urls[ urlIndex ].startIndex) : MAX_PTR;

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
                
                var nct = *(_NCTM + ch);
                #region [.dot.]
                if ( (nct & NERCharType.DotChar) == NERCharType.DotChar &&
                     IsUpperNextChar()
                   )
                {
                    _Length++;
                    TryCreateWordAndPut2List();
                    continue;
                }
                #endregion

                #region [.between-non-whitespace.]
                if ( (nct & NERCharType.BetweenLetterOrDigit) == NERCharType.BetweenLetterOrDigit )
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
                #endregion

                #region [.tokenize-different-separately.]
                if ( (nct & NERCharType.TokenizeDifferentSeparately) == NERCharType.TokenizeDifferentSeparately )
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
                if ( (nct & NERCharType.InterpreteAsWhitespace) == NERCharType.InterpreteAsWhitespace )
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
                
                var nct = *(_NCTM + ch);
                #region [.tokenize-different-separately.]
                if ( (nct & NERCharType.TokenizeDifferentSeparately) == NERCharType.TokenizeDifferentSeparately )
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
                if ( (nct & NERCharType.InterpreteAsWhitespace) == NERCharType.InterpreteAsWhitespace )
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
            
            _ProcessSentCallback( _Words );
        }

        private void TryCreateWordAndPut2List()
        {
            if ( _Length != 0 )
            {
                #region [.create word.]
                var v = new string( _BASE, _StartIndex, _Length );
                var w = new word_t()
                {
                    startIndex    = _StartIndex, 
                    length        = _Length, 
                    valueOriginal = v,
                    nerInputType  = NerInputTypeProcessor.GetNerInputType( _CTM, _BASE + _StartIndex, _Length ),
                };
                _Words.Add( w );
                #endregion

                _StartIndex += _Length;
                _Length      = 0;                
            }
        }

        private char* SkipNonLetterAndNonDigitToTheEnd()
        {
            if ( _NotSkipNonLetterAndNonDigitToTheEnd )
                return (_EndPtr);

            for ( char* ptr = _EndPtr; _StartPtr <= ptr; ptr-- )
            {
                var ct = *(_CTM + *ptr);
                if ( (ct & CharType.IsLetter) == CharType.IsLetter ||
                     (ct & CharType.IsDigit ) == CharType.IsDigit
                    )
                {
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
        /*private bool IsLetterPrevAndNextChar()
        {
            if ( _Ptr <= _StartPtr )
                return (false);

            var ch = *(_Ptr - 1);
            var ct = *(_CTM + ch);
            if ( (ct & CharType.IsLetter) != CharType.IsLetter )
                return (false);

            var p = _Ptr + 1;
            if ( _EndPtr <= p )
                return (false);
            ch = *p;
            ct = *(_CTM + ch);
            if ( (ct & CharType.IsLetter) != CharType.IsLetter )
                return (false);

            return (true);
        }*/
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

                var nct = *(_NCTM + ch_next);
                if ( (nct & NERCharType.InterpreteAsWhitespace) == NERCharType.InterpreteAsWhitespace )
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

        /*private bool IsPrevCharTokenizeDifferentSeparately()
        {
            if ( _StartPtr < _Ptr )
            {
                var nct = *(_NCTM + *(_Ptr - 1));
                if ( (nct & NERCharType.TokenizeDifferentSeparately) == NERCharType.TokenizeDifferentSeparately )
                    return (true);
            }
            return (false);
        }*/

        #region [.ner-model-builder.]
        private readonly sent_t                    _BuildModelSent;
        private readonly UrlDetector               _UrlDetector;
        private readonly List< buildmodel_word_t > _BuildModelWords;
        private BuildModelNerInputType             _BuildModelNerInputTypeB;
        private BuildModelNerInputType             _BuildModelNerInputTypeI;

        public ner_tokenizer( UrlDetectorConfig config )
        {
            config.UrlExtractMode = UrlDetector.UrlExtractModeEnum.Position;

            _UrlDetector     = new UrlDetector( config );
            _BuildModelSent  = sent_t.CreateEmpty();
            _Words           = new List< word_t >( DEFAULT_WORDSLIST_CAPACITY );
            _BuildModelWords = new List< buildmodel_word_t >( DEFAULT_WORDSLIST_CAPACITY );
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

            _ProcessSentCallback = ProcessSentCallbackModelBuilder;

            fixed ( char*        _base = partOfSentText    )
            fixed ( CharType*    ctm   = xlat.CHARTYPE_MAP )
            fixed ( NERCharType* nctm  = NER_CHARTYPE_MAP  )
            {
                _BASE = _base;
                _CTM  = ctm;
                _NCTM = nctm;
                _NotSkipNonLetterAndNonDigitToTheEnd = !isLastPartOfSentText;

                var urls = _UrlDetector.AllocateUrls( partOfSentText );
                _BuildModelSent.Set4ModelBuilder( 0, partOfSentText.Length, (0 < urls.Count) ? urls : null );

                ProcessSentSplitterCallback( _BuildModelSent );

                _NotSkipNonLetterAndNonDigitToTheEnd = false;
            }

            _ProcessSentCallback = null;

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
