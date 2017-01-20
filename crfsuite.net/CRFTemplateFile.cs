using System;
using System.Collections.Generic;
using System.Linq;

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
        private struct index_count_t : IEqualityComparer< index_count_t >
        {
            public static readonly index_count_t Instance = new index_count_t();

            public index_count_t( int index, int count )
            {
                Index = index;
                Count = count;
            }

            public int Index;
            public int Count;

            public bool Equals( index_count_t x, index_count_t y )
            {
                return ((x.Index == y.Index) && (x.Count == y.Count));
            }
            public int GetHashCode( index_count_t obj )
            {
                return (Index.GetHashCode() ^ Count.GetHashCode());
            }
        }

        private readonly int _MinCrfAttributePosition;
        private readonly int _MaxCrfAttributePosition;
        private readonly Dictionary< index_count_t, CRFNgram[] > _Dictionary;

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

            _Dictionary = new Dictionary< index_count_t, CRFNgram[] >( index_count_t.Instance );
		}

		/// <summary>
        /// // Наименования столбцов преобразованного входного файла
		/// </summary>
		public char[] ColumnNames
		{
            get;
            private set;
		}

		/// <summary>
        /// // шаблоны N-грамм
		/// </summary>
        public CRFNgram[] CRFNgrams
        {
            get;
            private set;
        }        

        public CRFNgram[] GetCRFNgramsWhichCanTemplateBeApplied( int wordIndex, int wordsCount )
        {
            var i1 = wordIndex + _MinCrfAttributePosition;                    if ( 0 < i1 ) i1 = 0;
            var i2 = wordsCount - (wordIndex + _MaxCrfAttributePosition) - 1; if ( 0 < i2 ) i2 = 0;
            var wordIndexAndCountTuple = new index_count_t( i1, i2 );

            CRFNgram[] ngrams;
            if ( !_Dictionary.TryGetValue( wordIndexAndCountTuple, out ngrams ) )
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


        private static void CheckTemplate( char[] _columnNames, CRFNgram[] _crfNgrams )
        {
            var columnNames = new HashSet< char >( _columnNames );

            foreach ( CRFNgram ngram in _crfNgrams )
            {
                foreach ( CRFAttribute crfAttribute in ngram.CRFAttributes )
                {
                    if ( !columnNames.Contains( crfAttribute.AttributeName ) )
                    {
                        throw (new Exception( "Аттрибут '" + crfAttribute.AttributeName + "' не содержащится в названиях столбцов CRF-файла-шаблона: '" +
                                             string.Join( "', '", _columnNames.Select( c => c.ToString() ) ) + '\'' ));
                    }
                }
            }
        }
	};
}
