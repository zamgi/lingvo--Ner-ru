using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace lingvo.core
{
#if XLAT_CHARTYPE_MAP || XLAT_CHARTYPE_MAP_DECLARE
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum CharType : ushort
    {
        __UNDEFINE__  = 0x0,

        IsUpper       = 0x1,
        IsLower       = 1 << 1,
        IsLetter      = 1 << 2,
        IsDigit       = 1 << 3,

        IsWhiteSpace  = 1 << 4,
        IsPunctuation = 1 << 5,

        IsUrlBreak       = 1 << 6,
        IsURIschemesChar = 1 << 7,
        
        IsQuote            = 1 << 8,
        IsQuoteLeft        = IsQuote   | (1 << 9),
        IsQuoteRight       = IsQuote   | (1 << 10),
        IsQuoteDoubleSided = IsQuote   | (1 << 11),
        IsBracket          = 1 << 12,
        IsBracketLeft      = IsBracket | (1 << 13),
        IsBracketRight     = IsBracket | (1 << 14),
        IsQuoteOrBracket   = IsQuote   | IsBracket,

        IsHyphen           = (1 << 15),
    }
#endif

    /// <summary>
    /// xlat-fusking-super-class
    /// </summary>
#if XLAT_INTERNAL
    internal
#else
    public
#endif
    static class xlat
    {
#if XLAT_CHARTYPE_MAP
        public static readonly CharType[] CHARTYPE_MAP = new CharType[ char.MaxValue + 1 ];
        private static readonly char[] QUOTES_LEFT  = new[] { '«', //0x00AB, 0171
                                                              '‹', //0x2039, 8249
                                                              '„', //0x201E, 8222
                                                              '“', //0x201C, 8220                                                              
                                                            };
        private static readonly char[] QUOTES_RIGHT = new[] { '»', //0x00BB, 0187
                                                              '›', //0x203A, 8250
                                                              '”', //0x201D, 8221
                                                              '‟', //0x201F, 8223
                                                            };
        private static readonly char   QUOTE_LEFT_RIGHT    = '"'; //0x0022, 0034
        private static readonly char[] QUOTES_DOUBLE_SIDED = new[] { '‛', //0x201B, 8219 - не встречается
                                                                     '‚', //0x201A, 8218 - не встречается
                                                                     '‘', //0x2018, 8216  - не встречается
                                                                     '’', //0x2019, 8217 - не встречается в качестве кавычки                                                                      
                                                                     '\'',//
                                                                     QUOTE_LEFT_RIGHT,
                                                                   };
        private static readonly char[] BRACKETS_LEFT  = new[] { '(', '‹', '{', '[', };
        private static readonly char[] BRACKETS_RIGHT = new[] { ')', '›', '}', ']', };
        private static readonly char[] HYPHENS        = new[] { '-', '—', '–', };
#endif
#if XLAT_UPPER_INVARIANT_MAP
        public static readonly char[] UPPER_INVARIANT_MAP = new char[ char.MaxValue + 1 ];
#endif
#if XLAT_WHITESPACE_CHARS
        public static readonly char[] WHITESPACE_CHARS;
#endif

        static xlat()
        {            
#if XLAT_CHARTYPE_MAP
            for ( var c = char.MinValue; /*c <= char.MaxValue*/; c++ )
            {
                if ( char.IsUpper( c ) )
                    CHARTYPE_MAP[ c ] |= CharType.IsUpper;

                if ( char.IsLower( c ) )
                    CHARTYPE_MAP[ c ] |= CharType.IsLower;

                if ( char.IsLetter( c ) )
                    CHARTYPE_MAP[ c ] |= CharType.IsLetter;

                if ( char.IsDigit( c ) )
                    CHARTYPE_MAP[ c ] |= CharType.IsDigit;

                var isWhiteSpace = char.IsWhiteSpace( c );
                if ( isWhiteSpace )
                    CHARTYPE_MAP[ c ] |= CharType.IsWhiteSpace;      

                var isPunctuation = char.IsPunctuation( c );
                if ( isPunctuation )
                    CHARTYPE_MAP[ c ] |= CharType.IsPunctuation;

                #region [.is-url-break.]
                if ( (isWhiteSpace  /*&& (c != '\u0160')*/) || 
                     (isPunctuation && !IsAscii( c ))  ||
                     (c == '\0')
                   )
                {
                    CHARTYPE_MAP[ c ] |= CharType.IsUrlBreak;
                }
                #endregion

                if ( IsURIschemes( c ) )
                    CHARTYPE_MAP[ c ] |= CharType.IsURIschemesChar;

                if ( c == char.MaxValue )
                {
                    break;
                }
            }

            foreach ( var c in HYPHENS )
            {
                CHARTYPE_MAP[ c ] |= CharType.IsHyphen;
            }

            foreach ( var c in QUOTES_LEFT )
            {
                CHARTYPE_MAP[ c ] |= CharType.IsQuoteLeft;
            }
            foreach ( var c in QUOTES_RIGHT )
            {
                CHARTYPE_MAP[ c ] |= CharType.IsQuoteRight;
            }
            foreach ( var c in QUOTES_DOUBLE_SIDED )
            {
                CHARTYPE_MAP[ c ] |= CharType.IsQuoteDoubleSided;
            }
            CHARTYPE_MAP[ QUOTE_LEFT_RIGHT ] |= CharType.IsQuoteRight | CharType.IsQuoteLeft;

            foreach ( var c in BRACKETS_LEFT )
            {
                CHARTYPE_MAP[ c ] |= CharType.IsBracketLeft;
            }
            foreach ( var c in BRACKETS_RIGHT )
            {
                CHARTYPE_MAP[ c ] |= CharType.IsBracketRight;
            }
#endif

#if XLAT_UPPER_INVARIANT_MAP
            char c2;
            for ( var c = char.MinValue; /*c <= char.MaxValue*/; c++ )
            {
                switch ( c )
                {
                    case 'ё':     // '\u0451'
                        c2 = 'е'; // '\u0435';
                    break;

                    case 'Ё':     // '\u0401'
                        c2 = 'Е'; // '\u0415';
                    break;

                    default:
                        c2 = c;
                    break;
                }

                UPPER_INVARIANT_MAP[ c ] = char.ToUpperInvariant( c2 );

                if ( c == char.MaxValue )
                {
                    break;
                }
            }
#endif

#if XLAT_WHITESPACE_CHARS
            var wsc = new List< char >();
            for ( var c = char.MinValue; /*c <= char.MaxValue*/; c++ )
            {
                if ( char.IsWhiteSpace( c ) )
                    wsc.Add( c );

                if ( c == char.MaxValue )
                {
                    break;
                }
            }
            WHITESPACE_CHARS = wsc.ToArray();
#endif
        }
		
		/*
#if XLAT_CHARTYPE_MAP       
        public static bool IsUpper( char ch )
        {
            return ((CHARTYPE_MAP[ ch ] & CharType.IsUpper) == CharType.IsUpper);
        }
        public static bool IsLower( char ch )
        {
            return ((CHARTYPE_MAP[ ch ] & CharType.IsLower) == CharType.IsLower);
        }
        public static bool IsLetter( char ch )
        {
            return ((CHARTYPE_MAP[ ch ] & CharType.IsLetter) == CharType.IsLetter);
        }
        public static bool IsDigit( char ch )
        {
            return ((CHARTYPE_MAP[ ch ] & CharType.IsDigit) == CharType.IsDigit);
        }
        public static bool IsWhiteSpace( char ch )
        {
            return ((CHARTYPE_MAP[ ch ] & CharType.IsWhiteSpace) == CharType.IsWhiteSpace);
        }
        public static bool IsPunctuation( char ch )
        {
            return ((CHARTYPE_MAP[ ch ] & CharType.IsPunctuation) == CharType.IsPunctuation);
        }

        public static bool IsHyphen( char ch )
        {
            return ((CHARTYPE_MAP[ ch ] & CharType.IsHyphen) == CharType.IsHyphen);
            /*
            switch ( ch )
            {
                case '-':
                case '—':
                case '–':
                    return (true);

                default:
                    return (false);
            }
            * /
        }
#endif
		*/

        public static bool IsDot( char ch )
        {
            //return (ch == '.');
            //*
            switch ( ch )
            {
                case '.':
                case char.MaxValue:
                    return (true);

                default:
                    return (false);
            }
            //*/
        }
        public static bool IsAscii( char ch )
        {
            return (0 <= ch && ch <= 127);
        }
        public static bool IsURIschemes( char ch )
        {
            if ( ('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') )
            {
                return (true);
            }
            switch ( ch )
            {
                case '-':
                    return (true);
                default:
                    return (false);
            }
        }
        public static bool IsURIschemesPathSeparator( char ch )
        {
            switch ( ch )    
            {
                case '/':
                case '\\':
                    return (true);
                default: 
                    return (false);
            }
        }
        public static bool IsDegree( char ch )
        {
            switch ( ch )
            {
                case '°':
                case 'º':
                    return (true);
                default:
                    return (false);
            }
        }
        public static bool IsSlash( char ch )
        {
            switch ( ch )
            {
                case '/':
                case '\\':
                    return (true);

                default:
                    return (false);
            }
        }

        public static char[] Create_LOWER_INVARIANT_MAP()
        {
            var lower_invariant_map = new char[ char.MaxValue + 1 ];
            for ( char ch = char.MinValue, ch2; /*ch <= char.MaxValue*/; ch++ )
            {
                switch ( ch )
                {
                    case 'ё':     // '\u0451'
                        ch2 = 'е'; // '\u0435';
                    break;

                    case 'Ё':     // '\u0401'
                        ch2 = 'Е'; // '\u0415';
                    break;

                    default:
                        ch2 = ch;
                    break;
                }

                lower_invariant_map[ ch ] = char.ToLowerInvariant( ch2 );

                if ( ch == char.MaxValue )
                {
                    break;
                }
            }
            return (lower_invariant_map);
        }
    }


    /// <summary>
    /// 
    /// </summary>
#if XLAT_INTERNAL
    unsafe internal
#else
    unsafe public
#endif
    sealed class xlat_Unsafe
    {
        /// <summary>
        /// Обозначение начала предложения (в формате CRFSuit)
        /// </summary> 
        public const string BEGIN_OF_SENTENCE = "__BOS__";
        /// <summary>
        /// Обозначение конца предложения (в формате CRFSuit)
        /// </summary> 
        public const string END_OF_SENTENCE = "__EOS__";
        public const string INPUTTYPE_OTHER = "O";

        public readonly byte* _InputtypeOtherPtrBase;
        public readonly byte* _PosInputtypeOtherPtrBase;
        public readonly byte* _NerInputtypeOtherPtrBase;
        public readonly byte* _BeginOfSentencePtrBase;
        public readonly byte* _EndOfSentencePtrBase;

#if XLAT_CHARTYPE_MAP
        public readonly CharType* _CHARTYPE_MAP;
#endif			
#if XLAT_UPPER_INVARIANT_MAP			
        public readonly char*     _UPPER_INVARIANT_MAP;
#endif	
		
        private xlat_Unsafe()
        {
            //string POSINPUTTYPE_OTHER = PosTaggerInputType.O.ToText();
            //string NERINPUTTYPE_OTHER = NerInputType.O.ToText();
            
            //-1-            
            var inputtypeOtherBytes         = Encoding.UTF8.GetBytes( INPUTTYPE_OTHER );
            var inputtypeOtherBytesGCHandle = GCHandle.Alloc( inputtypeOtherBytes, GCHandleType.Pinned );
            _InputtypeOtherPtrBase          = (byte*) inputtypeOtherBytesGCHandle.AddrOfPinnedObject().ToPointer();

            //-1-            
            var posInputtypeOtherBytes         = Encoding.UTF8.GetBytes( INPUTTYPE_OTHER /*POSINPUTTYPE_OTHER*/ );
            var posInputtypeOtherBytesGCHandle = GCHandle.Alloc( posInputtypeOtherBytes, GCHandleType.Pinned );
            _PosInputtypeOtherPtrBase          = (byte*) posInputtypeOtherBytesGCHandle.AddrOfPinnedObject().ToPointer();

            //-1-            
            var nerInputtypeOtherBytes         = Encoding.UTF8.GetBytes( INPUTTYPE_OTHER /*NERINPUTTYPE_OTHER*/ );
            var nerInputtypeOtherBytesGCHandle = GCHandle.Alloc( nerInputtypeOtherBytes, GCHandleType.Pinned );
            _NerInputtypeOtherPtrBase          = (byte*) nerInputtypeOtherBytesGCHandle.AddrOfPinnedObject().ToPointer();

            //-2-
            var beginOfSentenceBytes         = Encoding.UTF8.GetBytes( BEGIN_OF_SENTENCE );
            var beginOfSentenceBytesGCHandle = GCHandle.Alloc( beginOfSentenceBytes, GCHandleType.Pinned );
            _BeginOfSentencePtrBase          = (byte*) beginOfSentenceBytesGCHandle.AddrOfPinnedObject().ToPointer();

            //-3-
            var endOfSentenceBytes         = Encoding.UTF8.GetBytes( END_OF_SENTENCE );
            var endOfSentenceBytesGCHandle = GCHandle.Alloc( endOfSentenceBytes, GCHandleType.Pinned );
            _EndOfSentencePtrBase          = (byte*) endOfSentenceBytesGCHandle.AddrOfPinnedObject().ToPointer();

#if XLAT_CHARTYPE_MAP
            //-4-
            var _x_ = new ushort[ xlat.CHARTYPE_MAP.Length ];
            for ( int i = 0; i < _x_.Length; i++ )
            {
                _x_[ i ] = (ushort) xlat.CHARTYPE_MAP[ i ];
            }
            var ctmGCHandle = GCHandle.Alloc( _x_ /*xlat.CHARTYPE_MAP*/, GCHandleType.Pinned );
            _CHARTYPE_MAP   = (CharType*) ctmGCHandle.AddrOfPinnedObject().ToPointer();
            //_CHARTYPE_MAP = (CharType*) Marshal.UnsafeAddrOfPinnedArrayElement( xlat.CHARTYPE_MAP, 0 ).ToPointer();
#endif			
#if XLAT_UPPER_INVARIANT_MAP			
            //-5-
            var uimGCHandle      = GCHandle.Alloc( xlat.UPPER_INVARIANT_MAP, GCHandleType.Pinned );
            _UPPER_INVARIANT_MAP = (char*) uimGCHandle.AddrOfPinnedObject().ToPointer();
            //---_UPPER_INVARIANT_MAP = (char*) Marshal.UnsafeAddrOfPinnedArrayElement( UPPER_INVARIANT_MAP, 0 ).ToPointer();
#endif	
        }

        public static readonly xlat_Unsafe Inst = new xlat_Unsafe();

#if XLAT_CHARTYPE_MAP
        public bool IsUpper( char ch )
        {
            return ((_CHARTYPE_MAP[ ch ] & CharType.IsUpper) == CharType.IsUpper);
        }
        public bool IsLower( char ch )
        {
            return ((_CHARTYPE_MAP[ ch ] & CharType.IsLower) == CharType.IsLower);
        }
        public bool IsLetter( char ch )
        {
            return ((_CHARTYPE_MAP[ ch ] & CharType.IsLetter) == CharType.IsLetter);
        }
        public bool IsDigit( char ch )
        {
            return ((_CHARTYPE_MAP[ ch ] & CharType.IsDigit) == CharType.IsDigit);
        }
        public bool IsWhiteSpace( char ch )
        {
            return ((_CHARTYPE_MAP[ ch ] & CharType.IsWhiteSpace) == CharType.IsWhiteSpace);
        }
        public bool IsPunctuation( char ch )
        {
            return ((_CHARTYPE_MAP[ ch ] & CharType.IsPunctuation) == CharType.IsPunctuation);
        }
        public bool IsHyphen( char ch )
        {
            return ((_CHARTYPE_MAP[ ch ] & CharType.IsHyphen) == CharType.IsHyphen);
        }
#endif		
    }
}
