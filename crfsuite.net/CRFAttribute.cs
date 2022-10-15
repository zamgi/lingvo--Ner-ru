using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace lingvo.crfsuite
{
    /// <summary>
    /// Аттрибут линейного CRF алгоритма
    /// </summary>
    public sealed class CRFAttribute
	{
        public CRFAttribute( char attributeName, int position, int columnIndex )
        {
            AttributeName = attributeName;
            Position      = position; //PositionPlus1 = Position + 1;
            ColumnIndex   = columnIndex;
        }
	
        /// <summary>
        /// название аттрибута
        /// </summary>
        public char AttributeName { [M(O.AggressiveInlining)] get; }

        /// <summary>
        /// индекс позиции аттрибута
        /// </summary>
        public int Position { [M(O.AggressiveInlining)] get; }
        /// <summary>
        /// Position + 1 => comfortable for MorphoAmbiguityResolver
        /// </summary>
        //public readonly int PositionPlus1;

        /// <summary>
        /// 
        /// </summary>
        public int ColumnIndex { [M(O.AggressiveInlining)] get; }

        public override string ToString() => $"[{AttributeName}:{Position}], column-index: {ColumnIndex}";
	};
}