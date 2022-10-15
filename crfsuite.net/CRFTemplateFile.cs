using System;
using System.Collections.Generic;
using System.Linq;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace lingvo.crfsuite
{
    /// <summary>
    /// Внутреннее представление шаблона для построения входных данных SRFSuitNER
    /// </summary>
    public sealed class CRFTemplateFile
	{
        /// <summary>
        /// 
        /// </summary>
        private readonly struct index_count_t
        {
            /// <summary>
            /// 
            /// </summary>
            public sealed class EqualityComparer : IEqualityComparer< index_count_t >
            {
                public EqualityComparer() { }

                public bool Equals( index_count_t x, index_count_t y ) => (x.Index == y.Index) && (x.Count == y.Count);
                public int GetHashCode( index_count_t obj ) => (obj.Index.GetHashCode() ^ obj.Count.GetHashCode());
            }

            public index_count_t( int index, int count )
            {
                Index = index;
                Count = count;
            }

            public int Index { [M(O.AggressiveInlining)] get; }
            public int Count { [M(O.AggressiveInlining)] get; }
        }

        private int _MinCrfAttributePosition;
        private int _MaxCrfAttributePosition;
        private Dictionary< index_count_t, CRFNgram[] > _Dictionary;

        /// <summary>
        /// Конструктор шаблона для построения входных данных SRFSuitNER
        /// </summary>
        /// <param name="columnNames">Наименования столбцов преобразованного входного файла</param>
        /// <param name="crfNgrams">шаблоны N-грамм</param>
		public CRFTemplateFile( char[] columnNames, CRFNgram[] crfNgrams )
		{
            CheckTemplate( columnNames, crfNgrams );

			ColumnNames = columnNames;
			CRFNgrams   = crfNgrams;

            var positions = from crfNgram in CRFNgrams
                            from crfAttribute in crfNgram.CRFAttributes
                            select crfAttribute.Position;
            _MinCrfAttributePosition = positions.Min();
            _MaxCrfAttributePosition = positions.Max();

            _Dictionary = new Dictionary< index_count_t, CRFNgram[] >( new index_count_t.EqualityComparer() );
		}

		/// <summary>
        /// // Наименования столбцов преобразованного входного файла
		/// </summary>
		public char[] ColumnNames { [M(O.AggressiveInlining)] get; }

		/// <summary>
        /// // шаблоны N-грамм
		/// </summary>
        public CRFNgram[] CRFNgrams { [M(O.AggressiveInlining)] get; }

        [M(O.AggressiveInlining)] public CRFNgram[] GetCRFNgramsWhichCanTemplateBeApplied( int wordIndex, int wordsCount )
        {
            var i1 = wordIndex + _MinCrfAttributePosition;                    if ( 0 < i1 ) i1 = 0;
            var i2 = wordsCount - (wordIndex + _MaxCrfAttributePosition) - 1; if ( 0 < i2 ) i2 = 0;
            var wordIndexAndCountTuple = new index_count_t( i1, i2 );

            if ( !_Dictionary.TryGetValue( wordIndexAndCountTuple, out var ngrams ) )
            {
                var lst = new List< CRFNgram >();
                foreach ( var crfNgram in CRFNgrams )
                {
                    if ( crfNgram.CanTemplateBeApplied( wordIndex, wordsCount ) )
                    {
                        lst.Add( crfNgram );
                    }
                }
                ngrams = lst.ToArray();

                _Dictionary.Add( wordIndexAndCountTuple, ngrams );
            }
            return (ngrams);
        }


        private static void CheckTemplate( char[] columnNames, CRFNgram[] crfNgrams )
        {
            var columnNamesHS = new HashSet< char >( columnNames );

            foreach ( var ngram in crfNgrams )
            {
                foreach ( var ca in ngram.CRFAttributes )
                {
                    if ( !columnNamesHS.Contains( ca.AttributeName ) )
                    {
                        throw (new Exception( $"Аттрибут '{ca.AttributeName}' не содержащится в названиях столбцов CRF-файла-шаблона: '{string.Join( "', '", columnNames.Select( c => c.ToString() ) )}'" ));
                    }
                }
            }
        }
	};
}
