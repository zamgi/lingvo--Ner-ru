using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

using lingvo.core;

namespace lingvo.urls
{
    /// <summary>
    /// 
    /// </summary>
    public class url_t
    {
        public string  value;
        public int     startIndex;
        public int     length;

        public override string ToString()
        {
            if ( value != null )
                return ('\'' + value + "' [" + startIndex + ":" + length + "]");
            return ("[" + startIndex + ":" + length + "]");
        }

        internal url_t create_copy()
        {
            var url = new url_t()
            {
                startIndex = this.startIndex,
                length     = this.length,
                value      = this.value,
            };
            return (url);
        }
        unsafe internal url_struct_t to_url_struct( char* _base )
        {
            var url = new url_struct_t()
            {
                startPtr = _base + startIndex,
                length   = length,
            };
            return (url);
        }

        unsafe static internal url_t to_url( url_struct_t url, char* _base )
        {
            var _url = new url_t()
            {
                startIndex = (int) (url.startPtr - _base),
                length     = url.length,
            };
            return (_url);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    unsafe public struct url_struct_t
    {
        public char* startPtr;
        public int   length;
    }

    /// <summary>
    /// 
    /// </summary>
    public class UrlDetectorModel
    {
        public UrlDetectorModel( string urlDetectorResourcesXmlFilename )
        {
            var xdoc = XDocument.Load( urlDetectorResourcesXmlFilename );

            var firstLevelDomains = from xe in xdoc.Root.Element( "first-level-domains" ).Elements()
                                    select xe.Value;
            var uriSchemes        = from xe in xdoc.Root.Element( "uri-schemes" ).Elements()
                                    select xe.Value;

            Initialize( firstLevelDomains, uriSchemes );

            xdoc = null;
        }
        public UrlDetectorModel( IEnumerable< string > firstLevelDomains, IEnumerable< string > uriSchemes )
        {
            Initialize( firstLevelDomains, uriSchemes );
        }

        private void Initialize( IEnumerable< string > firstLevelDomains, IEnumerable< string > uriSchemes )
        {
            FirstLevelDomains          = firstLevelDomains.ToHashset_4Urls();
            FirstLevelDomainsMaxLength = FirstLevelDomains.GetItemMaxLength_4Urls();

            URIschemes                = uriSchemes.ToHashsetWithReverseValues_4Urls();
            URIschemesMaxLength       = URIschemes.GetItemMaxLength_4Urls();
        }

        public HashSet< string > FirstLevelDomains
        { 
            get; 
            private set; 
        }
        public int               FirstLevelDomainsMaxLength
        {
            get;
            private set;
        }
        public HashSet< string > URIschemes
        { 
            get; 
            private set; 
        }
        public int               URIschemesMaxLength
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UrlDetectorConfig
    {
        public UrlDetectorConfig()
        {
        }
        public UrlDetectorConfig( string urlDetectorResourcesXmlFilename )
        {
            Model = new UrlDetectorModel( urlDetectorResourcesXmlFilename );
        }

        public UrlDetectorModel Model
        {
            get;
            set;
        }
        public UrlDetector.UrlExtractModeEnum UrlExtractMode
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    unsafe public sealed class UrlDetector : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public enum UrlExtractModeEnum
        {
            ValueAndPosition,
            Position,           
        }

        #region [.private field's.]
        private const int DEFAULT_LIST_CAPACITY                              = 100;
        private const int ALLOCATEURL_BYFIRSTLEVELDOMAIN_MAXRECURSIONNESTING = 10;
        private readonly HashSet< string >    _FirstLevelDomains;
        private readonly int                  _FirstLevelDomainsMaxLength;
        private readonly HashSet< string >    _URIschemes;
        private readonly int                  _URIschemesMaxLength;
        private readonly bool                 _ExtractValue;
        private readonly List< url_t >        _Urls;
        private readonly List< url_struct_t > _Urlstructs;
        private readonly StringBuilder        _StringBuilder;
        private readonly char[]               _FirstLevelDomainBuffer; //buffer for first-level-domain (right) part of url
        private readonly GCHandle             _FirstLevelDomainBufferGCHandle;
        private char*                         _FldBufferPtrBase;
        private readonly char[]               _URIschemesBuffer;       //buffer for URI-schemes (left) part of url
        private readonly GCHandle             _URIschemesBufferGCHandle;
        private char*                         _UriSchBufferPtrBase;
        private readonly url_t                _Url;
        private readonly CharType*            _CTM;  //xlat.CHARTYPE_MAP
        private readonly char*                _UIM;  //xlat.UPPER_INVARIANT_MAP        
        private char*                         _BASE; //start pointer into text
        private char*                         _Ptr;  //current pointer into text
        #endregion

        #region [.ctor().]
        public UrlDetector( UrlDetectorConfig config )
        {
            _ExtractValue = (config.UrlExtractMode == UrlExtractModeEnum.ValueAndPosition);

            _FirstLevelDomains          = config.Model.FirstLevelDomains;
            _FirstLevelDomainsMaxLength = config.Model.FirstLevelDomainsMaxLength;

            _URIschemes                 = config.Model.URIschemes;
            _URIschemesMaxLength        = config.Model.URIschemesMaxLength;

            _Urls                       = new List< url_t >( DEFAULT_LIST_CAPACITY );            
            _StringBuilder              = new StringBuilder();
            _Url                        = new url_t();
            _Urlstructs                 = new List< url_struct_t >( DEFAULT_LIST_CAPACITY );
			
            _CTM = xlat_Unsafe.Inst._CHARTYPE_MAP;
            _UIM = xlat_Unsafe.Inst._UPPER_INVARIANT_MAP;

            //-1-
            _FirstLevelDomainBuffer         = new char[ _FirstLevelDomainsMaxLength + 1 ];
            _FirstLevelDomainBufferGCHandle = GCHandle.Alloc( _FirstLevelDomainBuffer, GCHandleType.Pinned );
            _FldBufferPtrBase               = (char*) _FirstLevelDomainBufferGCHandle.AddrOfPinnedObject().ToPointer();

            //-2-
            _URIschemesBuffer         = new char[ _URIschemesMaxLength + 1 ];
            _URIschemesBufferGCHandle = GCHandle.Alloc( _URIschemesBuffer, GCHandleType.Pinned );
            _UriSchBufferPtrBase      = (char*) _URIschemesBufferGCHandle.AddrOfPinnedObject().ToPointer();
        }

        ~UrlDetector()
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
            if ( _FldBufferPtrBase != null )
            {
                _FirstLevelDomainBufferGCHandle.Free();
                _FldBufferPtrBase = null;
            }

            if ( _UriSchBufferPtrBase != null )
            {
                _URIschemesBufferGCHandle.Free();
                _UriSchBufferPtrBase = null;
            }
        }
        #endregion

        unsafe public List< url_t > AllocateUrls( string text )
        {
            _Urls.Clear();

            fixed ( char* _base = text )
            {
                _BASE = _base;

                for ( _Ptr = _BASE; *_Ptr != '\0'; _Ptr++ )
                {
                    switch ( *_Ptr )
                    {
                        //-dot-
                        case '.':
                        #region
                        {
                            if ( TryAllocateUrl_ByWWW() )
                            {
                                _Urls.Add( _Url.create_copy() );
                            }
                            else if ( TryAllocateUrl_ByFirstLevelDomain( ALLOCATEURL_BYFIRSTLEVELDOMAIN_MAXRECURSIONNESTING ) )
                            {
                                _Urls.Add( _Url.create_copy() );
                            }
                        }
                        #endregion
                        break;

                        //-colon-
                        case ':':
                        #region
                        {
#if DEBUG
var xxx = new string( _Ptr - 25, 0, 100 );
#endif
                            if ( TryAllocateUrl_ByURIschemes() )
                            {
                                _Urls.Add( _Url.create_copy() );
                            }
                        }
                        #endregion
                        break;
                    }
                }
            }

            return (_Urls);
        }

        unsafe public List< url_struct_t > AllocateUrls( char* _base )
        {
            _Urlstructs.Clear();

            _BASE = _base;

            for ( _Ptr = _BASE; *_Ptr != '\0'; _Ptr++ )
            {
                switch ( *_Ptr )
                {
                    //-dot-
                    case '.':
                    #region
                    {
                        if ( TryAllocateUrl_ByWWW() )
                        {
                            _Urlstructs.Add( _Url.to_url_struct( _base ) );
                        }
                        else if ( TryAllocateUrl_ByFirstLevelDomain( ALLOCATEURL_BYFIRSTLEVELDOMAIN_MAXRECURSIONNESTING ) )
                        {
                            _Urlstructs.Add( _Url.to_url_struct( _base ) );
                        }
                    }
                    #endregion
                    break;

                    //-colon-
                    case ':':
                    #region
                    {
#if DEBUG
var xxx = new string( _Ptr - 25, 0, 100 );
#endif
                        if ( TryAllocateUrl_ByURIschemes() )
                        {
                            _Urlstructs.Add( _Url.to_url_struct( _base ) );
                        }
                    }
                    #endregion
                    break;
                }
            }

            return (_Urlstructs);
        }

        unsafe public void InitializeAllocate( char* _base )
        {
            //_CTM  = ctm;
            //_UIM  = uim;
            _BASE = _base;
        }
        unsafe public url_t AllocateSingleUrl( char* ptr )
        {
            switch ( *ptr )
            {
                //-dot-
                case '.':
                #region
                {
                    _Ptr = ptr;

                    if ( TryAllocateUrl_ByWWW() )
                    {
                        return (_Url);
                    }

                    if ( TryAllocateUrl_ByFirstLevelDomain( ALLOCATEURL_BYFIRSTLEVELDOMAIN_MAXRECURSIONNESTING ) )
                    {
                        return (_Url);
                    }

                    return (null);
                }
                #endregion

                //-colon-
                case ':':
                #region
                {
                    _Ptr = ptr;
#if DEBUG
var xxx = new string( _Ptr - 25, 0, 100 );
#endif
                    if ( TryAllocateUrl_ByURIschemes() )
                    {
                        return (_Url);
                    }
                    return (null);
                }
                #endregion
            }

            return (null);
        }
        unsafe public url_t AllocateSingleUrlByColon( char* ptr )
        {
            #region [.must by (*ptr == ':').]
            {
                _Ptr = ptr;
#if DEBUG
var xxx = new string( _Ptr - 25, 0, 100 );
#endif
                if ( TryAllocateUrl_ByURIschemes() )
                {
                    return (_Url.create_copy());
                }
                return (null);
            }
            #endregion
        }
        unsafe public url_t AllocateSingleUrlByDot( char* ptr )
        {
            #region [.must by (*ptr == '.').]
            {
                _Ptr = ptr;

                if ( TryAllocateUrl_ByWWW() )
                {
                    return (_Url.create_copy());
                }

                if ( TryAllocateUrl_ByFirstLevelDomain( ALLOCATEURL_BYFIRSTLEVELDOMAIN_MAXRECURSIONNESTING ) )
                {
                    return (_Url.create_copy());
                }

                return (null);
            }
            #endregion
        }

        unsafe public url_struct_t? AllocateSingleUrlstruct( char* ptr )
        {
            switch ( *ptr )
            {
                //-dot-
                case '.':
                #region
                {
                    _Ptr = ptr;

                    if ( TryAllocateUrl_ByWWW() )
                    {
                        return (_Url.to_url_struct( _BASE ));
                    }

                    if ( TryAllocateUrl_ByFirstLevelDomain( ALLOCATEURL_BYFIRSTLEVELDOMAIN_MAXRECURSIONNESTING ) )
                    {
                        return (_Url.to_url_struct( _BASE ));
                    }

                    return (null);
                }
                #endregion

                //-colon-
                case ':':
                #region
                {
                    _Ptr = ptr;
#if DEBUG
var xxx = new string( _Ptr - 25, 0, 100 );
#endif
                    if ( TryAllocateUrl_ByURIschemes() )
                    {
                        return (_Url.to_url_struct( _BASE ));
                    }
                    return (null);
                }
                #endregion
            }

            return (null);
        }
        unsafe public url_struct_t? AllocateSingleUrlstructByColon( char* ptr )
        {
            #region [.must by (*ptr == ':').]
            {
                _Ptr = ptr;
#if DEBUG
var xxx = new string( _Ptr - 25, 0, 100 );
#endif
                if ( TryAllocateUrl_ByURIschemes() )
                {
                    return (_Url.to_url_struct( _BASE ));
                }
                return (null);
            }
            #endregion
        }
        unsafe public url_struct_t? AllocateSingleUrlstructByDot( char* ptr )
        {
            #region [.must by (*ptr == '.').]
            {
                _Ptr = ptr;

                if ( TryAllocateUrl_ByWWW() )
                {
                    return (_Url.to_url_struct( _BASE ));
                }

                if ( TryAllocateUrl_ByFirstLevelDomain( ALLOCATEURL_BYFIRSTLEVELDOMAIN_MAXRECURSIONNESTING ) )
                {
                    return (_Url.to_url_struct( _BASE ));
                }

                return (null);
            }
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        private bool TryAllocateUrl_ByWWW()
        {
            const int WWW_LENGTH = 3;

            #region [.check WWW on the left.]
            if ( _Ptr - WWW_LENGTH < _BASE )
            {
                return (false);
            }
            var isWWW = (*(_UIM + *(_Ptr - 1)) == 'W') &&
                        (*(_UIM + *(_Ptr - 2)) == 'W') &&
                        (*(_UIM + *(_Ptr - 3)) == 'W');
            if ( !isWWW )
            {
                return (false);
            }            
            #endregion

            #region [.find-url-end-on-the-right.]
            var right_len = FindUrlEndOnTheRight( 0 );
            #endregion

            #region [.create url_t.]
            var left_ptr = _Ptr - WWW_LENGTH;
#if DEBUG
var xxx = new string( left_ptr - 25, 0, 75 );
#endif
            var length = WWW_LENGTH + 1 + right_len;
            _Url.startIndex = (int) (left_ptr - _BASE);
            _Url.length     = length;
            if ( _ExtractValue )
            {
                _Url.value = new string( left_ptr, 0, length );
            }
            _Ptr += 1 + right_len;
            return (true);
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        private bool TryAllocateUrl_ByFirstLevelDomain( int maxRecursionNesting )
        {
            if ( maxRecursionNesting <= 0 )
            {
                return (false);    
            }

            #region [.check first-level-domain on the right.]
            char ch;
            var right_len = 0;
            for ( _Ptr++; ; right_len++ )
            {
                ch = _Ptr[ right_len ];
                //char - '\0' - not marked as CharType.IsLetter
                #region
                /*if ( ch == '\0' )
                {
                    break;
                }
                */
                #endregion
                if ( (_CTM[ ch ] & CharType.IsLetter) != CharType.IsLetter )
                {
                    break;
                }

                if ( _FirstLevelDomainsMaxLength < right_len )
                {
                    return (false);
                }

                //to upper
                _FldBufferPtrBase[ right_len ] = _UIM[ ch ];
            }

            if ( right_len == 0 )
            {
                return (false);
            }

            _StringBuilder.Clear().Append( _FirstLevelDomainBuffer, 0, right_len );
            if ( !_FirstLevelDomains.Contains( _StringBuilder.ToString() ) )
            {
                return (false);
            }
            #endregion

            #region [.find-url-end-on-the-right.]
            if ( xlat.IsDot( ch ) )
            {
#if DEBUG
var xxx1 = new string( _Ptr - 25, 0, 75 );
#endif
                var safe_Ptr = _Ptr;
                _Ptr += right_len;
                var res = TryAllocateUrl_ByFirstLevelDomain( maxRecursionNesting-- );
                if ( res )
                {
                    return (true);
                }
                _Ptr = safe_Ptr;
            }

            _Ptr--;
            if ( xlat.IsURIschemesPathSeparator( ch ) )
            {
                right_len = FindUrlEndOnTheRight( right_len );
            }
            #endregion

            #region [.find-url-end-on-the-left.]
            var left_len = FindUrlEndOnTheLeft( 1 );
            //skip url with empty left-part
            if ( left_len == 0 )
            {
                return (false);
            }
            #endregion

            #region [.create url_t.]
            var left_ptr = _Ptr - left_len;
#if DEBUG
var xxx = new string( left_ptr - 25, 0, 75 );
#endif
            var length = left_len + 1 + right_len;
            _Url.startIndex = (int) (left_ptr - _BASE);
            _Url.length     = length;
            if ( _ExtractValue )
            {
                _Url.value = new string( left_ptr, 0, length );
            }
            _Ptr += 1 + right_len;
            return (true);
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        private bool TryAllocateUrl_ByURIschemes()
        {
            #region [.check URI-schemes on the left.]
            var left_len = 0;
            for ( ; ; left_len++ )
            {
                var p = _Ptr - left_len - 1;
                if ( p < _BASE )
                {                    
                    break;
                }

                var ch = *p;
                if ( (_CTM[ ch ] & CharType.IsURIschemesChar) != CharType.IsURIschemesChar )
                {
                    break;
                }

                if ( _URIschemesMaxLength < left_len )
                {
                    return (false);
                }

                //to upper
                _UriSchBufferPtrBase[ left_len ] = _UIM[ ch ];
            }

            if ( left_len == 0 )
            {
                return (false);
            }

            _StringBuilder.Clear().Append( _URIschemesBuffer, 0, left_len );
            if ( !_URIschemes.Contains( _StringBuilder.ToString() ) )
            {
                _Ptr++;
                return (false);
            }
            #endregion

            #region [.find-url-end-on-the-right.]
            var right_len = FindUrlEndOnTheRight( 0 );
            #endregion

            #region [.create url_t.]
            var left_ptr = _Ptr - left_len;
#if DEBUG
var xxx = new string( left_ptr - 25, 0, 75 );
#endif
            var length = left_len + 1 + right_len;
            _Url.startIndex = (int) (left_ptr - _BASE);
            _Url.length     = length;
            if ( _ExtractValue )
            {
                _Url.value = new string( left_ptr, 0, length );
            }
            _Ptr += 1 + right_len;
            return (true);
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        private int FindUrlEndOnTheRight( int offsetToRight )
        {
            var right_len = offsetToRight;
            for ( ; ; right_len++ )
            {
                var ch = _Ptr[ right_len ];
                //char - '\0' - marked as CharType.IsUrlBreak
                #region
                /*if ( ch == '\0' )
                {
                    right_len--;
                    break;
                }*/
                #endregion

                if ( (_CTM[ ch ] & CharType.IsUrlBreak) == CharType.IsUrlBreak )
                {
                    for ( right_len--; 0 <= right_len; right_len-- )
                    {
                        ch = _Ptr[ right_len ];
                        if ( ch == '/' )
                            break;
                        if ( (_CTM[ ch ] & CharType.IsPunctuation) != CharType.IsPunctuation )
                            break;
                    }
                    break;

                    #region commented
                    /*
                    right_len--;
                    #region [.if ends with dot.]
                    ch = _Ptr[ right_len ];
                    if ( xlat.IsSentEndChar( ch ) )
                        right_len--;
                    #endregion
                    break;
                    */ 
                    #endregion
                }
            }
            return ((right_len > 0) ? right_len : 0);
        }
        /// <summary>
        /// 
        /// </summary>
        private int FindUrlEndOnTheLeft( int offsetToLeft )
        {
            var left_len = offsetToLeft;
            for ( ; ; left_len++ )
            {
                var p = _Ptr - left_len;
                if ( p <= _BASE )
                {
                    while ( p < _BASE )
                    {
                        p++;
                        left_len--;
                    }

                    for ( /*left_len--*/; 0 <= left_len; left_len-- )
                    {
                        var ch = *(_Ptr - left_len);
                        if ( ch == '/' )
                            break;
                        var ct = _CTM[ ch ];
                        if ( (ct & CharType.IsWhiteSpace) == CharType.IsWhiteSpace )
                            continue;
                        if ( (ct & CharType.IsPunctuation) != CharType.IsPunctuation )
                            break;
                    }
                    break;
                }

                if ( (_CTM[ *p ] & CharType.IsUrlBreak) == CharType.IsUrlBreak )
                {
                    for ( left_len--; 0 <= left_len; left_len-- )
                    {
                        var ch = *(_Ptr - left_len);
                        if ( ch == '/' )
                            break;
                        var ct = _CTM[ ch ];
                        if ( (ct & CharType.IsWhiteSpace) == CharType.IsWhiteSpace )
                            continue;
                        if ( (ct & CharType.IsPunctuation) != CharType.IsPunctuation )
                            break;
                    }
                    break;
                }
            }
            return ((left_len > 0) ? left_len : 0);
        }        
    }
}

namespace lingvo.urls
{
    /// <summary>
    /// 
    /// </summary>
    internal static class UrlDetectorExt
    {
        public static HashSet< string > ToHashset_4Urls( this IEnumerable< string > seq )
        {
            var hs = new HashSet< string >( seq.Select( d => (d != null) ? d.Trim().ToUpperInvariant() : null ).Where( d => !string.IsNullOrEmpty( d ) ) );
            return (hs);
        }
        public static HashSet< string > ToHashsetWithReverseValues_4Urls( this IEnumerable< string > seq )
        {
            var hs = new HashSet< string >( seq.Select( d => (d != null) ? new string( d.Trim().Reverse().ToArray() ).ToUpperInvariant() : null ).Where( d => !string.IsNullOrEmpty( d ) ) );
            return (hs);
        }
        public static int GetItemMaxLength_4Urls( this HashSet< string > hs )
        {
            return ((hs.Count != 0) ? hs.Max( d => d.Length ) : 0);
        }
    }
}
