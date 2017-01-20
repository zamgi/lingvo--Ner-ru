using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using lingvo.core;
using lingvo.crfsuite;
using lingvo.tokenizing;

namespace lingvo.ner
{
    /// <summary>
    /// Конвертор в формат CRF
    /// </summary>
    unsafe public sealed class NerScriber : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private struct PinnedWord_t
        {
            public char*    basePtr;
            public GCHandle gcHandle;

            public int          length;
            public NerInputType nerInputType;
        }

        #region [.private field's.]
        private const char VERTICAL_SLASH = '|';
        private const char SLASH          = '\\';
        private const char COLON          = ':'; 

        private const int UTF8_BUFFER_SIZE         = 1024 * 16; //16KB
        private const int ATTRIBUTE_MAX_LENGTH     = UTF8_BUFFER_SIZE / 4; //4KB
        private const int WORD_MAX_LENGTH          = 0x100; //256-chars - fusking-enough
        private const int PINNED_WORDS_BUFFER_SIZE = 100;
        private static readonly char[] ALLOWED_COLUMNNAMES = new[] { 'w', 'c', 'y' };

        private static readonly Encoding UTF8_ENCODING = Encoding.UTF8;

		private readonly CRFTemplateFile _CrfTemplateFile;
        private IntPtr                   _Tagger;
        //private readonly byte[]          _UTF8Buffer;
        private readonly GCHandle        _UTF8BufferGCHandle;
        private byte*                    _UTF8BufferPtrBase;
        //private readonly char[]          _AttributeBuffer;
        private readonly GCHandle        _AttributeBufferGCHandle;
        private char*                    _AttributeBufferPtrBase;
        private char*                    _AttributeBufferPtr;
        //private char[]                  _PinnedWordsBuffer;
        private int                      _PinnedWordsBufferSize;
        private GCHandle                 _PinnedWordsBufferGCHandle;
        private PinnedWord_t*            _PinnedWordsBufferPtrBase;

        //private List< word_t >            _Words;
        private readonly List< string >   _Result4ModelBuilder;
        private readonly StringBuilder    _StringBuilder4ModelBuilder;
        private List< buildmodel_word_t > _Words4ModelBuilder;        
        private int                       _WordsCount_4ModelBuilder;
        private int                       _WordsCount_Minus1_4ModelBuilder;        
        #endregion

        #region [.ctor().]
        private NerScriber( string modelFilename, string templateFilename )
		{
            _CrfTemplateFile = CRFTemplateFileLoader.Load( templateFilename, ALLOWED_COLUMNNAMES );   

            //-0-
            native.load_native_crf_suite();
            var ptr = Marshal.StringToHGlobalAnsi( modelFilename );			
            _Tagger = native.crf_tagger_initialize( ptr );
            Marshal.FreeHGlobal( ptr );

            if ( _Tagger == IntPtr.Zero )
            {
				throw (new InvalidDataException("Не удалось открыть CRF-модель."));
            }

            //-1-
            //_UTF8Buffer      = new byte[ UTF8_BUFFER_SIZE ];
            var utf8Buffer      = new byte[ UTF8_BUFFER_SIZE ];
            _UTF8BufferGCHandle = GCHandle.Alloc( utf8Buffer, GCHandleType.Pinned );
            _UTF8BufferPtrBase  = (byte*) _UTF8BufferGCHandle.AddrOfPinnedObject().ToPointer();

            //-2-
            //_AttributeBuffer = new char[ ATTRIBUTE_MAX_LENGTH + 1 ];
            var attributeBuffer = new char[ ATTRIBUTE_MAX_LENGTH + 1 ];
            _AttributeBufferGCHandle = GCHandle.Alloc( attributeBuffer, GCHandleType.Pinned );
            _AttributeBufferPtrBase = (char*) _AttributeBufferGCHandle.AddrOfPinnedObject().ToPointer();

            //-3-
            ReAllocPinnedWordsBuffer( PINNED_WORDS_BUFFER_SIZE );
		}
        private NerScriber( string templateFilename )
		{
            _CrfTemplateFile = CRFTemplateFileLoader.Load( templateFilename, ALLOWED_COLUMNNAMES );
            _Result4ModelBuilder        = new List< string >();
            _StringBuilder4ModelBuilder = new StringBuilder();
		}

