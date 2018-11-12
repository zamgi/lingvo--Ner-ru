using System.Text;

namespace lingvo.sentsplitting
{
    /// <summary>
    /// 
    /// </summary>
    unsafe internal sealed class ss_word_t
    {
        public ss_word_t( char* _startPtr, int _length )
        {
            startPtr      = _startPtr;
            length        = _length;
            valueOriginal = new string( _startPtr, 0, _length );
        }

        public string valueOriginal;
        public string valueUpper;
        public char*  startPtr;
        public int    length;
        public ss_word_t prev;
        public ss_word_t next;

        public char* endPtr()
        {
            return (startPtr + length);
        }
        public bool  hasPrev
        {
            get { return (prev != null); }
        }
        public bool  hasNext
        {
            get { return (next != null); }    
        }

        public override string ToString()
        {
            return ('\'' + valueOriginal + "' [0x" + ((long) startPtr).ToString( "x" ) + ":" + length + "]");
        }
        public string GetAllWordsChain()
        {
            var sb = new StringBuilder();
            for ( var w = this; w != null; w = w.next )
            {
                sb.Append( w.valueOriginal ).Append( ' ' );
            }
            sb.Remove( sb.Length - 1, 1 );
            return (sb.ToString());
        }
    }
}
