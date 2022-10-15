using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using lingvo.core;
using lingvo.urls;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace lingvo.sentsplitting
{    
    using SentCharType = SentSplitterModel.SentCharType;

    /// <summary>
    /// 
    /// </summary>
    public sealed class sent_t
    {
        private const int DEFAULT_URLS_LIST_CAPACITY = 4;

        internal sent_t() : this( 0 ) { }
        internal sent_t( int _startIndex )
        {
            startIndex = _startIndex;
            length     = 0;
        }
        internal sent_t( int _startIndex, int _length )
        {
            startIndex = _startIndex;
            length     = _length;
        }

        [M(O.AggressiveInlining)] public void Set4ModelBuilder( int _startIndex, int _length, List< url_t > _urls )
        {
            startIndex = _startIndex;
            length     = _length;
            urls       = _urls;
        }
        [M(O.AggressiveInlining)] public static sent_t CreateEmpty() => new sent_t();

        public int           startIndex { [M(O.AggressiveInlining)] get; private set; }
        public int           length     { [M(O.AggressiveInlining)] get; private set; }
        public List< url_t > urls       { [M(O.AggressiveInlining)] get; private set; }

        [M(O.AggressiveInlining)] internal void SetLength( int _length ) => length = _length;
        [M(O.AggressiveInlining)] internal void AppendUrl( url_t url )
        {
            if ( urls == null )
            {
                urls = new List< url_t >( DEFAULT_URLS_LIST_CAPACITY );
            }
            urls.Add( url );
        }
        [M(O.AggressiveInlining)] internal void SetAsFirst()
        {
            startIndex = 0;
            length     = 0;
            urls       = null;
        }
        [M(O.AggressiveInlining)] internal void Reset( int _startIndex )
        {
            startIndex = _startIndex;
            length     = 0;
            urls       = null;
        }

        public string GetValue( string originalText ) => originalText.Substring( startIndex, length );
        public sent_t CreateCopy()
        {
            var sent = new sent_t( this.startIndex, this.length );
            if ( this.urls != null )
            {
                sent.urls = this.urls.ToList();
            }
            return (sent);
        }
        public override string ToString()
        {
            if ( urls != null )
                return ("[" + startIndex + ":" + length + "], url's: " + urls.Count);
            return ("[" + startIndex + ":" + length + "]");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    unsafe public struct sent_struct_t
    {
        public char* startPtr;
        public int   length;
        public List< url_struct_t > urls;
    }

    /// <summary>
    /// 
    /// </summary>
    unsafe public sealed class SentSplitter : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public delegate void ProcessSentCallbackDelegate( sent_t sent );

        /// <summary>
        /// 
        /// </summary>
        private sealed class dot_vicinity_t
        {
            //public dot_vicinity_t( int DEFAULT_WORDS_CAPACITY ) => _Words = new DirectAccessList< word_t >( DEFAULT_WORDS_CAPACITY );

            public  ss_word_t _HeadWord;   //head-word (in vicinity of dot)
            private ss_word_t _EndWord;    //current end-word 
            public  ss_word_t _LeftWord;   //left-word  (from dot)
            public  ss_word_t _RightWord;  //right-word (from dot)
            public  int       _WordsCount; //total count of words
            //public readonly DirectAccessList< word_t > _Words;

            public bool HasWords { [M(O.AggressiveInlining)] get => (_HeadWord != null); }

            [M(O.AggressiveInlining)] public void Insert2Head( ss_word_t word )
            {
                if ( _HeadWord == null )
                {
                    _HeadWord = word;
                    _EndWord  = word;
                }
                else
                {
                    word.next      = _HeadWord;
                    _HeadWord.prev = word;
                    _HeadWord      = word;
                }
                _WordsCount++;
                //_Words.Insert2Head( word );
            }
            [M(O.AggressiveInlining)] public void Add( ss_word_t word )
            {
                if ( _HeadWord == null )
                {
                    _HeadWord = word;
                    _EndWord  = word;
                }
                else
                {                    
                    _EndWord.next = word;
                    word.prev     = _EndWord;
                    _EndWord      = word;
                }
                _WordsCount++;
                //_Words.Add( word );
            }

            [M(O.AggressiveInlining)] public void FixupLeftWord() => _LeftWord = _EndWord;
            [M(O.AggressiveInlining)] public void FixupRightWord()
            {
                if ( _LeftWord != null )
                {
                    _RightWord = _LeftWord.next;
                }
                else
                {
                    _RightWord = _HeadWord;
                }
            }
            public bool HasWordAfterLeftWord { [M(O.AggressiveInlining)] get => (_LeftWord != null && _LeftWord.next != null); }
            public bool HasLeftWord { [M(O.AggressiveInlining)] get => (_LeftWord != null); }
            public bool HasRightWord { [M(O.AggressiveInlining)] get => (_RightWord != null); }

            [M(O.AggressiveInlining)] public void Reset()
            {
                _HeadWord   = null;
                _EndWord    = null;
                _LeftWord   = null;
                _RightWord  = null;
                _WordsCount = 0;
                //_Words.Clear();
            }
        }

        #region [.cctor().]
        private static readonly char* MAX_PTR = (char*) (0xffffffffFFFFFFFF);
        private static readonly char* MIN_PTR = (char*) (0x0);

        private static CharType* _CTM;
        private static char*     _UIM;
        static SentSplitter()
        {
            _CTM = xlat_Unsafe.Inst._CHARTYPE_MAP;
            _UIM = xlat_Unsafe.Inst._UPPER_INVARIANT_MAP;
        }
        #endregion

        #region [.private field's.]
        private const int DEFAULT_LIST_CAPACITY   = 100;
        private const int NGRAM_MIN_LENGTH_2_LEFT = 3;
        private readonly SentSplitterModel     _Model;         //model 
        private /*readonly*/ bool              _SplitBySmiles; //split sent's by smile's
        private readonly int                   _NgramMaxLength2Left;  //max ngram's length to left
        private readonly int                   _NgramMaxLength2Right; //max ngram's length to right
        private readonly dot_vicinity_t        _DotVicinity;   //vicinity of dot data
        private readonly UrlDetector           _UrlDetector;   //url-detecor
        private readonly List< sent_t >        _Sents;         //list of sent-class'es
        private readonly List< sent_struct_t > _Sentstructs;   //list of sent-structure's
        private readonly StringBuilder         _StringBuilder; //buffer string builder
        private readonly char[]                _Buffer;    //buffer for smile's & etc.
        private readonly GCHandle              _BufferGCHandle;
        private char*                          _BufferPtrBase; //pointer to buffer
        private readonly SentCharType*         _SCTM; //_Model.SENTCHARTYPE_MAP
        private sent_t                         _Sent; //current open sent
        private char*                          _BASE; //start pointer into text
        private char*                          _Ptr;  //current pointer into text
        private int                            _OpenBrakets; //open bracket's count (on the left)
        private int                            _OpenQuotas; //open twin quota's count (on the left)        
        private bool                           _OpenQuotaDoubleSided; //open unpaired quota's (on the left)
        private List< url_struct_t >           _Urls;
        private int                            _UrlIndex;
        private char*                          _EndUrlPtr;
        private ProcessSentCallbackDelegate    _OuterProcessSentCallback_Delegate;
        #endregion

        #region [.ctor().]
        public SentSplitter( SentSplitterConfig config )
        {
            _Model          = config.Model;
            _SplitBySmiles  = config.SplitBySmiles;
            _UrlDetector    = new UrlDetector( config.UrlDetectorConfig );

            _Sents          = new List< sent_t >( DEFAULT_LIST_CAPACITY );            
            _StringBuilder  = new StringBuilder();
            _Sent           = new sent_t();
            _Sentstructs    = new List< sent_struct_t >( DEFAULT_LIST_CAPACITY );

            _NgramMaxLength2Left  = NGRAM_MIN_LENGTH_2_LEFT;
            _NgramMaxLength2Right = _Model.GetNgramMaxLength() - 1;
            _DotVicinity          = new dot_vicinity_t( /*_NgramMaxLength*/ );

            _SCTM = _Model._SENTCHARTYPE_MAP;

            _Buffer         = new char[ _Model.GetValuesMaxLength() + 1 ];
            _BufferGCHandle = GCHandle.Alloc( _Buffer, GCHandleType.Pinned );
            _BufferPtrBase  = (char*) _BufferGCHandle.AddrOfPinnedObject().ToPointer();
        }

        ~SentSplitter() => DisposeNativeResources();
        public void Dispose()
        {
            DisposeNativeResources();
            GC.SuppressFinalize( this );
        }
        private void DisposeNativeResources()
        {
            if ( _BufferPtrBase != null )
            {
                _BufferGCHandle.Free();
                _BufferPtrBase = null;
            }
        }
        #endregion

        public bool SplitBySmiles
        {
            get => _SplitBySmiles;
            set => _SplitBySmiles = value;
        }

        public void AllocateSents( string text, ProcessSentCallbackDelegate processSentCallback )
        {
            _OuterProcessSentCallback_Delegate = processSentCallback;

            _Sent.SetAsFirst();
            _OpenBrakets = 0;
            _OpenQuotas  = 0;
            _OpenQuotaDoubleSided = false;

            fixed ( char* _base = text )
            {
                _BASE = _base;

                _Urls           = _UrlDetector.AllocateUrls( _base );
                _UrlIndex       = 0;
                _EndUrlPtr      = MIN_PTR;
                var startUrlPtr = (_UrlIndex < _Urls.Count) ? _Urls[ _UrlIndex ].startPtr : MAX_PTR;                

                for ( _Ptr = _BASE; ; )
                {
                    #region [.skip allocated url's.]
                    if ( startUrlPtr <= _Ptr )
                    {
                        var url = _Urls[ _UrlIndex ];
                        _Sent.AppendUrl( url_t.to_url( url, _BASE ) );
                        _Ptr = _EndUrlPtr = startUrlPtr + url.length;
                        _UrlIndex++;
                        startUrlPtr = (_UrlIndex < _Urls.Count) ? _Urls[ _UrlIndex ].startPtr : MAX_PTR;                        
                        continue;
                    }
                    #endregion

                    var ch  = *_Ptr;
                    #region [.end-of-text.]
                    if ( ch == '\0' )
                    {
                        break;
                    }
                    #endregion

                    var sct = *(_SCTM + ch);
                    #region [.unconditional.]
                    if ( sct == SentCharType.Unconditional )
                    {
                        _Ptr++;
                        SetSentAndCallback();
                        continue;
                    }
                    #endregion

                    var ct = *(_CTM + ch);
                    #region [.open bracket's & quota count.]
                    //if ( (ct & CharType.IsQuoteOrBracket) == CharType.IsQuoteOrBracket )
                    switch ( (ct & CharType.IsQuoteOrBracket) )
                    {
                        case CharType.IsQuote:
                        case CharType.IsBracket:
                            if ( (ct & CharType.IsBracketLeft) == CharType.IsBracketLeft )
                                _OpenBrakets++;
                            else 
                            if ( (ct & CharType.IsBracketRight) == CharType.IsBracketRight )
                                _OpenBrakets--;
                            else //if ( (ct & CharType.IsQuote) == CharType.IsQuote )
                            {
                                if ( (ct & CharType.IsQuoteLeft) == CharType.IsQuoteLeft )
                                {
                                    if ( (ct & CharType.IsQuoteDoubleSided) == CharType.IsQuoteDoubleSided )
                                        _OpenQuotaDoubleSided = !_OpenQuotaDoubleSided;
                                    else
                                        _OpenQuotas++;
                                }
                                else
                                if ( (ct & CharType.IsQuoteRight) == CharType.IsQuoteRight )
                                {
                                    if ( (ct & CharType.IsQuoteDoubleSided) == CharType.IsQuoteDoubleSided )
                                        _OpenQuotaDoubleSided = !_OpenQuotaDoubleSided;
                                    else
                                        _OpenQuotas--;
                                }
                            }
                        break;
                    }                    
                    #endregion


                    #region [.символы … ? !.]
                    if ( (sct & SentCharType.ExcludeInBracketAndQuote) == SentCharType.ExcludeInBracketAndQuote )
                    {
                        if ( IsEndOfSentTDQMEP( sct ) )
                        {
                            SkipFollowPunctuation();
                            SetSentAndCallback();
                            continue;
                        }
                        _Ptr++;
                        //считаем что с букв …?! больше ничего не начинается
                        continue;
                    }
                    #endregion

                    #region [.эмотиконы (smiles).]
                    if ( _SplitBySmiles &&( sct & SentCharType.SmileBegin ) == SentCharType.SmileBegin )
                    {
                        var smile_length = TryGetSmileLength();
                        if ( 0 < smile_length )
                        {
                            _Ptr += smile_length;
                            SetSentAndCallback();
                            continue;
                        }
                    }
                    #endregion

                    #region [.all! (inc. potential url).]
                    if ( (sct & SentCharType.Dot) == SentCharType.Dot )
                    {
                        #region [.слева и справа цифра (без пробела) – не конец.]
                        if ( IsPreviousAndNextCharDigit() )
                        {
                            _Ptr++;
                            continue;
                        }
                        #endregion

                        #region [.-!-tokenize vicinity-of-dot-!-.]
                        if ( !TokenizeVicinityOfDot() )
                        {
                            _Ptr++;
                            continue;
                        }
                        #endregion

                        #region [.BeforeProperOrNumber & BeforeNoProper.]
                        var offset = Try_BeforeProperOrNumber_BeforeNoProper();
                        if ( offset.HasValue )
                        {
                            if ( offset.Value < 0 )
                            {   //sent-not-end
                                _Ptr -= offset.Value;
                            }
                            else
                            {   //sent-end
                                _Ptr += offset.Value;
                                SetSentAndCallback();
                            }
                            continue;
                        }
                        #endregion

                        #region [.three-dots.]
                        offset = TryGetThreeDotsLength();
                        if ( offset.HasValue )
                        {
                            if ( offset.Value < 0 )
                            {   //sent-not-end
                                _Ptr -= offset.Value;
                            }
                            else
                            {   //sent-end
                                _Ptr += offset.Value;
                                SetSentAndCallback();
                            }
                            continue;
                        }
                        #endregion

                        #region [.single-upper-char.]
                        offset = TryGetSingleUpperChar();
                        if ( offset.HasValue )
                        {
                            if ( offset.Value < 0 )
                            {   //sent-not-end
                                _Ptr -= offset.Value;
                            }
                            else
                            {   //sent-end
                                _Ptr += offset.Value;
                                SetSentAndCallback();
                            }
                            continue;
                        }
                        #endregion

                        #region [.interjection.]
                        if ( IsInterjection() )
                        {
                            _Ptr++;
                            SetSentAndCallback();
                            continue;
                        }
                        #endregion

                        #region [.file-extensions.]
                        var len = TryGetFileExtensionLength();
                        if ( 0 < len )
                        {
                            _Ptr += len;
                            continue;
                        }
                        #endregion

                        #region [.yandex-combinations.]
                        len = TryGetYandexCombinationsLength();
                        if ( 0 < len )
                        {
                            _Ptr += len;
                            continue;
                        }
                        #endregion

                        #region [.other-situation.]
                        offset = TryOtherSituation();
                        if ( offset.HasValue )
                        {
                            if ( offset.Value < 0 )
                            {   //sent-not-end
                                _Ptr -= offset.Value;
                            }
                            else
                            {   //sent-end
                                _Ptr += offset.Value;
                                SetSentAndCallback();
                            }
                            continue;
                        }
                        #endregion

                        #region [.end-of-quoting.]
                        len = TryEndOfQuotingLength();
                        if ( 0 < len )
                        {
                            _Ptr += len;
                            continue;
                        }
                        #endregion

                        #region [.list's.]
                        len = TryListLength();
                        if ( 0 < len )
                        {
                            _Ptr += len;
                            continue;
                        }
                        #endregion

                        #region [.если дошли сюда – конец (а может пиздец).]
                        _Ptr++;
                        SetSentAndCallback();
                        continue;
                        #endregion
                    }
                    #endregion

                    #region [.move-to-next-char.]
                    _Ptr++;
                    #endregion
                }

                SetLastSentAndCallback( text.Length );
            }

            _OuterProcessSentCallback_Delegate = null;
        }
        public List< sent_t > AllocateSents( string text, bool splitBySmiles )
        {
            _SplitBySmiles = splitBySmiles;

            return (AllocateSents( text ));
        }
        public List< sent_t > AllocateSents( string text )
        {
            _Sents.Clear();
            _Sent.SetAsFirst();
            _OpenBrakets = 0;
            _OpenQuotas  = 0;
            _OpenQuotaDoubleSided = false;

            fixed ( char* _base = text )
            {
                _BASE = _base;

                _Urls           = _UrlDetector.AllocateUrls( _base );
                _UrlIndex       = 0;
                _EndUrlPtr      = MIN_PTR;
                var startUrlPtr = (_UrlIndex < _Urls.Count) ? _Urls[ _UrlIndex ].startPtr : MAX_PTR;                

                for ( _Ptr = _BASE; ; )
                {
                    #region [.skip allocated url's.]
                    if ( startUrlPtr <= _Ptr )
                    {
                        var url = _Urls[ _UrlIndex ];
                        _Sent.AppendUrl( url_t.to_url( url, _BASE ) );
                        _Ptr = _EndUrlPtr = startUrlPtr + url.length;
                        _UrlIndex++;
                        startUrlPtr = (_UrlIndex < _Urls.Count) ? _Urls[ _UrlIndex ].startPtr : MAX_PTR;                        
                        continue;
                    }
                    #endregion

                    var ch  = *_Ptr;
                    #region [.end-of-text.]
                    if ( ch == '\0' )
                    {
                        break;
                    }
                    #endregion

                    var sct = *(_SCTM + ch);
                    #region [.unconditional.]
                    if ( sct == SentCharType.Unconditional )
                    {
                        _Ptr++;
                        CreateSentAndPut2List();
                        continue;
                    }
                    #endregion

                    var ct = *(_CTM + ch);
                    #region [.open bracket's & quota count.]
                    //if ( (ct & CharType.IsQuoteOrBracket) == CharType.IsQuoteOrBracket )
                    switch ( (ct & CharType.IsQuoteOrBracket) )
                    {
                        case CharType.IsQuote:
                        case CharType.IsBracket:
                            if ( (ct & CharType.IsBracketLeft) == CharType.IsBracketLeft )
                                _OpenBrakets++;
                            else 
                            if ( (ct & CharType.IsBracketRight) == CharType.IsBracketRight )
                                _OpenBrakets--;
                            else //if ( (ct & CharType.IsQuote) == CharType.IsQuote )
                            {
                                if ( (ct & CharType.IsQuoteLeft) == CharType.IsQuoteLeft )
                                {
                                    if ( (ct & CharType.IsQuoteDoubleSided) == CharType.IsQuoteDoubleSided )
                                        _OpenQuotaDoubleSided = !_OpenQuotaDoubleSided;
                                    else
                                        _OpenQuotas++;
                                }
                                else
                                if ( (ct & CharType.IsQuoteRight) == CharType.IsQuoteRight )
                                {
                                    if ( (ct & CharType.IsQuoteDoubleSided) == CharType.IsQuoteDoubleSided )
                                        _OpenQuotaDoubleSided = !_OpenQuotaDoubleSided;
                                    else
                                        _OpenQuotas--;
                                }
                            }
                        break;
                    }                    
                    #endregion


                    #region [.символы … ? !.]
                    if ( (sct & SentCharType.ExcludeInBracketAndQuote) == SentCharType.ExcludeInBracketAndQuote )
                    {
                        if ( IsEndOfSentTDQMEP( sct ) )
                        {
                            SkipFollowPunctuation();
                            CreateSentAndPut2List();
                            continue;
                        }
                        _Ptr++;
                        //считаем что с букв …?! больше ничего не начинается
                        continue;

                        #region commented
                        /*
                        #region [..]
                        var offset = TryGetThreeDotLength( sct );
                        if ( offset.HasValue )
                        {
                            if ( offset.Value < 0 )
                            {   //sent-not-end
                                _Ptr -= offset.Value;
                            }
                            else
                            {   //sent-end
                                _Ptr += offset.Value;
                                CreateSentAndPut2List();
                            }
                            continue;
                        }
                        #endregion

                        if ( !IsInBracketsOrQuotesTDQMEP() )
                        {
                            SkipFollowPunctuation();
                            CreateSentAndPut2List();
                            continue;
                        }
                        _Ptr++;
                        //считаем что с букв …?! больше ничего не начинается
                        continue;
                        */
                        #endregion
                    }
                    #endregion

                    #region [.эмотиконы (smiles).]
                    if ( _SplitBySmiles &&( sct & SentCharType.SmileBegin ) == SentCharType.SmileBegin )
                    {
                        var smile_length = TryGetSmileLength();
                        if ( 0 < smile_length )
                        {
                            _Ptr += smile_length;
                            CreateSentAndPut2List();
                            continue;
                        }
                    }
                    #endregion

                    #region commented
                    /*
                    #region [.potential url by colon-':'.]
                    if ( (sct & SentCharType.Colon) == SentCharType.Colon )
                    {
                        #region [.urls.]
                        var url = _UrlDetector.AllocateSingleUrlByColon( _Ptr );
                        if  ( url != null )
                        {
                            _Sent.AppendUrl( url );
                            _Ptr = _BASE + url.startIndex + url.length;
                            continue;
                        }
                        #endregion
                        _Ptr++;
                        //считаем что с буквы : больше ничего не начинается
                        continue;
                    }
                    #endregion
                    */
                    #endregion

                    #region [.all! (inc. potential url).]
                    if ( (sct & SentCharType.Dot) == SentCharType.Dot )
                    {
                        #region [.слева и справа цифра (без пробела) – не конец.]
                        if ( IsPreviousAndNextCharDigit() )
                        {
                            _Ptr++;
                            continue;
                        }
                        #endregion

                        #region commented
                        /*
                        #region [.urls.]
                        var url = _UrlDetector.AllocateSingleUrlByDot( _Ptr );
                        if ( url != null )
                        {
                            _Sent.AppendUrl( url );
                            _Ptr = _BASE + url.startIndex + url.length;
                            continue;
                        }
                        #endregion
                        */
                        #endregion

                        #region [.-!-tokenize vicinity-of-dot-!-.]
                        if ( !TokenizeVicinityOfDot() )
                        {
                            _Ptr++;
                            continue;
                        }
                        #endregion

                        #region [.BeforeProperOrNumber & BeforeNoProper.]
                        var offset = Try_BeforeProperOrNumber_BeforeNoProper();
                        if ( offset.HasValue )
                        {
                            if ( offset.Value < 0 )
                            {   //sent-not-end
                                _Ptr -= offset.Value;
                            }
                            else
                            {   //sent-end
                                _Ptr += offset.Value;
                                CreateSentAndPut2List();
                            }
                            continue;
                        }
                        #endregion

                        #region [.three-dots.]
                        offset = TryGetThreeDotsLength();
                        if ( offset.HasValue )
                        {
                            if ( offset.Value < 0 )
                            {   //sent-not-end
                                _Ptr -= offset.Value;
                            }
                            else
                            {   //sent-end
                                _Ptr += offset.Value;
                                CreateSentAndPut2List();
                            }
                            continue;
                        }
                        #endregion

                        #region [.single-upper-char.]
                        offset = TryGetSingleUpperChar();
                        if ( offset.HasValue )
                        {
                            if ( offset.Value < 0 )
                            {   //sent-not-end
                                _Ptr -= offset.Value;
                            }
                            else
                            {   //sent-end
                                _Ptr += offset.Value;
                                CreateSentAndPut2List();
                            }
                            continue;
                        }
                        #endregion

                        #region [.interjection.]
                        if ( IsInterjection() )
                        {
                            _Ptr++;
                            CreateSentAndPut2List();
                            continue;
                        }
                        #endregion

                        #region [.file-extensions.]
                        var len = TryGetFileExtensionLength();
                        if ( 0 < len )
                        {
                            _Ptr += len;
                            continue;
                        }
                        #endregion

                        #region [.yandex-combinations.]
                        len = TryGetYandexCombinationsLength();
                        if ( 0 < len )
                        {
                            _Ptr += len;
                            continue;
                        }
                        #endregion

                        #region [.other-situation.]
                        offset = TryOtherSituation();
                        if ( offset.HasValue )
                        {
                            if ( offset.Value < 0 )
                            {   //sent-not-end
                                _Ptr -= offset.Value;
                            }
                            else
                            {   //sent-end
                                _Ptr += offset.Value;
                                CreateSentAndPut2List();
                            }
                            continue;
                        }
                        #endregion

                        #region [.end-of-quoting.]
                        len = TryEndOfQuotingLength();
                        if ( 0 < len )
                        {
                            _Ptr += len;
                            continue;
                        }
                        #endregion

                        #region [.list's.]
                        len = TryListLength();
                        if ( 0 < len )
                        {
                            _Ptr += len;
                            continue;
                        }
                        #endregion

                        #region [.если дошли сюда – конец (а может пиздец).]
                        _Ptr++;
                        CreateSentAndPut2List();
                        continue;
                        #endregion
                    }
                    #endregion
                     
                //MOVE_2_NEXT_CHAR: 
                    #region [.move-to-next-char.]
                    _Ptr++;
                    #endregion
                }

                CreateLastSentAndPut2List( text.Length );
            }

            return (_Sents);
        }

        /// <summary>
        /// 
        /// </summary>
        [M(O.AggressiveInlining)] private void CreateSentAndPut2List()
        {
            var startIndex = _Sent.startIndex + _Sent.length;
            var length     = (int) (_Ptr - _BASE - startIndex);
            if ( length <= 0 )
                return;

            _Sent.SetLength( length );
            if ( !IsCurrentSentContainsPunctuationOrWhitespace() )
            {
                _Sents.Add( _Sent );
            }
            var sent = new sent_t( _Sent.startIndex + _Sent.length );

            _Sent = sent;
            _OpenBrakets = 0;
            _OpenQuotas  = 0;
            _OpenQuotaDoubleSided = false;
        }
        [M(O.AggressiveInlining)] private void CreateLastSentAndPut2List( int text_length )
        {
            var startIndex = _Sent.startIndex + _Sent.length;
            var length     = text_length - startIndex;
            if ( length <= 0 )
                return;

            _Sent.SetLength( length );
            if ( !IsCurrentSentContainsPunctuationOrWhitespace() )
            {
                _Sents.Add( _Sent );
            }

            //_Sent = sent;
            //_OpenBrakets = 0;
            //_OpenQuotas  = false;
        }
        [M(O.AggressiveInlining)] private void SetSentAndCallback()
        {
            var startIndex = _Sent.startIndex + _Sent.length;
            var length     = (int) (_Ptr - _BASE - startIndex);
            if ( length <= 0 )
                return;

            _Sent.SetLength( length );
            if ( !IsCurrentSentContainsPunctuationOrWhitespace() )
            {
                _OuterProcessSentCallback_Delegate( _Sent );
            }
            _Sent.Reset( _Sent.startIndex + _Sent.length );

            _OpenBrakets = 0;
            _OpenQuotas  = 0;
            _OpenQuotaDoubleSided = false;
        }
        [M(O.AggressiveInlining)] private void SetLastSentAndCallback( int text_length )
        {
            var startIndex = _Sent.startIndex + _Sent.length;
            var length     = text_length - startIndex;
            if ( length <= 0 )
                return;

            _Sent.SetLength( length );
            if ( !IsCurrentSentContainsPunctuationOrWhitespace() )
            {
                _OuterProcessSentCallback_Delegate( _Sent );
            }

            //_Sent = sent;
            //_OpenBrakets = 0;
            //_OpenQuotas  = false;
        }

        [M(O.AggressiveInlining)] private bool IsEndOfSentTDQMEP( SentCharType sct )
        {
            //- '…' -//
            if ( (sct & SentCharType.ThreeDot) == SentCharType.ThreeDot )
            {
                var wasSomePunctuation = false;
                var ct = default(CharType);
                for ( var ptr = _Ptr + 1; ; ptr++ )
                {
                    var ch = *ptr;
                    if ( ch == '\0' )
                        return (true);

                    ct = *(_CTM + ch);
                    if ( (ct & CharType.IsBracketRight) == CharType.IsBracketRight )
                    {
                        if ( wasSomePunctuation )
                            return (true);
                        return (_OpenBrakets <= 0);
                    }
                    if ( (*(_SCTM + ch) & SentCharType.AfterThreeDotAllowedPunctuation) == SentCharType.AfterThreeDotAllowedPunctuation )
                    {
                        wasSomePunctuation = true;
                    }
                    else
                    if ( (ct & CharType.IsWhiteSpace) != CharType.IsWhiteSpace )
                    {
                        break;
                    }
                }

                if ( (ct & CharType.IsLower) == CharType.IsLower )
                {
                    return (false);
                }
            }
            //- '!', '?' -//
            else 
            {
                if ( _OpenBrakets == 1 )
                    return (false);
                if ( _OpenQuotas == 1 )
                    return (false);
                if ( _OpenQuotaDoubleSided )
                    return (false);

                //has on right bracket and ,:
                for ( var ptr = _Ptr + 1; ; ptr++ )
                {
                    var ch = *ptr;
                    if ( ch == '\0' )
                        break;
                    var ct = *(_CTM + ch);
                    if ( (ct & CharType.IsBracketLeft ) == CharType.IsBracketLeft ||
                         (ct & CharType.IsBracketRight) == CharType.IsBracketRight
                       )
                    {
                        for ( ptr++; ; ptr++ )
                        {
                            ch = *ptr;
                            if ( ch == '\0' )
                            {
                                return (false);
                            }
                            if ( (*(_SCTM + ch) & SentCharType.AfterBracketAllowedPunctuation4QMEP) == SentCharType.AfterBracketAllowedPunctuation4QMEP )
                            {
                                return (false);
                            }
                            ct = *(_CTM + ch);
                            if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                                 (ct & CharType.IsPunctuation) != CharType.IsPunctuation )
                            {
                                return (true);
                            }
                        }
                        
                    }
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation )
                    {
                        break;
                    }
                }
                //return (!IsInBracketsOrQuotesQMEP());
            }

            return (true);
        }
        /*private bool IsInBracketsOrQuotesQMEP()
        {
            if ( 0 < _OpenBrakets )
            {
                for ( var ptr = _Ptr + 1; *ptr != '\0'; ptr++ )
                {
                    var ct = *(_CTM + *ptr);
                    if ( (ct & CharType.IsBracketRight) == CharType.IsBracketRight )
                        return (true);
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation )
                        break;
                }
            }
            else 
            if ( 0 < _OpenQuotas )
            {
                for ( var ptr = _Ptr + 1; ; ptr++ )
                {
                    var ch = *ptr;
                    if ( ch == '\0' )
                        break;
                    var ct = *(_CTM + ch);
                    if ( (ct & CharType.IsQuoteRight) == CharType.IsQuoteRight )
                    {
                        return (true);
                    }
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation )
                    {
                        break;
                    }
                }
            }
            else 
            if ( _OpenQuotaDoubleSided )
            {
                for ( var ptr = _Ptr + 1; ; ptr++ )
                {
                    var ch = *ptr;
                    if ( ch == '\0' )
                        break;
                    var ct = *(_CTM + ch);
                    if ( (ct & CharType.IsQuoteDoubleSided) == CharType.IsQuoteDoubleSided )
                    {
                        return (true);
                    }
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation )
                    {
                        break;
                    }
                }
            }
            return (false);
        }*/
        /*private bool IsInBracketsOrQuotesTDQMEP()
        {
            if ( 0 < _OpenBrakets )
            {
                for ( var ptr = _Ptr + 1; *ptr != '\0'; ptr++ )
                {
                    var ct = *(_CTM + *ptr);
                    if ( (ct & CharType.IsBracketRight) == CharType.IsBracketRight )
                        return (true);
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation )
                        break;
                }
            }
            else if ( _OpenQuotas )
            {
                for ( var ptr = _Ptr + 1; ; ptr++ )
                {
                    var ch = *ptr;
                    if ( ch == '\0' )
                        break;
                    var ct = *(_CTM + ch);
                    if ( (ct & CharType.IsQuote) == CharType.IsQuote )
                    {
                        //return (true);
                        for ( ptr++; ; ptr++ )
                        {
                            ch = *ptr;
                            if ( ch == '\0' )
                            {
                                return (false);
                            }
                            if ( xlat.IsHyphen( ch ) )
                            {
                                return (true);
                            }
                            ct = *(_CTM + ch);
                            if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                                 (ct & CharType.IsPunctuation) != CharType.IsPunctuation )
                            {
                                return (false);
                            }
                        }
                        
                    }
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation )
                    {
                        break;
                    }
                }
            }
            return (false);
        }*/
        [M(O.AggressiveInlining)] private int  TryGetSmileLength()
        {
            #region [.sent-not-end if has opened left bracket's and this right bracket.]
            if ( (0 < _OpenBrakets) &&
                 (*(_CTM + *_Ptr) & CharType.IsBracketRight) == CharType.IsBracketRight
               )
            {
                return (0);
            }
            #endregion

            #region [.check -smile- on the right.]
            var buffer_index = 0;
            for ( var right_offset = 0; ; right_offset++ )
            {
                var ch = *(_Ptr + right_offset);
                if ( ch == '\0' )
                {
                    break;
                }

                if ( (*(_CTM + ch) & CharType.IsWhiteSpace) == CharType.IsWhiteSpace )
                {
                    continue;
                }

                if ( _Model.Smiles.ValuesMaxLength <= buffer_index )
                {
                    break;
                }

                *(_BufferPtrBase + buffer_index) = ch;
                buffer_index++;
            }

            if ( buffer_index < _Model.Smiles.ValuesMinLength )
            {
                return (0);
            }
            #endregion

            #region [.check -smile- by smile's-list.]
            switch ( _Model.Smiles.DiffBetweenMixAndMaxLength )
            {
                case 0:
                    return (TryCheckSmileInBuffer( buffer_index ));
                case 1:
                    {
                    var _right_offset = TryCheckSmileInBuffer( buffer_index );
                    if ( _right_offset != 0 )
                        return (_right_offset);
                    return (TryCheckSmileInBuffer( buffer_index - 1 ));
                    }
                default:
                    for ( int i = _Model.Smiles.ValuesMinLength; i <= buffer_index; i++ )
                    {
                        var _right_offset = TryCheckSmileInBuffer( i );
                        if ( _right_offset != 0 )
                            return (_right_offset);
                    }
                break;
            }

            return (0);
            #endregion
        }
        [M(O.AggressiveInlining)] private int  TryCheckSmileInBuffer( int buffer_index )
        {
            smile_t smile;
            if ( _Model.Smiles.Values.TryGetValue(
                    _StringBuilder.Clear().Append( _Buffer, 0, buffer_index ).ToString()
                    , out smile ) 
               )
            {
                if ( smile.SpaceBefore && !IsPreviousCharWhitespace() )
                {
                    return (0);
                }

                //skip all punctuation & whitespace's after/with smile
                for ( buffer_index++; ; buffer_index++ )
                {
                    var ch = *(_Ptr + buffer_index);
                    if ( ch == '\0' )
                    {                            
                        break;
                    }

                    var ct = *(_CTM + ch);
                    if ( (ct & CharType.IsWhiteSpace ) != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation
                       )
                    {
                        break;
                    }
                }

                buffer_index--;
                return (buffer_index);
            }

            return (0);
        }
        [M(O.AggressiveInlining)] private bool IsPreviousCharWhitespace()
        {
            if ( _Ptr == _BASE )
                return (true);

            var ct = *(_CTM + *(_Ptr - 1));
            if ( (ct & CharType.IsWhiteSpace) == CharType.IsWhiteSpace )
                return (true);

            return (false);
        }
        [M(O.AggressiveInlining)] private void SkipFollowPunctuation()
        {
            for ( _Ptr++; ; _Ptr++ )
            {
                var ch = *_Ptr;
                if ( (ch == '\0') || 
                     (*(_CTM + ch) & CharType.IsPunctuation) != CharType.IsPunctuation || 
                     (*(_SCTM + ch) & SentCharType.Unconditional) == SentCharType.Unconditional
                   )
                {
                    return;
                }
            }
        }

        [M(O.AggressiveInlining)] private bool IsPreviousAndNextCharDigit()
        {
            if ( (*(_CTM + *(_Ptr + 1)) & CharType.IsDigit) == CharType.IsDigit )
            {
                if ( _BASE == _Ptr )
                {
                    return (true);
                }
                if ( (*(_CTM + *(_Ptr - 1)) & CharType.IsDigit) == CharType.IsDigit )
                {
                    return (true);
                }
            }
            return (false);
        }

        [M(O.AggressiveInlining)] private bool TokenizeVicinityOfDot()
        {
            //(*_Ptr == '.')
            _DotVicinity.Reset();

            #region [.to-left.]
            var leftBorder = GetMaxPtr( _BASE + _Sent.startIndex, _EndUrlPtr );
            if ( leftBorder < _Ptr )
            {
                var checkUnstickFromDigits = false;
                for ( char* start = _Ptr, left_ptr = start - 1; ; left_ptr-- )
                {
                    #region [.start-of-sent.]
                    if ( left_ptr <= leftBorder )
                    {
                        #region [.create word.]
                        var ct0 = *(_CTM + *left_ptr);
                        if ( (ct0 & CharType.IsWhiteSpace) == CharType.IsWhiteSpace )
                        {
                            left_ptr++;
                        }

                        var length = (int) (start - left_ptr);
                        if ( 0 < length )
                        {
                            if ( (ct0 & CharType.IsPunctuation) == CharType.IsPunctuation )
                            {
                                var word = new ss_word_t( left_ptr + 1, length );
                                //слово (слева или справа от точки), содержащие внутри себя слеш (без пробелов)  – не конец
                                /*if ( !_DotVicinity.HasWords && IsContainsSlash( word.startPtr, word.length ) )
                                    return (false);
                                word.valueOriginal = new string( word.startPtr, 0, word.length );*/
                                _DotVicinity.Insert2Head( word );

                                //punctuation
                                word = new ss_word_t( left_ptr, 1 );
                                _DotVicinity.Insert2Head( word );
                            }
                            else
                            {
                                var word = new ss_word_t( left_ptr, length + 1 );
                                //слово (слева или справа от точки), содержащие внутри себя слеш (без пробелов)  – не конец
                                /*if ( !_DotVicinity.HasWords && IsContainsSlash( word.startPtr, word.length ) )
                                    return (false);
                                word.valueOriginal = new string( word.startPtr, 0, word.length );*/
                                _DotVicinity.Insert2Head( word );
                            }
                        }
                        #endregion

                        break;
                    }
                    #endregion
               
                    var ch = *left_ptr;
                    var ct = *(_CTM + ch);
                    if ( ch == '.' )
                    {
                        checkUnstickFromDigits = false;
                        #region [.create word.]
                        var length = (int) (start - left_ptr);
                        if ( 0 < length )
                        {
                            var word = new ss_word_t( left_ptr + 1, length );
                            //слово (слева или справа от точки), содержащие внутри себя слеш (без пробелов)  – не конец
                            /*if ( !_DotVicinity.HasWords && IsContainsSlash( word.startPtr, word.length ) )
                                return (false);
                            word.valueOriginal = new string( word.startPtr, 0, word.length );*/
                            _DotVicinity.Insert2Head( word );
                            if ( _NgramMaxLength2Left <= _DotVicinity._WordsCount )
                                break;
                            start = left_ptr;
                        }
                        #endregion
                    }
                    else
                    if ( (ct & CharType.IsWhiteSpace) == CharType.IsWhiteSpace )
                    {
                        checkUnstickFromDigits = false;
                        #region [.create word.]
                        var length = (int) (start - left_ptr);
                        if ( 0 < length )
                        {
                            var word = new ss_word_t( left_ptr + 1, length );
                            //слово (слева или справа от точки), содержащие внутри себя слеш (без пробелов)  – не конец
                            /*if ( !_DotVicinity.HasWords && IsContainsSlash( word.startPtr, word.length ) )
                                return (false);
                            word.valueOriginal = new string( word.startPtr, 0, word.length );*/
                            _DotVicinity.Insert2Head( word );
                            if ( _NgramMaxLength2Left <= _DotVicinity._WordsCount )
                                break;
                            start = left_ptr - 1;
                        }
                        else
                        {
                            start = left_ptr;
                        }
                        #endregion
                    }
                    else 
                    if ( xlat.IsDegree( ch ) )
                    {
                        checkUnstickFromDigits = false;
                        #region [.create word.]
                        var length = (int) (start - left_ptr);
                        if ( 0 < length )
                        {
                            var word = new ss_word_t( left_ptr + 1, length );
                            //слово (слева или справа от точки), содержащие внутри себя слеш (без пробелов) – не конец
                            /*if ( !_DotVicinity.HasWords && IsContainsSlash( word.startPtr, word.length ) )
                                return (false);
                            word.valueOriginal = new string( word.startPtr, 0, word.length );*/
                            _DotVicinity.Insert2Head( word );
                            if ( _NgramMaxLength2Left <= _DotVicinity._WordsCount )
                                break;
                            start = left_ptr - 1;
                        }
                        else
                        {
                            start = left_ptr;
                        }

                        #region [.degree-word.]
                        var degreeWord = new ss_word_t( left_ptr, 1 );
                        _DotVicinity.Insert2Head( degreeWord );
                        if ( _NgramMaxLength2Left <= _DotVicinity._WordsCount )
                            break;
                        #endregion
                        #endregion
                    }
                    else 
                    if ( (ct & CharType.IsPunctuation) == CharType.IsPunctuation && 
                         (ct & CharType.IsHyphen     ) != CharType.IsHyphen //!xlat.IsHyphen( ch ) 
                       )
                    {
                        checkUnstickFromDigits = false;
                        #region [.create word.]
                        /*if ( !_DotVicinity.HasWords && xlat.IsSlash( ch ) )
                        {
                            return (false);
                        }*/
                        var length = (int) (start - left_ptr);
                        if ( 0 < length )
                        {
                            var word = new ss_word_t( left_ptr + 1, length );
                            //слово (слева или справа от точки), содержащие внутри себя слеш (без пробелов)  – не конец
                            /*if ( !_DotVicinity.HasWords && IsContainsSlash( word.startPtr, word.length ) )
                                return (false);
                            word.valueOriginal = new string( word.startPtr, 0, word.length );*/
                            _DotVicinity.Insert2Head( word );
                            if ( _NgramMaxLength2Left <= _DotVicinity._WordsCount )
                                break;
                            start = left_ptr;
                        }
                        #endregion
                    }
                    //надо отодрать, блять, написанные в плотную к цифрам «г. | п. | с. | м.» и не только эти - все отсюда - UnstickFromDigits!
                    else
                    if ( (ct & CharType.IsDigit) == CharType.IsDigit )
                    {
                        #region
                        if ( !checkUnstickFromDigits )
                        {
                            var length = (int) (start - left_ptr);
                            if ( (1 < length) /*(length == 2)*/ && xlat.IsDot( *start ) /*&& 
                                 (*(_SCTM + *(start - 1)) & SentCharType.BeforeProperOrNumberDigitsBeforeChar) == SentCharType.BeforeProperOrNumberDigitsBeforeChar*/
                               )
                            {
                                checkUnstickFromDigits = true;

                                var p = left_ptr + 1;
                                _StringBuilder.Clear();
                                switch ( length )
                                {
                                    case 2: _StringBuilder.Append( *p ).Append( *(++p) ); break;
                                    case 3: _StringBuilder.Append( *p ).Append( *(++p) ).Append( *(++p) ); break;
                                    case 4: _StringBuilder.Append( *p ).Append( *(++p) ).Append( *(++p) ).Append( *(++p) ); break;
                                    case 5: _StringBuilder.Append( *p ).Append( *(++p) ).Append( *(++p) ).Append( *(++p) ).Append( *(++p) ); break;
                                    case 6: _StringBuilder.Append( *p ).Append( *(++p) ).Append( *(++p) ).Append( *(++p) ).Append( *(++p) ).Append( *(++p) ); break;
                                    default:
                                        for ( ; p <= start; p++ )
                                        {
                                            _StringBuilder.Append( *p );
                                        }
                                    break;
                                }
                                if ( _Model.UnstickFromDigits.Contains( _StringBuilder.ToString() ) )
                                {
                                    #region [.create word.]
                                    var word = new ss_word_t( left_ptr + 1, length );
                                    _DotVicinity.Insert2Head( word );
                                    if ( _NgramMaxLength2Left <= _DotVicinity._WordsCount )
                                        break;
                                    start = left_ptr;
                                    #endregion
                                }                        
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        checkUnstickFromDigits = false;
                    }
                }
            }
            #endregion

            #region [.to-right.]
            _DotVicinity.FixupLeftWord();
            var wordsToRight = 0;
            for ( char* start = _Ptr + 1, right_ptr = start; ; right_ptr++ )
            {
                var ch = *right_ptr;
                #region [.end-of-text.]
                if ( ch == '\0' )
                {
                    #region [.create word.]
                    var length = (int) (right_ptr - start);
                    if ( 0 < length )
                    {
                        var word = new ss_word_t( start, length );
                        //слово (слева или справа от точки), содержащие внутри себя слеш (без пробелов)  – не конец
                        /*if ( !_DotVicinity.HasWordAfterLeftWord && IsContainsSlash( word.startPtr, word.length ) )
                            return (false);
                        word.valueOriginal = new string( word.startPtr, 0, word.length );*/
                        _DotVicinity.Add( word );
                    }
                    #endregion

                    break;
                }
                #endregion

                var ct = *(_CTM + ch);
                if ( ch == '.' )
                {
                    #region [.create word.]
                    var length = (int) (right_ptr - start);
                    if ( 0 < length )
                    {
                        if ( *start == '.' )
                        {
                            for ( right_ptr++; ; right_ptr++ )
                            {
                                ch = *right_ptr;
                                ct = *(_CTM + ch);
                                if ( ((ct & CharType.IsPunctuation) != CharType.IsPunctuation && 
                                      (ct & CharType.IsWhiteSpace ) != CharType.IsWhiteSpace) ||
                                     ((*(_SCTM + ch) & SentCharType.Unconditional) == SentCharType.Unconditional)
                                   )
                                {
                                    break;
                                }
                            }
                            right_ptr--;
                            length = (int) (right_ptr - start);
                        }

                        var word = new ss_word_t( start, length + 1 );
                        //слово (слева или справа от точки), содержащие внутри себя слеш (без пробелов)  – не конец
                        /*if ( !_DotVicinity.HasWordAfterLeftWord && IsContainsSlash( word.startPtr, word.length ) )
                            return (false);
                        word.valueOriginal = new string( word.startPtr, 0, word.length );*/
                        _DotVicinity.Add( word );
                        if ( _NgramMaxLength2Right <= ++wordsToRight )
                            break;
                        start = right_ptr + 1;
                    }
                    else
                    {
                        start = right_ptr;
                    }
                    #endregion
                }
                else 
                if ( (ct & CharType.IsWhiteSpace ) == CharType.IsWhiteSpace )
                {
                    #region [.create word.]
                    var length = (int) (right_ptr - start);
                    if ( 0 < length )
                    {
                        var word = new ss_word_t( start, length );
                        //слово (слева или справа от точки), содержащие внутри себя слеш (без пробелов)  – не конец
                        /*if ( !_DotVicinity.HasWordAfterLeftWord && IsContainsSlash( word.startPtr, word.length ) )
                            return (false);
                        word.valueOriginal = new string( word.startPtr, 0, word.length );*/
                        _DotVicinity.Add( word );
                        if ( _NgramMaxLength2Right <= ++wordsToRight )
                            break;
                    }
                    start = right_ptr + 1;
                    #endregion
                }
                else 
                if ( (ct & CharType.IsPunctuation) == CharType.IsPunctuation &&
                     (ct & CharType.IsHyphen     ) != CharType.IsHyphen //!xlat.IsHyphen( ch )
                   )
                {
                    #region [.create word.]
                    var length = (int) (right_ptr - start);
                    if ( 0 < length )
                    {
                        var word = new ss_word_t( start, length );
                        //слово (слева или справа от точки), содержащие внутри себя слеш (без пробелов)  – не конец
                        /*if ( !_DotVicinity.HasWordAfterLeftWord && IsContainsSlash( word.startPtr, word.length ) )
                            return (false);
                        word.valueOriginal = new string( word.startPtr, 0, word.length );*/
                        _DotVicinity.Add( word );
                        if ( _NgramMaxLength2Right <= ++wordsToRight )
                            break;
                        start = right_ptr;
                    }
                    #endregion
                }
            }
            _DotVicinity.FixupRightWord();
            #endregion

            return (_DotVicinity.HasWords);
        }

        /*private int? TryGetThreeDotLength( SentCharType sct )
        {
            if ( (sct & SentCharType.ThreeDot) == SentCharType.ThreeDot )
            {
                var ptr = _Ptr + 1;
                var ch  = default(char);
                for ( ; ; ptr++ )
                {
                    ch = *ptr;
                    if ( ch == '\0' )
                    {
                        return (-1);
                    }
                    var ct = *(_CTM + ch); 
                    if ( (ct & CharType.IsPunctuation) != CharType.IsPunctuation &&
                         (ct & CharType.IsWhiteSpace ) != CharType.IsWhiteSpace
                       )
                    {
                        break;
                    }
                }

                var offset = (int) (ptr - _Ptr);
                if ( (*(_CTM + ch) & CharType.IsLower) == CharType.IsLower )
                {
                    return (-offset);
                }
                return (offset);
            }
            return (null);
        }*/
        [M(O.AggressiveInlining)] private int? TryGetThreeDotsLength()
        {
            /*
            многоточие
            3)	многоточие + пунктуация + маленькая буква = не конец
            */

            if ( _DotVicinity.HasRightWord )
            {
                var wordValue = _DotVicinity._RightWord.valueOriginal;
                var sct = *(_SCTM + wordValue[ 0 ]);
                if ( wordValue.Length == 1 )
                {
                    if ( (sct & SentCharType.Dot) == SentCharType.Dot )
                    {
                        if ( _DotVicinity._RightWord.hasNext )
                        {
                            var next = _DotVicinity._RightWord.next;
                            wordValue = next.valueOriginal;
                            if ( (*(_SCTM + wordValue[ 0 ]) & SentCharType.Dot) == SentCharType.Dot )
                            {
                                var offset = (int) (next.endPtr() - _Ptr);

                                if ( next.hasNext )
                                {
                                    wordValue = next.next.valueOriginal;
                                    if ( (*(_CTM + wordValue[ 0 ]) & CharType.IsLower) == CharType.IsLower )
                                    {
                                        return (-offset);
                                    }
                                }

                                return (offset);
                            }
                        }
                    }
                }
                else 
                if ( (sct & SentCharType.Dot) == SentCharType.Dot &&
                     (*(_SCTM + wordValue[ 1 ]) & SentCharType.Dot) == SentCharType.Dot
                   )
                {
                    var offset = (int) (_DotVicinity._RightWord.endPtr() - _Ptr);

                    if ( _DotVicinity._RightWord.hasNext )
                    {
                        wordValue = _DotVicinity._RightWord.next.valueOriginal;
                        if ( (*(_CTM +  wordValue[ 0 ]) & CharType.IsLower) == CharType.IsLower )
                        {
                            return (-offset);
                        }
                    }
                    
                    return (offset);
                }
            }
            return (null);
        }
        [M(O.AggressiveInlining)] private int? Try_BeforeProperOrNumber_BeforeNoProper()
        {
            var sr_pon = default(SearchResultOfHead2Left< before_proper_or_number_t >?);
            /*
            слева (от точки) стоит слово из BeforeProperOrNumber – не конец.
            исключение: 
                1)	токен из BeforeProperOrNumber одно из  «г. | п. | с. | м.», а за ними большая буквы, тогда смотреть Слово* перед BeforeProperOrNumber, 
                    и если оно состоит из комбинации больших букв IVXCLM или это цифры - конец
            */
            var pon = _Model.BeforeProperOrNumberSearcher.FindOfHead2Left( _DotVicinity._LeftWord );
            if ( 0 < pon.Count )
            {
                var sr = pon.First();
                if ( sr.v.DigitsBeforeOrSlashBefore && sr.LastWord.hasNext )
                {
                    var nextWordValue = sr.LastWord.next.valueOriginal;
                    if ( //(3 <= nextWordValue.Length) &&
                         (*(_CTM + nextWordValue[ 0 ]) & CharType.IsUpper) == CharType.IsUpper &&
                         IsLowerCharsAfterFirstChar( nextWordValue )
                       )
                    {
                        if ( _DotVicinity._LeftWord.hasPrev )
                        {
                            var prevWordValue = _DotVicinity._LeftWord.prev.valueOriginal;
                            if ( (sr.v.DigitsBefore && IsDigitsOnlyOrRomanDigitsOnly( prevWordValue )) ||
                                 (sr.v.SlashBefore  && xlat.IsSlash( prevWordValue[ prevWordValue.Length - 1 ] ))
                               )
                            {
                                var offset = (int) (sr.LastWord.endPtr() - _Ptr);
                                return (offset);
                            }
                        }
                    }
                }
                sr_pon = sr;
            }

            //слово из списка BeforeNoProper, за которым следует Большая буква – конец;
            var np = _Model.BeforeNoProperSearcher.FindOfHead2Left( _DotVicinity._LeftWord );
            if ( 0 < np.Count )
            {
                var sr_np = np.First();
                if ( sr_pon.HasValue && (sr_np.Length < sr_pon.Value.Length) )
                {
                    //sent-not-end
                    var offset = (int) (sr_pon.Value.LastWord.endPtr() - _Ptr);
                    return (-offset);
                }
                else 
                {
                    if ( sr_np.LastWord.hasNext )
                    {
                        var nextWordValue = sr_np.LastWord.next.valueOriginal;
                        if ( //(3 <= nextWordValue.Length) && 
                             (*(_CTM + nextWordValue[ 0 ]) & CharType.IsUpper) == CharType.IsUpper 
                           )
                        {
                            var offset = (int) (sr_np.LastWord.endPtr() - _Ptr);
                            return (offset);
                        }
                    }

                    //sent-not-end
                    var offset2 = (int) (sr_np.LastWord.endPtr() - _Ptr);
                    return (-offset2);
                }
            }
            else if ( sr_pon.HasValue )
            {
                //sent-not-end
                var offset = (int) (sr_pon.Value.LastWord.endPtr() - _Ptr);
                return (-offset);
            }

            return (null);
        }
        [M(O.AggressiveInlining)] private int? TryGetSingleUpperChar()
        {
            /*
            Одна большая буква с точкой – не конец.
                исключения:
                1) справа слово или большая буква др. алфавитного диапазона (кириллица / латиница)          –  конец
                2) перед буквой есть слово более 3 букв с заглавной буквы и за ней слово с заглавной буквы  –  конец
                3) буква из множества I|V|X, за которыми слово с заглавной буквы                            –  конец
                4) ºC.|F.|С.|Ф. - если перед буквами C.F.С.Ф. стоит значек º                                –  конец
             (слово – любое сочетание из 3 букв и более, может быть с заглавной буквы, незаглавной и смешанным написанием заглавных и незаглавных букв)
            */

            if ( _DotVicinity.HasLeftWord )
            {
                if ( _DotVicinity._LeftWord.valueOriginal.Length == 2 /*single-char + dot*/ )
                {
                    var wordFirstChar = _DotVicinity._LeftWord.valueOriginal[ 0 ];
                    if ( (*(_CTM + wordFirstChar) & CharType.IsUpper) == CharType.IsUpper )
                    {
                        var rightWordLength           = default(int);
                        var rightWordFirstCharIsUpper = default(bool);
                        if ( _DotVicinity.HasRightWord )
                        {
                            var rightWordValue = _DotVicinity._RightWord.valueOriginal;
                            rightWordLength           = rightWordValue.Length;
                            rightWordFirstCharIsUpper = (*(_CTM + rightWordValue[ 0 ]) & CharType.IsUpper) == CharType.IsUpper;
                        }

                        #region [.1) справа слово или большая буква др. алфавитного диапазона (кириллица / латиница) – конец.]
                        if ( rightWordFirstCharIsUpper )
                        {
                            var left_cst = GetCharsSetType( _DotVicinity._LeftWord.valueOriginal );
                            switch ( left_cst )
                            {
                                case chars_set_type.CyrillicOnly:
                                {
                                    var right_cst = GetCharsSetType( _DotVicinity._RightWord.valueOriginal );
                                    if ( right_cst == chars_set_type.LatinOnly )
                                    {
                                        return (1);
                                    }
                                }
                                break;

                                case chars_set_type.LatinOnly:
                                {
                                    var right_cst = GetCharsSetType( _DotVicinity._RightWord.valueOriginal );
                                    if ( right_cst == chars_set_type.CyrillicOnly )
                                    {
                                        return (1);
                                    }
                                }
                                break;
                            }
                        }
                        #endregion
                        
                        var prevWordValue            = default(string);
                        var prevWordLength           = default(int);
                        var prevWordFirstCharIsUpper = default(bool);
                        if ( _DotVicinity._LeftWord.hasPrev )
                        {
                            prevWordValue            = _DotVicinity._LeftWord.prev.valueOriginal;
                            prevWordLength           = prevWordValue.Length;
                            prevWordFirstCharIsUpper = (*(_CTM + prevWordValue[ 0 ]) & CharType.IsUpper) == CharType.IsUpper;
                        }

                        #region [.2) перед буквой есть слово более 3 букв с заглавной буквы, !но это не первое слово в предложении!, и за ней слово с заглавной буквы – конец.]
                        if ( (3 <  prevWordLength ) && prevWordFirstCharIsUpper  && 
                             (3 <= rightWordLength) && rightWordFirstCharIsUpper &&
                             !(IsFirstWordInSent( _DotVicinity._LeftWord.prev ))
                           )
                        {
                            return (1);
                        }
                        #endregion

                        #region [.3) буква из множества I|V|X, за которыми слово с заглавной буквы – конец.]
                        if ( rightWordFirstCharIsUpper )
                        {
                            switch ( wordFirstChar )
                            {
                                case 'I':
                                case 'V':
                                case 'X':
                                    if ( 3 <= rightWordLength )
                                    {
                                        return (1);
                                    }
                                break;
                            }
                        }
                        #endregion

                        #region [.4) º C.|F.|С.|Ф. - если перед буквами C.F.С.Ф. стоит значек º  – конец.]
                        if ( prevWordValue != null )
                        {
                            switch ( wordFirstChar )
                            {
                                case 'C':
                                case 'F':
                                case 'С':
                                case 'Ф':
                                    if ( xlat.IsDegree( prevWordValue[ 0 ] ) )
                                    {
                                        return (1);
                                    }
                                break;
                            }
                        }
                        #endregion

                        //sent-not-end
                        return (-1);
                    }
                }
            }

            return (null);
        }
        [M(O.AggressiveInlining)] private int? TryOtherSituation()
        {
            /*
            слева сочетание букв (любых), заканчивающихся на цифру, справа:
                1) большая   буква (или [_СЛОВО_] с большой   буквы) – конец.
                2) маленькая буква (или [_СЛОВО_] с маленькой буквы) – не конец.
             
            слева все большие буквы (больше одной), справа [_СЛОВО_] с:
                1) большой буквы    –  конец.
                2) маленькой буквы  –  не конец.            
             
            слева [_СЛОВО_] с большой  буквы не более 2 (уменьшим не единицу!!!) букв, справа [_СЛОВО_] с большой  буквы – не конец.
                исключения:
                1) справа и слева [_СЛОВО_] разного алфавитного диапазона (кириллица / латиница) - конец.
             
            ([_СЛОВО_] – любое сочетание 3 букв и более, может быть с заглавной буквы, незаглавной и смешанным написанием заглавных и незаглавных букв.)
            */

            if ( _DotVicinity.HasLeftWord && _DotVicinity.HasRightWord )
            {
                if ( 1 < _DotVicinity._LeftWord.valueOriginal.Length )
                {
                    //слева сочетание букв (любых), заканчивающихся на цифру
                    if ( IsLettersEndsWithDigit( _DotVicinity._LeftWord.valueOriginal ) )
                    {
                        //справа:
                        var rightWordStartUpper = ((*(_CTM + _DotVicinity._RightWord.valueOriginal[ 0 ]) & CharType.IsUpper) == CharType.IsUpper);
                        //1) большая   буква (или [_СЛОВО_] с большой   буквы) – конец.
                        //2) маленькая буква (или [_СЛОВО_] с маленькой буквы) – не конец.
                        return (rightWordStartUpper ? 1 : -1);
                    }

                    var wordValue = _DotVicinity._LeftWord.valueOriginal;
                    if ( (*(_CTM + wordValue[ 0 ]) & CharType.IsUpper) == CharType.IsUpper )
                    {
                        //справа [_СЛОВО_]
                        if ( 3 <= _DotVicinity._RightWord.valueOriginal.Length )
                        {
                            var rightWordStartUpper = ((*(_CTM + _DotVicinity._RightWord.valueOriginal[ 0 ]) & CharType.IsUpper) == CharType.IsUpper);

                            //слева все большие буквы (больше одной)
                            if ( IsUpperCharsAfterFirstChar( wordValue ) )
                            {
                                //большой буквы  –  конец.
                                if ( rightWordStartUpper )
                                {
                                    return (1);
                                }
                                //маленькой буквы  –  не конец.
                                //sent-not-end
                                return (-1);
                            }
                            //слева слово с большой буквы не более 2 букв
                            else 
                            if ( rightWordStartUpper && (LengthWithoutStartEndPunctuation( _DotVicinity._LeftWord.valueOriginal ) <= 2) )
                            {
                                var left_cst = GetCharsSetType( _DotVicinity._LeftWord.valueOriginal );
                                switch ( left_cst )
                                {
                                    case chars_set_type.CyrillicOnly:
                                    {
                                        var right_cst = GetCharsSetType( _DotVicinity._RightWord.valueOriginal );
                                        if ( right_cst == chars_set_type.LatinOnly )
                                        {
                                            return (1);
                                        }
                                    }
                                    break;

                                    case chars_set_type.LatinOnly:
                                    {
                                        var right_cst = GetCharsSetType( _DotVicinity._RightWord.valueOriginal );
                                        if ( right_cst == chars_set_type.CyrillicOnly )
                                        {
                                            return (1);
                                        }
                                    }
                                    break;
                                }

                                //с большой буквы – не конец
                                //sent-not-end
                                return (-1);
                            }
                        }
                    }
                }
            }
            return (null);
        }
        [M(O.AggressiveInlining)] private bool IsInterjection()
        {
            //междометия (interjections) с точкой (регистр не важен) – конец
            if ( _DotVicinity.HasLeftWord )
            {
                //convert left-word value to-upper
                _StringBuilder.Clear();
                fixed ( char* _base = _DotVicinity._LeftWord.valueOriginal )
                {
                    for ( var ptr = _base; ; ptr++ )
                    {
                        var ch = *ptr;
                        if ( ch == '\0' )
                            break;
                        if ( (*(_CTM + ch) & CharType.IsLetter) != CharType.IsLetter )
                            break;
                        _StringBuilder.Append( *(_UIM + ch) );
                        if ( _Model.Interjections.ValuesMaxLength <= _StringBuilder.Length )
                            break;
                    }
                }
                if ( _Model.Interjections.Values.Contains( _StringBuilder.ToString() ) )
                {
                    return (true);
                }

                #region
                /*
                if ( _DotVicinity._LeftWord.value.Length <= _Model.Interjections.ValuesMaxLength )
                {
                    var wordValue = _DotVicinity._LeftWord.value;
                    fixed ( char* _base = wordValue )
                    {
                        for ( var ptr = _base; *ptr != '\0'; ptr++ )
                        {
                            *ptr = *(_UIM + *ptr);
                        }
                    }
                    _DotVicinity._LeftWord.isUpperCase = true;

                    if ( _Model.Interjections.Values.Contains( wordValue ) )
                    {
                        return (true);
                    }
                }
                */
                #endregion
            }
            return (false);
        }
        [M(O.AggressiveInlining)] private int  TryGetFileExtensionLength()
        {
            const long ONE_LONG = 1L;

            //расширения файлов (по правилам работы с доменами первого уровня) – не конец
            if ( _DotVicinity.HasRightWord )
            {
                //convert right-word value to-upper
                if ( ((_DotVicinity._RightWord.startPtr - _Ptr) == ONE_LONG) && //расширение файла идет вплотную к точке, без пробелов
                     /*IsNoHasNextWordOrHasNotInRow( _DotVicinity._RightWord ) && */ //-!не могу вспомнить нахуй это было нужно!- //расширение файла идет отдельным словом, через (пробел)
                     (_DotVicinity._RightWord.valueOriginal.Length <= _Model.FileExtensions.ValuesMaxLength) 
                   )
                {
                    #region [.RightWord => to-upper if not exists.]
                    if ( _DotVicinity._RightWord.valueUpper == null )
                    {
                        _StringBuilder.Clear();
                        fixed ( char* _base = _DotVicinity._RightWord.valueOriginal )
                        {                            
                            for ( var ptr = _base; *ptr != '\0'; ptr++ )
                            {
                                _StringBuilder.Append( *(_UIM + *ptr) );
                            }
                        }
                        _DotVicinity._RightWord.valueUpper = _StringBuilder.ToString();
                    }
                    #endregion
                    if ( _Model.FileExtensions.Values.Contains( _DotVicinity._RightWord.valueUpper ) )
                    {
                        return (_DotVicinity._RightWord.valueUpper.Length + 1);
                    }
                }
            }
            return (0);
        }
        [M(O.AggressiveInlining)] private int  TryGetYandexCombinationsLength()
        {
            //Яндекс.сочетания (точка вплотную к яндексу, регистр не важен) – не конец
            if ( _DotVicinity.HasLeftWord && _DotVicinity.HasRightWord )
            {
                //convert left-word value to-upper
                if ( _DotVicinity._LeftWord.valueOriginal.Length != SentSplitterModel.YANDEX_EN.Length &&
                     _DotVicinity._LeftWord.valueOriginal.Length != SentSplitterModel.YANDEX_RU.Length )
                {
                    return (0);
                }
                if ( _DotVicinity._LeftWord.valueUpper == null )
                {
                    _StringBuilder.Clear();
                    fixed ( char* _base = _DotVicinity._LeftWord.valueOriginal )
                    {
                        for ( var ptr = _base; *ptr != '\0'; ptr++ )
                        {
                            _StringBuilder.Append( *(_UIM + *ptr) );
                        }
                    }
                    _DotVicinity._LeftWord.valueUpper = _StringBuilder.ToString();
                }
                switch ( _DotVicinity._LeftWord.valueUpper )
                {
                    case SentSplitterModel.YANDEX_RU:
                    case SentSplitterModel.YANDEX_EN: break;
                    default: 
                        return (0);
                }

                //right-word value to-upper while letters!
                _StringBuilder.Clear();
                fixed ( char* _base = _DotVicinity._RightWord.valueOriginal )
                {
                    for ( var ptr = _base; ; ptr++ )
                    {
                        var ch = *ptr;
                        if ( ch == '\0' )
                            break;
                        if ( (*(_CTM + ch) & CharType.IsLetter) != CharType.IsLetter )
                            break;
                        _StringBuilder.Append( *(_UIM + ch) );
                        if ( _Model.YandexCombinations.ValuesMaxLength <= _StringBuilder.Length )
                            break;
                    }
                }
                if ( _Model.YandexCombinations.Values.Contains( _StringBuilder.ToString() ) )
                {
                    var offset = (int) (_DotVicinity._RightWord.startPtr + _StringBuilder.Length - _Ptr);
                    return (offset);
                }
            }
            return (0);
        }
        [M(O.AggressiveInlining)] private int  TryEndOfQuotingLength()
        {
            //сочетание символов .“ – (справа символы пунктуации “ –) или любой из символов ,;:   –  не конец
            if ( _DotVicinity.HasRightWord )
            {
                var wordValue = _DotVicinity._RightWord.valueOriginal;

                var firstChar = wordValue[ 0 ];
                switch ( firstChar )
                {
                    case ',':
                    case ';':
                    case ':':
                        return (1);
                }

                if ( wordValue.Length == 1 )
                {
                    if ( (*(_CTM + firstChar) & CharType.IsQuote) == CharType.IsQuote )
                    {
                        if ( _DotVicinity._RightWord.hasNext )
                        {
                            if ( (*(_CTM + _DotVicinity._RightWord.next.valueOriginal[ 0 ]) & CharType.IsHyphen) == CharType.IsHyphen ) //if ( xlat.IsHyphen( _DotVicinity._RightWord.next.valueOriginal[ 0 ] ) )
                            {
                                var offset = (int) (_DotVicinity._RightWord.next.endPtr() - _Ptr);
                                return (offset);
                            }
                        }
                    }
                }
                else
                if ( (*(_CTM + firstChar     ) & CharType.IsQuote ) == CharType.IsQuote &&
                     (*(_CTM + wordValue[ 1 ]) & CharType.IsHyphen) == CharType.IsHyphen //xlat.IsHyphen( wordValue[ 1 ] )
                   )
                {
                    var offset = (int) (_DotVicinity._RightWord.endPtr() - _Ptr);
                    return (offset);
                }
            }
            return (0);
        }
        [M(O.AggressiveInlining)] private int  TryListLength()
        {
            /*
            списки:
            9.	предложение, состоящее только из цифр с точкой на конце присоединять к следующему предложению.
            */

            if ( _DotVicinity.HasLeftWord && !_DotVicinity._LeftWord.hasPrev )
            {
                if ( IsDigitsOnlyWithoutLastDot( _DotVicinity._LeftWord.valueOriginal ) )
                {
                    return (1);
                }
            }
            return (0);
        }

        [M(O.AggressiveInlining)] private bool IsDigitsOnlyOrRomanDigitsOnly( string value )
        {
            fixed ( char* _base = value )
            {
                return (IsDigitsOnly( _base ) || IsRomanDigitsOnly( _base ));
            }
        }
        [M(O.AggressiveInlining)] private bool IsDigitsOnlyWithoutLastDot( string value )
        {
            fixed ( char* _base = value )
            {
                var ptr = _base;
                for ( int i = 0, len = value.Length - 1; i < len; i++ )
                {
                    if ( (*(_CTM + *(ptr + i)) & CharType.IsDigit) != CharType.IsDigit )
                    {
                        return (false);
                    }
                }
                return (true);
            }
        }
        [M(O.AggressiveInlining)] private bool IsUpperCharsAfterFirstChar( string value )
        {
            fixed ( char* _base = value )
            {
                return (IsUpperChars( _base + 1 ));
            }
        }
        [M(O.AggressiveInlining)] private bool IsLowerCharsAfterFirstChar( string value )
        {
            fixed ( char* _base = value )
            {
                return (IsLowerChars( _base + 1 ));
            }
        }
        [M(O.AggressiveInlining)] unsafe private bool IsDigitsOnly( char* ptr )
        {
            for ( ; *ptr != '\0'; ptr++ )
            {
                if ( (*(_CTM + *ptr) & CharType.IsDigit) != CharType.IsDigit )
                {
                    return (false);
                }
            }
            return (true);
        }
        [M(O.AggressiveInlining)] unsafe private bool IsRomanDigitsOnly( char* ptr )
        {
            for ( ; *ptr != '\0'; ptr++ )
            {
                if ( (*(_SCTM + *ptr) & SentCharType.RomanDigit) != SentCharType.RomanDigit )
                {
                    return (false);
                }
            }
            return (true);
        }
        /*unsafe private bool IsContainsSlash( char* ptr, int length )
        {
            for ( length--; 0 <= length; length-- )
            {
                switch ( *(ptr + length) )
                {
                    case '/':
                    case '\\':
                        return (true);
                }
            }
            return (false);
        }*/
        [M(O.AggressiveInlining)] unsafe private bool IsUpperChars( char* ptr )
        {
            for ( ; ; ptr++ )
            {
                var ch = *ptr;
                if ( ch == '\0' )
                {
                    break;
                }
                var ct = *(_CTM + ch);
                if ( (ct & CharType.IsPunctuation) == CharType.IsPunctuation /*||
                     (ct & CharType.IsDigit)       == CharType.IsDigit*/
                   )
                {
                    continue;
                }
                if ( (ct & CharType.IsUpper) != CharType.IsUpper )
                {
                    return (false);
                }
            }
            return (true);
        }
        [M(O.AggressiveInlining)] unsafe private bool IsLowerChars( char* ptr )
        {
            for ( ; ; ptr++ )
            {
                var ch = *ptr;
                if ( ch == '\0' )
                {
                    break;
                }
                var ct = *(_CTM + ch);
                if ( (ct & CharType.IsPunctuation) == CharType.IsPunctuation /*||
                     (ct & CharType.IsDigit)       == CharType.IsDigit*/ 
                   )
                {
                    continue;
                }
                if ( (ct & CharType.IsLower) != CharType.IsLower )
                {
                    return (false);
                }
            }
            return (true);
        }
        [M(O.AggressiveInlining)] private int LengthWithoutStartEndPunctuation( string value )
        {
            fixed ( char* _base = value )
            {                
                var start = _base;
                for ( ; ; start++ )
                {
                    var ch = *start;
                    if ( ch == '\0' )
                    {
                        break;
                    }
                    if ( (*(_CTM + ch) & CharType.IsPunctuation) != CharType.IsPunctuation )
                    {
                        break;
                    }
                }

                var end = _base + value.Length - 1;
                for ( ; ; end-- )
                {
                    if ( end <= start )
                    {
                        break;
                    }
                    if ( (*(_CTM + *end) & CharType.IsPunctuation) != CharType.IsPunctuation )
                    {
                        break;
                    }
                }

                return (int) (end + 1 - start);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags] private enum chars_set_type
        {
            __UNDEFINE__ = 0,

            CyrillicOnly = 0x1,
            LatinOnly    = 1 << 1,
        }
        [M(O.AggressiveInlining)] private chars_set_type GetCharsSetType( string value )
        {
            var cst = default(chars_set_type);
            fixed ( char* _base = value )
            {
                for ( var ptr = _base; ; ptr++ )
                {
                    var ch = *ptr;
                    if ( ch == '\0' )
                        break;
                    if ( (*(_CTM + ch) & CharType.IsLetter) != CharType.IsLetter )
                        continue;
                    if ( 0 <= ch && ch <= 127 )
                    {
                        cst |= chars_set_type.LatinOnly;
                    }
                    else
                    if ( 'А' <= ch && ch <= 'я' )
                    {
                        cst |= chars_set_type.CyrillicOnly;
                    }
                }
            }
            return (cst);
        }
        [M(O.AggressiveInlining)] private bool IsLettersEndsWithDigit( string value )
        {
            fixed ( char* _base = value )
            {
                var ptr = _base + value.Length - 1;
                for ( ; _base <= ptr; ptr-- )
                {
                    if ( (*(_CTM + *ptr) & CharType.IsPunctuation) != CharType.IsPunctuation )
                        break;
                }
                if ( (*(_CTM + *ptr  ) & CharType.IsDigit ) == CharType.IsDigit &&
                     (*(_CTM + *_base) & CharType.IsLetter) == CharType.IsLetter
                   )
                {
                    return (true);
                }
                return (false);
            }
        }
        [M(O.AggressiveInlining)] private bool IsCurrentSentContainsPunctuationOrWhitespace()
        {
            switch ( _Sent.length )
            {
                case 1:
                #region
                {
                    var ct = *(_CTM + *(_BASE + _Sent.startIndex));
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation
                       )
                    {
                        return (false);
                    }
                    return (true);
                }
                #endregion

                case 2:
                #region
                {
                    var p = _BASE + _Sent.startIndex;
                    var ct = *(_CTM + *p);
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation
                       )
                    {
                        return (false);
                    }
                    ct = *(_CTM + *(++p));
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation
                       )
                    {
                        return (false);
                    }
                    return (true);
                }
                #endregion

                case 3:
                #region
                {
                    var p = _BASE + _Sent.startIndex;
                    var ct = *(_CTM + *p);
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation
                       )
                    {
                        return (false);
                    }
                    ct = *(_CTM + *(++p));
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation
                       )
                    {
                        return (false);
                    }
                    ct = *(_CTM + *(++p));
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation
                       )
                    {
                        return (false);
                    }
                    return (true);
                }
                #endregion

                case 4:
                #region
                {
                    var p = _BASE + _Sent.startIndex;
                    var ct = *(_CTM + *p);
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation
                       )
                    {
                        return (false);
                    }
                    ct = *(_CTM + *(++p));
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation
                       )
                    {
                        return (false);
                    }
                    ct = *(_CTM + *(++p));
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation
                       )
                    {
                        return (false);
                    }
                    ct = *(_CTM + *(++p));
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation
                       )
                    {
                        return (false);
                    }
                    return (true);
                }
                #endregion

                default:
                #region
                for ( int i = _Sent.startIndex, end = i + _Sent.length; i < end; i++ )
                {
                    var ct = *(_CTM + *(_BASE + i));
                    if ( (ct & CharType.IsWhiteSpace)  != CharType.IsWhiteSpace &&
                         (ct & CharType.IsPunctuation) != CharType.IsPunctuation
                       )
                    {
                        return (false);
                    }
                }
                return (true);
                #endregion
            }
        }
        [M(O.AggressiveInlining)] private bool IsFirstWordInSent( ss_word_t word )
        {
            if ( word.hasPrev )
            {
                for ( char* ptr = word.startPtr - 1, 
                        leftBorder = GetMaxPtr( _BASE + _Sent.startIndex, _EndUrlPtr ); 
                        leftBorder <= ptr; ptr-- 
                    )
                {
                    var ct = *(_CTM + *ptr);
                    if ( (ct & CharType.IsLetter) == CharType.IsLetter ||
                         (ct & CharType.IsDigit ) == CharType.IsDigit 
                       )
                    {
                        return (false);
                    }
                }
            }
            return (true);
        }
        [M(O.AggressiveInlining)] unsafe private static char* GetMaxPtr( char* p1, char* p2 ) => (p1 > p2 ? p1 : p2);
        [M(O.AggressiveInlining)] private static bool IsNoHasNextWordOrHasNotInRow( ss_word_t word )
        {
                        //---?!---(word.next.startPtr - word.endPtr() [-?!-]=[-?!-]> 1)---?!---//
            if ( !word.hasNext || (word.next.startPtr - word.next.endPtr() > 1) )
            {
                return (true);
            }

            return (false);
        }
    }
}