        public static NerScriber Create( string modelFilename, string templateFilename )
        {
            var nerScriber = new NerScriber( modelFilename, templateFilename );
            return (nerScriber);
        }
        public static NerScriber Create4ModelBuilder( string templateFilename )
        {
            var nerScriber = new NerScriber( templateFilename );
            return (nerScriber);
        }

        private void ReAllocPinnedWordsBuffer( int newBufferSize )
        {
            DisposePinnedWordsBuffer();

            _PinnedWordsBufferSize     = newBufferSize;
            var pinnedWordsBuffer      = new PinnedWord_t[ _PinnedWordsBufferSize ];
            _PinnedWordsBufferGCHandle = GCHandle.Alloc( pinnedWordsBuffer, GCHandleType.Pinned );
            _PinnedWordsBufferPtrBase  = (PinnedWord_t*) _PinnedWordsBufferGCHandle.AddrOfPinnedObject().ToPointer();
        }
        private void DisposePinnedWordsBuffer()
        {
            if ( _PinnedWordsBufferPtrBase != null )
            {
                _PinnedWordsBufferGCHandle.Free();
                _PinnedWordsBufferPtrBase = null;
            }
        }

        ~NerScriber()
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
            if ( _Tagger != IntPtr.Zero )
            {
                native.crf_tagger_uninitialize( _Tagger );
                _Tagger = IntPtr.Zero;
            }

            if ( _AttributeBufferPtrBase != null )
            {
                _AttributeBufferGCHandle.Free();
                _AttributeBufferPtrBase = null;
            }

            if ( _UTF8BufferPtrBase != null )
            {
                _UTF8BufferGCHandle.Free();
                _UTF8BufferPtrBase = null;
            }

