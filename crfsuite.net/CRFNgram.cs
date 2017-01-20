using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace lingvo.crfsuite
{   
	/// <summary>
    /// N-грамма
	/// </summary>
    unsafe public sealed class CRFNgram : IDisposable
    {
        private readonly GCHandle _GCHandle;
        private char*             _AttributesHeaderBase;

		/// <summary>
		/// .ctor()
		/// </summary>
        /// <param name="crfAttributes">Составные части N-граммы</param>
		public CRFNgram( CRFAttribute[] crfAttributes )
        {
            CRFAttributes = crfAttributes;

            var attrs_len = CRFAttributes.Length;
            switch ( attrs_len )
            {
                case 1:
                #region
                {
                    CRFAttribute_0 = CRFAttributes[ 0 ];
                    //CRFAttributes  = null;

                    AttributesHeader = CRFAttribute_0.AttributeName + "[" + CRFAttribute_0.Position + ']' + '=';

                }
                #endregion
                break;

                case 2:
                #region
                {
                    CRFAttribute_0 = CRFAttributes[ 0 ];
                    CRFAttribute_1 = CRFAttributes[ 1 ];
                    //CRFAttributes  = null;

                    AttributesHeader = CRFAttribute_0.AttributeName + "[" + CRFAttribute_0.Position + ']' + '|' +
                                       CRFAttribute_1.AttributeName + "[" + CRFAttribute_1.Position + ']' + '=';
                }
                #endregion
                break;

                case 3:
                #region
                {
                    CRFAttribute_0 = CRFAttributes[ 0 ];
                    CRFAttribute_1 = CRFAttributes[ 1 ];
                    CRFAttribute_2 = CRFAttributes[ 2 ];
                    //CRFAttributes  = null;

                    AttributesHeader = CRFAttribute_0.AttributeName + "[" + CRFAttribute_0.Position + ']' + '|' +
                                       CRFAttribute_1.AttributeName + "[" + CRFAttribute_1.Position + ']' + '|' +
                                       CRFAttribute_2.AttributeName + "[" + CRFAttribute_2.Position + ']' + '=';
                }
                #endregion
                break;

                case 4:
                #region
                {
                    CRFAttribute_0 = CRFAttributes[ 0 ];
                    CRFAttribute_1 = CRFAttributes[ 1 ];
                    CRFAttribute_2 = CRFAttributes[ 2 ];
                    CRFAttribute_3 = CRFAttributes[ 3 ];
                    //CRFAttributes  = null;

                    AttributesHeader = CRFAttribute_0.AttributeName + "[" + CRFAttribute_0.Position + ']' + '|' +
                                       CRFAttribute_1.AttributeName + "[" + CRFAttribute_1.Position + ']' + '|' +
                                       CRFAttribute_2.AttributeName + "[" + CRFAttribute_2.Position + ']' + '|' +
                                       CRFAttribute_3.AttributeName + "[" + CRFAttribute_3.Position + ']' + '=';
                }
                #endregion
                break;

                case 5:
                #region
                {
                    CRFAttribute_0 = CRFAttributes[ 0 ];
                    CRFAttribute_1 = CRFAttributes[ 1 ];
                    CRFAttribute_2 = CRFAttributes[ 2 ];
                    CRFAttribute_3 = CRFAttributes[ 3 ];
                    CRFAttribute_4 = CRFAttributes[ 4 ];
                    //CRFAttributes  = null;

                    AttributesHeader = CRFAttribute_0.AttributeName + "[" + CRFAttribute_0.Position + ']' + '|' +
                                       CRFAttribute_1.AttributeName + "[" + CRFAttribute_1.Position + ']' + '|' +
                                       CRFAttribute_2.AttributeName + "[" + CRFAttribute_2.Position + ']' + '|' +
                                       CRFAttribute_3.AttributeName + "[" + CRFAttribute_3.Position + ']' + '|' +
                                       CRFAttribute_4.AttributeName + "[" + CRFAttribute_4.Position + ']' + '=';
                }
                #endregion
                break;

                case 6:
                #region
                {
                    CRFAttribute_0 = CRFAttributes[ 0 ];
                    CRFAttribute_1 = CRFAttributes[ 1 ];
                    CRFAttribute_2 = CRFAttributes[ 2 ];
                    CRFAttribute_3 = CRFAttributes[ 3 ];
                    CRFAttribute_4 = CRFAttributes[ 4 ];
                    CRFAttribute_5 = CRFAttributes[ 5 ];
                    //CRFAttributes  = null;

                    AttributesHeader = CRFAttribute_0.AttributeName + "[" + CRFAttribute_0.Position + ']' + '|' +
                                       CRFAttribute_1.AttributeName + "[" + CRFAttribute_1.Position + ']' + '|' +
                                       CRFAttribute_2.AttributeName + "[" + CRFAttribute_2.Position + ']' + '|' +
                                       CRFAttribute_3.AttributeName + "[" + CRFAttribute_3.Position + ']' + '|' +
                                       CRFAttribute_4.AttributeName + "[" + CRFAttribute_4.Position + ']' + '|' +
                                       CRFAttribute_5.AttributeName + "[" + CRFAttribute_5.Position + ']' + '=';
                }
                #endregion
                break;

                default:
                #region
                {
                    for ( var j = 0; j < attrs_len; j++ )
                    {
                        var attr = CRFAttributes[ j ];

                        AttributesHeader += attr.AttributeName + "[" + attr.Position + ']' + '|';
                    }
                    AttributesHeader = AttributesHeader.Remove( AttributesHeader.Length - 1 ) + '=';
                }
                #endregion
                break;
            }

            CRFAttributesLength    = attrs_len;
            AttributesHeaderLength = AttributesHeader.Length;

            _GCHandle = GCHandle.Alloc( AttributesHeader, GCHandleType.Pinned );
            _AttributesHeaderBase = (char*) _GCHandle.AddrOfPinnedObject().ToPointer();
        }
        ~CRFNgram()
        {
            if ( _AttributesHeaderBase != null )
            {
                _GCHandle.Free();
                _AttributesHeaderBase = null;
            }
        }
        public void Dispose()
        {
            if ( _AttributesHeaderBase != null )
            {
                _GCHandle.Free();
                _AttributesHeaderBase = null;
            }
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Составные части N-граммы
        /// </summary>
        public readonly CRFAttribute[] CRFAttributes;
        /// <summary>
        /// 
        /// </summary>
        public readonly int CRFAttributesLength;

        /// <summary>
        /// 
        /// </summary>
        public CRFAttribute CRFAttribute_0
        {
            get;
            private set;
        }
        /// <summary>
        /// 
        /// </summary>
        public CRFAttribute CRFAttribute_1
        {
            get;
            private set;
        }
        /// <summary>
        /// 
        /// </summary>
        public CRFAttribute CRFAttribute_2
        {
            get;
            private set;
        }
        /// <summary>
        /// 
        /// </summary>
        public CRFAttribute CRFAttribute_3
        {
            get;
            private set;
        }
        /// <summary>
        /// 
        /// </summary>
        public CRFAttribute CRFAttribute_4
        {
            get;
            private set;
        }
        /// <summary>
        /// 
        /// </summary>
        public CRFAttribute CRFAttribute_5
        {
            get;
            private set;
        }

        public readonly string AttributesHeader;
        public readonly int    AttributesHeaderLength;
        unsafe public char* CopyAttributesHeaderChars( char* attributeBuffer )
        {
            for ( var ptr = _AttributesHeaderBase; ; ptr++ )
            {
                var ch = *ptr;
                if ( ch == '\0' )
                    break;
                *(attributeBuffer++) = ch;
            }
            return (attributeBuffer);
        }
        unsafe public byte* CopyAttributesHeaderChars( byte* attributeBuffer )
        {
            for ( var ptr = _AttributesHeaderBase; ; ptr++ )
            {
                var ch = *ptr;
                if ( ch == '\0' )
                    break;
                *(attributeBuffer++) = (byte) ch;
            }
            return (attributeBuffer);
        }

        public bool CanTemplateBeApplied( int wordIndex, int wordsCount )
        {
            foreach ( CRFAttribute crfAttribute in CRFAttributes )
            {
                int index = wordIndex + crfAttribute.Position;
                if ( (index < 0) || (wordsCount <= index) )
                {
                    return (false);
                }
            }
            return (true);
        }

        public override string ToString()
        {
            return (AttributesHeader);
        }
    }
}