            DisposePinnedWordsBuffer();
        }
        #endregion

        public void Run( List< word_t > words )
        {
            #region [.init.]
            if ( !Init( words ) )
            {
                return;
            }
            var wordsCount        = words.Count;
            var wordsCount_Minus1 = wordsCount - 1;
            #if DEBUG
                var sb_attr_debug = new StringBuilder();
            #endif
            #endregion

            native.crf_tagger_beginAddItemSequence( _Tagger );

            #region [.put-attr-values-to-crf.]
            for ( int wordIndex = 0; wordIndex < wordsCount; wordIndex++ )
            {
                native.crf_tagger_beginAddItemAttribute( _Tagger );

                #region [.process-crf-attributes-by-word.]
                native.crf_tagger_addItemAttributeNameOnly( _Tagger, xlat_Unsafe.Inst._NerInputtypeOtherPtrBase );
                #if DEBUG
                    sb_attr_debug.Append( xlat_Unsafe.INPUTTYPE_OTHER ).Append( '\t' );
                #endif

                var ngrams = _CrfTemplateFile.GetCRFNgramsWhichCanTemplateBeApplied( wordIndex, wordsCount );
                for ( int i = 0, ngramsLength = ngrams.Length; i < ngramsLength; i++ )
                {
                    var ngram = ngrams[ i ];

                    _AttributeBufferPtr = ngram.CopyAttributesHeaderChars( _AttributeBufferPtrBase );

                    #region [.build attr-values.]
                    switch ( ngram.CRFAttributesLength )
                    {
                        case 1:
                        #region
                        {
                            AppendAttrValue( wordIndex, ngram.CRFAttribute_0 );
                        }
                        #endregion
                        break;

                        case 2:
                        #region
                        {
                            AppendAttrValue( wordIndex, ngram.CRFAttribute_0 ); *(_AttributeBufferPtr++) = VERTICAL_SLASH;
                            AppendAttrValue( wordIndex, ngram.CRFAttribute_1 );
                        }
                        #endregion
                        break;

                        case 3:
                        #region
                        {
                            AppendAttrValue( wordIndex, ngram.CRFAttribute_0 ); *(_AttributeBufferPtr++) = VERTICAL_SLASH;
                            AppendAttrValue( wordIndex, ngram.CRFAttribute_1 ); *(_AttributeBufferPtr++) = VERTICAL_SLASH;
                            AppendAttrValue( wordIndex, ngram.CRFAttribute_2 );
                        }
                        #endregion
                        break;

                        default:
                        #region
                        {
                            for ( var j = 0; j < ngram.CRFAttributesLength; j++ )
                            {
                                var attr_ = ngram.CRFAttributes[ j ];
                                AppendAttrValue( wordIndex, attr_ );
                                *(_AttributeBufferPtr++) = VERTICAL_SLASH;
                            }
                            // Удалить последний '|'
                            _AttributeBufferPtr--;
                        }
                        #endregion
                        break;
                    }
                    #endregion

                    #region [.add-attr-values.]
                    *(_AttributeBufferPtr++) = '\0';
                    var attr_len_with_zero = Math.Min( ATTRIBUTE_MAX_LENGTH, (int) (_AttributeBufferPtr - _AttributeBufferPtrBase) );
                    UTF8_ENCODING.GetBytes( _AttributeBufferPtrBase, attr_len_with_zero, _UTF8BufferPtrBase, UTF8_BUFFER_SIZE ); //var bytesWritten = UTF8_ENCODER.GetBytes( attr_ptr, attr_len, utf8buffer, UTF8_BUFFER_SIZE, true ); 
                    native.crf_tagger_addItemAttributeNameOnly( _Tagger, _UTF8BufferPtrBase );
                    #if DEBUG
                        var s_debug = new string( _AttributeBufferPtrBase, 0, attr_len_with_zero - 1 );
                        sb_attr_debug.Append( s_debug ).Append( '\t' );
                    #endif
                    #endregion
                }

                if ( wordIndex == 0 )
                {
                    native.crf_tagger_addItemAttributeNameOnly( _Tagger, xlat_Unsafe.Inst._BeginOfSentencePtrBase );
                    #if DEBUG
                        sb_attr_debug.Append( xlat_Unsafe.BEGIN_OF_SENTENCE ).Append( '\t' );
                    #endif
                }
                else
                if ( wordIndex == wordsCount_Minus1 )
                {
                    native.crf_tagger_addItemAttributeNameOnly( _Tagger, xlat_Unsafe.Inst._EndOfSentencePtrBase );
                    #if DEBUG
                        sb_attr_debug.Append( xlat_Unsafe.END_OF_SENTENCE ).Append( '\t' );
                    #endif
                }
                #endregion

                native.crf_tagger_endAddItemAttribute( _Tagger );
                #if DEBUG
                    sb_attr_debug.Append( '\n' );
                #endif
            }
            #endregion

            native.crf_tagger_endAddItemSequence( _Tagger );
            #if DEBUG
                var attr_debug = sb_attr_debug.ToString();
            #endif

            #region [.run-crf-tagging-words.]
            native.crf_tagger_tag( _Tagger );
            #endregion

            #region [.get-crf-tagging-data.]
            System.Diagnostics.Debug.Assert( native.crf_tagger_getResultLength( _Tagger ) == wordsCount, "(native.crf_tagger_getResultLength( _Tagger ) != _WordsCount)" );
            for ( var i = 0; i < wordsCount; i++ )
            {
                var ptr = native.crf_tagger_getResultValue( _Tagger, (uint) i );

                var value = (byte*) ptr.ToPointer();
                words[ i ].nerOutputType = NerExtensions.ToNerOutputType( value );

                //free pinned-gcHandle
                (_PinnedWordsBufferPtrBase + i)->gcHandle.Free();
            }
            #endregion

            #region [.un-init.]
            //Uninit();
            #endregion
        }

        private bool Init( List< word_t > words )
        {
            if ( words.Count == 0 )
            {
                return (false);
            }

            //_Words = words;
            var wordsCount = words.Count;

            if ( _PinnedWordsBufferSize < wordsCount )
            {
                ReAllocPinnedWordsBuffer( wordsCount );
            }
            for ( var i = 0; i < wordsCount; i++ )
            {
                var word     = words[ i ];
                var gcHandle = GCHandle.Alloc( word.valueOriginal, GCHandleType.Pinned );
                var basePtr  = (char*) gcHandle.AddrOfPinnedObject().ToPointer();
                PinnedWord_t* pw = _PinnedWordsBufferPtrBase + i;
                pw->basePtr  = basePtr;
                pw->gcHandle = gcHandle;

                pw->nerInputType = word.nerInputType;
                pw->length       = word.valueOriginal.Length;
            }

            return (true);
        }
        /*private void Uninit()
        {
            for ( var i = 0; i < _WordsCount; i++ )
            {
                (_PinnedWordsBufferPtrBase + i)->gcHandle.Free();
            }
            //_Words = null;
        }*/

        private void AppendAttrValue( int wordIndex, CRFAttribute crfAttribute )
        {
            switch ( crfAttribute.AttributeName )
            {
                case 'w':
                #region
                {
                    /*
                    символы ':' '\'
                    - их комментировать в поле "w", "\:" и "\\"
                    */
                    var index = wordIndex + crfAttribute.Position;
                    var pw = (_PinnedWordsBufferPtrBase + index);

                    char* _base = pw->basePtr;
                    switch ( *_base )
                    {
                        case COLON:
                            *(_AttributeBufferPtr++) = SLASH;
                            *(_AttributeBufferPtr++) = COLON;
                        break;

                        case SLASH:
                            *(_AttributeBufferPtr++) = SLASH;
                            *(_AttributeBufferPtr++) = SLASH;
                        break;

                        default:
                            //---System.Diagnostics.Debug.Assert( word.valueOriginal.Length <= WORD_MAX_LENGTH, "!(word.valueOriginal.Length <= WORD_MAX_LENGTH)" );
                            //---System.Diagnostics.Debug.Assert( word.length == word.valueOriginal.Length, "!(word.length == word.valueOriginal.Length)" );
                            for ( int i = 0, len = Math.Min( WORD_MAX_LENGTH, pw->length ); i < len; i++ )
                            {
                                *(_AttributeBufferPtr++) = *(_base + i);
                            }
                            #region commented
                            /*
                            for ( int i = 0; i < WORD_MAX_LENGTH; i++ )
                            {
                                var ch = *(_base + i);
                                if ( ch == '\0' )
                                    break;
                                *(_AttributeBufferPtr++) = ch;
                            }
                            */
                            #endregion
                        break;
                    }
                }
                #endregion
                break;

                case 'c':
                #region
                {
                    var index = wordIndex + crfAttribute.Position;
                    *(_AttributeBufferPtr++) = (_PinnedWordsBufferPtrBase + index)->nerInputType.ToCrfChar();
                }
                #endregion
                break;

                case 'y':
                #region
                {
                    *(_AttributeBufferPtr++) = 'O'; //NERINPUTTYPE_OTHER == "O"
                }
                #endregion
                break;

                #if DEBUG
                default: throw (new InvalidDataException( "Invalid column-name: '" + crfAttribute.AttributeName + "'" ));
                #endif
            }
        }
        /*private void AppendAttrValue__previous( int wordIndex, CRFAttribute crfAttribute )
        {
            switch ( crfAttribute.AttributeName )
            {
                case 'w':
                #region
                {
                    /*
                    символы ':' '\'
                    - их комментировать в поле "w", "\:" и "\\"
                    * /
                    var index = wordIndex + crfAttribute.Position;
                    var word = _Words[ index ];
                    fixed ( char* _base = word.valueOriginal )
                    {
                        switch ( *_base )
                        {
                            case COLON:
                                *(_AttributeBufferPtr++) = SLASH;
                                *(_AttributeBufferPtr++) = COLON;
                            break;

                            case SLASH:
                                *(_AttributeBufferPtr++) = SLASH;
                                *(_AttributeBufferPtr++) = SLASH;
                            break;

                            default:
                                System.Diagnostics.Debug.Assert( word.valueOriginal.Length <= WORD_MAX_LENGTH, "!(word.valueOriginal.Length <= WORD_MAX_LENGTH)" );
                                System.Diagnostics.Debug.Assert( word.length == word.valueOriginal.Length, "!(word.length == word.valueOriginal.Length)" );
                                for ( int i = 0; i < WORD_MAX_LENGTH; i++ )
                                {
                                    var ch = *(_base + i);
                                    if ( ch == '\0' )
                                        break;
                                    *(_AttributeBufferPtr++) = ch;
                                }
                            break;
                        }
                    }
                }
                #endregion
                break;

                case 'c':
                #region
                {
                    var index = wordIndex + crfAttribute.Position;
                    *(_AttributeBufferPtr++) = _Words[ index ].nerInputType.ToCrfChar();
                }
                #endregion
                break;

                case 'y':
                #region
                {
                    *(_AttributeBufferPtr++) = 'O'; //NERINPUTTYPE_OTHER == "O"
                }
                #endregion
                break;
            }
        }*/

        #region [.model-builder.]
        private void AppendAttrValue4ModelBuilder( int wordIndex, CRFAttribute crfAttribute )
        {
            switch ( crfAttribute.AttributeName )
            {
                case 'w':
                #region
                {
                    /*
                    символы ':' '\'
                    - их комментировать в поле "w", "\:" и "\\"
                    */
                    var index = wordIndex + crfAttribute.Position;
                    var word = _Words4ModelBuilder[ index ].word;
                    switch ( word.valueOriginal[ 0 ] )
                    {
                        case COLON:
                            _StringBuilder4ModelBuilder.Append( SLASH ).Append( COLON );
                        break;

                        case SLASH:
                            _StringBuilder4ModelBuilder.Append( SLASH ).Append( SLASH );
                        break;

                        default:
                            _StringBuilder4ModelBuilder.Append( word.valueOriginal );
                        break;
                    }
                }
                #endregion
                break;

                case 'c':
                #region
                {
                    var index = wordIndex + crfAttribute.Position;
                    _StringBuilder4ModelBuilder.Append( _Words4ModelBuilder[ index ].word.nerInputType.ToCrfChar() );
                }
                #endregion
                break;

                case 'y':
                #region
                {
                    _StringBuilder4ModelBuilder.Append( xlat_Unsafe.INPUTTYPE_OTHER /*NerInputType.O.ToCrfChar()*/ );
                }
                #endregion
                break;

                #if DEBUG
                default: throw (new InvalidDataException( "Invalid column-name: '" + crfAttribute.AttributeName + "'" ));
                #endif
            }
        }

        public void WriteCrfAttributesWords4ModelBuilder( TextWriter textWriter, List< buildmodel_word_t > words )
        {
            #region [.init.]
            _Words4ModelBuilder              = words;
            _WordsCount_4ModelBuilder        = _Words4ModelBuilder.Count;
            _WordsCount_Minus1_4ModelBuilder = _WordsCount_4ModelBuilder - 1;
            #endregion

            #region [.write-crf-attributes-words.]
            for ( int i = 0; i < _WordsCount_4ModelBuilder; i++ )
			{
                var atts = GetNerAttributes4ModelBuilder( i );
                for ( int j = 0, len = atts.Count; j < len; j++ )
                {
                    textWriter.Write( atts[ j ] );
                    textWriter.Write( '\t' );
                }
                textWriter.Write( '\n' );
			}

            textWriter.Write( '\n' );
            #endregion

            #region [.un-init.]
            _Words4ModelBuilder = null;
            #endregion
        }

        public void BeginGetNerAttributes4ModelBuilder( List< buildmodel_word_t > words )
        {
            _Words4ModelBuilder              = words;
            _WordsCount_4ModelBuilder        = _Words4ModelBuilder.Count;
            _WordsCount_Minus1_4ModelBuilder = _WordsCount_4ModelBuilder - 1;
        }
        public void EndGetNerAttributes4ModelBuilder()
        {
            _Words4ModelBuilder = null;
        }
        public List< string > GetNerAttributes4ModelBuilder( int wordIndex )
        {
            _Result4ModelBuilder.Clear();

            var bmword_current = _Words4ModelBuilder[ wordIndex ];

            _Result4ModelBuilder.Add( bmword_current.buildModelNerInputType.ToText() );

            var ngrams = _CrfTemplateFile.GetCRFNgramsWhichCanTemplateBeApplied( wordIndex, _WordsCount_4ModelBuilder );
            var ngramsLength = ngrams.Length;
            for ( var i = 0; i < ngramsLength; i++  )
            {
                var ngram = ngrams[ i ];

                _StringBuilder4ModelBuilder.Clear().Append( ngram.AttributesHeader );

                #region [.build attr-values.]
                var attrs_len = ngram.CRFAttributes.Length;
                switch ( attrs_len )
                {
                    case 1:
                    #region
                    {
                        var attr1 = ngram.CRFAttributes[ 0 ];

                        AppendAttrValue4ModelBuilder( wordIndex, attr1 );		        
                    }
                    #endregion
                    break;

                    case 2:
                    #region
                    {
                        var attr1 = ngram.CRFAttributes[ 0 ];
                        var attr2 = ngram.CRFAttributes[ 1 ];

                        AppendAttrValue4ModelBuilder( wordIndex, attr1 ); _StringBuilder4ModelBuilder.Append( VERTICAL_SLASH );
                        AppendAttrValue4ModelBuilder( wordIndex, attr2 );
                    }
                    #endregion
                    break;

                    case 3:
                    #region
                    {
                        var attr1 = ngram.CRFAttributes[ 0 ];
                        var attr2 = ngram.CRFAttributes[ 1 ];
                        var attr3 = ngram.CRFAttributes[ 2 ];

                        AppendAttrValue4ModelBuilder( wordIndex, attr1 ); _StringBuilder4ModelBuilder.Append( VERTICAL_SLASH );
                        AppendAttrValue4ModelBuilder( wordIndex, attr2 ); _StringBuilder4ModelBuilder.Append( VERTICAL_SLASH );
                        AppendAttrValue4ModelBuilder( wordIndex, attr3 );
                    }
                    #endregion
                    break;

                    default:
                    #region
                    {
                        for ( var j = 0; j < attrs_len; j++ )
			            {
                            var attr = ngram.CRFAttributes[ j ];
                            AppendAttrValue4ModelBuilder( wordIndex, attr );
				            _StringBuilder4ModelBuilder.Append( VERTICAL_SLASH );
			            }
			            // Удалить последний |
			            _StringBuilder4ModelBuilder.Remove( _StringBuilder4ModelBuilder.Length - 1, 1 );
                    }
                    #endregion
                    break;
                }
                #endregion

			    _Result4ModelBuilder.Add( _StringBuilder4ModelBuilder.ToString() );                    
            }

			if ( wordIndex == 0 )
            {
                _Result4ModelBuilder.Add( xlat_Unsafe.BEGIN_OF_SENTENCE );
            }
            else 
            if ( wordIndex == _WordsCount_Minus1_4ModelBuilder )
            {
                _Result4ModelBuilder.Add( xlat_Unsafe.END_OF_SENTENCE );
            }

            return (_Result4ModelBuilder);
        }
        #endregion
    };
}



