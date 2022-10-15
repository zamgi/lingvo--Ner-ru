using System.Collections.Generic;
using System.Linq;

using lingvo.core;
using lingvo.tokenizing;

namespace lingvo.ner
{
    /*
Объединение  типов  цепочек сущностей

Типы, возвращаемые NER
NAME, ORG, GEO, EVENT, PROD.

Правила объединения типов NER в лингвистические типы.
1.	Шесть NAME подряд без знаков препинания = первые три NAME и вторые три NAME. 
2.	Пять NAME подряд без знаков препинания = первые три NAME и вторые два NAME.
3.	Четыре NAME подряд без знаков препинания = первые два NAME и вторые два NAME.
4.	Два или три подряд, без знаков препинания NAME= NAME; 
5.	Инициалы + NAME; (инициалы: слово с заглавной буквы с точкой на конце, не более трех букв, стоящие перед NAME) = NAME;
6.	NAME + бен, ван, ван дер, да, дель, дер, де, ди, дю, ла, ле, фон, эль, ben, da, de, del, der, des, du, el, la, le, les, van, van der, von + NAME = NAME;
7.	Два и более подряд, без знаков препинания EVENT = EVENT;
8.	Два и более подряд, без знаков препинания PROD= PROD;
9.	Сокращения (м.б. прописными и заглавными): г.; д.; н.; наб.; о.; пер.; пл.; пос.; п.; пр.; р.; ст.; с.; ул.; х.; + GEO = GEO;
10.	ORG +им.+ NAME  или ORG +имени+ NAME  = ORG ;
11.	GEO + ORG или ORG + GEO  = ORG;
12.	Слово с заглавной буквы, но не первое слово в предложении + ORG = ORG;
13.	ORG + слово с заглавной буквы = ORG;
14.	Слово: все буквы заглавные + ORG = ORG;
15.	Два и более подряд, без знаков препинания ORG = ORG;
---(16.	Два и более подряд, без знаков препинания GEO= GEO;)---
    */

    /// <summary>
    /// Combining types of chains of entities
    /// </summary>
    public static class NerPostMerging
    {
        private static readonly Searcher          _Searcher;
        private static readonly HashSet< string > _SingleWordsBetween2NAME;
        private static readonly HashSet< string > _SingleWordsBeforeGEO;
        private static readonly int               _MaxLengthSingleWordsBeforeGEO;

        static NerPostMerging()
        {
            //-1-
            var ngrams = new List< ngram_t >();

            var ngram = default(ngram_t);
            //1. Шесть NAME подряд без знаков препинания = первые три NAME и вторые три NAME. 
            //2. Пять  NAME подряд без знаков препинания = первые три NAME и вторые два NAME.
            //3. Четыре NAME подряд без знаков препинания = первые два NAME и вторые два NAME.
            //4. Два или три подряд, без знаков препинания NAME = NAME; 
            for ( var i = 2; i <= 6; i++ )
            {
                ngram = new ngram_t( Enumerable.Repeat( NerOutputType.NAME, i ).ToArray(), NerOutputType.NAME );

                ngrams.Add( ngram );
            }

            //7.  Два и более подряд, без знаков препинания ENTR = ENTR;
            //8.  Два и более подряд, без знаков препинания PROD = PROD;
            //15. Два и более подряд, без знаков препинания ORG  = ORG;
            //---(16. Два и более подряд, без знаков препинания GEO  = GEO;)---
            for ( var i = 2; i <= 50; i++ )
            {
                ngram = new ngram_t( Enumerable.Repeat( NerOutputType.ENTR, i ).ToArray(), NerOutputType.ENTR );
                ngrams.Add( ngram );

                ngram = new ngram_t( Enumerable.Repeat( NerOutputType.PROD, i ).ToArray(), NerOutputType.PROD );
                ngrams.Add( ngram );

                ngram = new ngram_t( Enumerable.Repeat( NerOutputType.ORG, i ).ToArray(), NerOutputType.ORG );
                ngrams.Add( ngram );

                //ngram = new ngram_t( Enumerable.Repeat( NerOutputType.GEO, i ).ToArray(), NerOutputType.GEO );
                //ngrams.Add( ngram );
            }

            //11. GEO + ORG или ORG + GEO  = ORG;
            ngram = new ngram_t( new[] { NerOutputType.GEO, NerOutputType.ORG }, NerOutputType.ORG );
            ngrams.Add( ngram );
            ngram = new ngram_t( new[] { NerOutputType.ORG, NerOutputType.GEO }, NerOutputType.ORG );
            ngrams.Add( ngram );

            _Searcher = new Searcher( ngrams );

            //-2-
            _SingleWordsBetween2NAME = new HashSet< string >( 
                    new[] { 
                        "бен", "ван", "да", "дель", "дер", "де", "ди", 
                        "дю", "ла", "ле", "фон", "эль", "ben", "da", 
                        "de", "del", "der", "des", "du", "el", "la", 
                        "le", "les", "van", "van der", "von"
                    } 
                );

            //-3-
            var items = new[] { 
                        "г.", "д.", "н.", "наб.", 
                        "о.", "пер.", "пл.", "пос.", 
                        "п.", "пр.", "р.", "ст.", 
                        "с.", "ул.", "х." 
                    }.Select( _ => _.ToUpperInvariant() );
            _SingleWordsBeforeGEO = new HashSet< string >( items );
            _MaxLengthSingleWordsBeforeGEO = _SingleWordsBeforeGEO.Max( _ => _.Length );
        }

        public static void Run( List< word_t > words )
        {
            var srs = _Searcher.FindAll( words );
            if ( srs != null )
            {
                Merge4Searcher( srs, words );
            }

            OtherRules( words );
        }

        private static void Merge4Searcher( ICollection< SearchResult > srs, List< word_t > words )
        {
            foreach ( var sr in srs )
            {
                var w1 = words[ sr.StartIndex ];
                if ( w1.IsWordInNerChain )
                    continue;

                if ( sr.NerOutputType == NerOutputType.NAME )
                {
                    switch ( sr.Length )
                    {
                        case 2:
                        #region
                        {
                            var w2 = words[ sr.StartIndex + 1 ];
                                            
                            w1.SetNextPrev( w2, sr.NerOutputType );
                        }
                        #endregion
                        break;

                        case 3:
                        #region
                        {
                            var w2 = words[ sr.StartIndex + 1 ];
                            var w3 = words[ sr.StartIndex + 2 ];
                                            
                            w1.SetNextPrev( w2, sr.NerOutputType );
                            w2.SetNextPrev( w3, sr.NerOutputType );
                        }
                        #endregion
                        break;

                        case 4:
                        #region
                        {
                            var w2 = words[ sr.StartIndex + 1 ];
                            var w3 = words[ sr.StartIndex + 2 ];
                            var w4 = words[ sr.StartIndex + 3 ];
                        
                            w3.SetNextPrev( w4, sr.NerOutputType );

                            w1.SetNextPrev( w2, sr.NerOutputType );
                        }
                        #endregion
                        break;

                        case 5:
                        #region
                        {
                            var w2 = words[ sr.StartIndex + 1 ];
                            var w3 = words[ sr.StartIndex + 2 ];
                            var w4 = words[ sr.StartIndex + 3 ];
                            var w5 = words[ sr.StartIndex + 4 ];

                            w4.SetNextPrev( w5, sr.NerOutputType );

                            w1.SetNextPrev( w2, sr.NerOutputType );
                            w2.SetNextPrev( w3, sr.NerOutputType );
                        }
                        #endregion
                        break;

                        case 6:
                        #region
                        {
                            var w2 = words[ sr.StartIndex + 1 ];
                            var w3 = words[ sr.StartIndex + 2 ];
                            var w4 = words[ sr.StartIndex + 3 ];
                            var w5 = words[ sr.StartIndex + 4 ];
                            var w6 = words[ sr.StartIndex + 5 ];
                        
                            w4.SetNextPrev( w5, sr.NerOutputType );
                            w5.SetNextPrev( w6, sr.NerOutputType );

                            w1.SetNextPrev( w2, sr.NerOutputType );
                            w2.SetNextPrev( w3, sr.NerOutputType );
                        }
                        #endregion
                        break;

                        default:
                        #region
                        {
                            /*Merge( ss, words, w1 );*/
                            for ( int i = sr.StartIndex + 1, len = i + sr.Length - 1; i < len; i++ )
                            {
                                var w = words[ i ];
                                w1.SetNextPrev( w, sr.NerOutputType );
                                w1 = w;
                            }
                        }
                        #endregion
                        break;
                    }
                }
                else
                {
                    Merge( sr, words, w1 );
                }
            }
        }
        private static void Merge( SearchResult sr, List< word_t > words, word_t _w1 )
        {
            var w1 = _w1 ?? words[ sr.StartIndex ];

            switch ( sr.Length )
            {
                case 2:
                #region
                {
                    var w2 = words[ sr.StartIndex + 1 ];
                                        
                    w1.SetNextPrev( w2, sr.NerOutputType );
                }
                #endregion
                break;

                case 3:
                #region
                {
                    var w2 = words[ sr.StartIndex + 1 ];
                    var w3 = words[ sr.StartIndex + 2 ];

                    w1.SetNextPrev( w2, sr.NerOutputType );
                    w2.SetNextPrev( w3, sr.NerOutputType );
                }
                #endregion
                break;

                case 4:
                #region
                {
                    var w2 = words[ sr.StartIndex + 1 ];
                    var w3 = words[ sr.StartIndex + 2 ];
                    var w4 = words[ sr.StartIndex + 3 ];

                    w1.SetNextPrev( w2, sr.NerOutputType );
                    w2.SetNextPrev( w3, sr.NerOutputType );
                    w3.SetNextPrev( w4, sr.NerOutputType );
                }
                #endregion
                break;

                case 5:
                #region
                {
                    var w2 = words[ sr.StartIndex + 1 ];
                    var w3 = words[ sr.StartIndex + 2 ];
                    var w4 = words[ sr.StartIndex + 3 ];
                    var w5 = words[ sr.StartIndex + 4 ];

                    w1.SetNextPrev( w2, sr.NerOutputType );
                    w2.SetNextPrev( w3, sr.NerOutputType );
                    w3.SetNextPrev( w4, sr.NerOutputType );
                    w4.SetNextPrev( w5, sr.NerOutputType );
                }
                #endregion
                break;

                default:
                #region
                {
                    for ( int i = sr.StartIndex + 1, len = i + sr.Length - 1; i < len; i++ )
                    {
                        var w = words[ i ];
                        w1.SetNextPrev( w, sr.NerOutputType );
                        w1 = w;
                    }
                }
                #endregion
                break;
            }
        }

        private static void OtherRules( List< word_t > words )
        {
            for ( int i = 0, len = words.Count; i < len; i++ )
            {
                var w = words[ i ];

                switch ( w.nerOutputType )
                {
                    case NerOutputType.NAME:
                    {
                        #region [.rules.]
                        var i_prev = (0 < i) ? (i - 1) : -1;

                        #region [. 5. Инициалы + NAME; (инициалы: слово с заглавной буквы с точкой на конце, не более трех букв, стоящие перед NAME) = NAME .]
                        if ( i_prev != -1 )
                        {
                            var word_prev = words[ i_prev ];
                            if ( (word_prev.nerOutputType == NerOutputType.O) &&
                                 (word_prev.length <= 3) && 
                                 xlat_Unsafe.Inst.IsUpper( word_prev.valueOriginal[ 0 ] ) &&
                                 xlat.IsDot( word_prev.valueOriginal[ word_prev.length - 1 ] ) 
                               )
                            {
                                var ss = new SearchResult( i_prev, 2, NerOutputType.NAME );
                                Merge( ss, words, word_prev );
                            }
                        }
                        #endregion

                        i += w.GetNerChainLength() - 1;

                        #region [. 6. NAME + бен, ван, да, дель, дер, де, ди, дю, ла, ле, фон, эль, ben, da, de, del, der, des, du, el, la, le, les, van, van der, von + NAME = NAME .]
                        // 6(2). NAME + ван дер + NAME = NAME;
                        if ( i < len - 2 )
                        {
                            var word_next = words[ i + 1 ];
                            if ( (word_next.nerOutputType == NerOutputType.O) &&
                                 _SingleWordsBetween2NAME.Contains( word_next.valueOriginal ) 
                               )
                            {
                                var word_next_next = words[ i + 2 ];
                                if ( word_next_next.nerOutputType == NerOutputType.NAME )
                                {
                                    var ss = new SearchResult( i, 3, NerOutputType.NAME );
                                    Merge( ss, words, null );
                                }
                                //"ван дер"
                                if ( (word_next     .valueOriginal == "ван") &&
                                     (word_next_next.nerOutputType == NerOutputType.O) &&
                                     (word_next_next.valueOriginal == "дер") &&
                                     (i < len - 3)
                                   )
                                {
                                    var word_next_next_next = words[ i + 3 ];
                                    if ( word_next_next_next.nerOutputType == NerOutputType.NAME )
                                    {
                                        var ss = new SearchResult( i, 4, NerOutputType.NAME );
                                        Merge( ss, words, null );
                                    }
                                }
                            }
                        }
                        #endregion
                        #endregion
                    }
                    break;

                    case NerOutputType.GEO:
                    {
                        #region [. 9. Сокращения (м.б. прописными и заглавными): г.; д.; н.; наб.; о.; пер.; пл.; пос.; п.; пр.; р.; ст.; с.; ул.; х.; + GEO = GEO .]
                        if ( 0 < i )
                        {
                            var i_prev = i - 1;

                            var word_prev = words[ i_prev ];
                            if ( (!word_prev.IsWordInNerChain) && //---(word_prev.nerOutputType == NerOutputType.O) &&
                                 (word_prev.length <= _MaxLengthSingleWordsBeforeGEO) &&
                                 xlat.IsDot( word_prev.valueOriginal[ word_prev.length - 1 ] ) &&
                                 _SingleWordsBeforeGEO.Contains( word_prev.valueUpper /*word_prev.valueOriginal.ToUpperInvariant()*/ )
                               )
                            {
                                var ss = new SearchResult( i_prev, 2, NerOutputType.GEO );
                                Merge( ss, words, word_prev );
                            }
                        }
                        #endregion
                    }
                    break;

                    case NerOutputType.ORG:
                    {
                        #region [.rules.]
                        var i_prev = (0 < i) ? (i - 1) : -1;

                        #region [. 14. Слово: все буквы заглавные + ORG = ORG .]
                        if ( i_prev != -1 )
                        {
                            var word_prev = words[ i_prev ];
                            if ( (word_prev.nerOutputType == NerOutputType.O) &&
                                 (word_prev.nerInputType == NerInputType.allC) )
                            {
                                var ss = new SearchResult( i_prev, 2, NerOutputType.ORG );
                                Merge( ss, words, word_prev );
                            }
                        }
                        #endregion

                        #region [. 12. Слово с заглавной буквы, но не первое слово в предложении + ORG = ORG .]
                        if ( 1 < i )
                        {
                            var word_prev = words[ i_prev ];
                            switch ( word_prev.nerInputType )
                            {
                                case NerInputType.C:
                                case NerInputType.latC:
                                    if ( word_prev.nerOutputType == NerOutputType.O )
                                    {
                                        var ss = new SearchResult( i_prev, 2, NerOutputType.GEO );
                                        Merge( ss, words, word_prev );
                                    }
                                break;
                            }
                        }
                        #endregion

                        i += w.GetNerChainLength() - 1;

                        #region [. 13. ORG + слово с заглавной буквы = ORG .]
                        if ( i < len - 1 )
                        {
                            var word_next = words[ i + 1 ];
                            switch ( word_next.nerInputType )
                            {
                                case NerInputType.C:
                                case NerInputType.latC:
                                    if ( word_next.nerOutputType == NerOutputType.O )
                                    {
                                        var ss = new SearchResult( i, 2, NerOutputType.ORG );
                                        Merge( ss, words, null );
                                    }
                                break;
                            }
                        }
                        #endregion

                        // 10.	ORG +им.+ NAME  или ORG +имени+ NAME  = ORG ;
                        #region commented
                        /*
                        if ( i < len - 2 )
                        {
                            var word_next = words[ i + 1 ];
                            if ( word_next.nerOutputType == NerOutputType.O )
                            {
                                switch ( word_next.value )
                                {
                                    case "им.":
                                    case "имени":
                                        var word_next_next = words[ i + 2 ];
                                        if ( word_next_next.nerOutputType == NerOutputType.NAME )
                                        {
                                            return (new SearchResult( i, 3, NerOutputType.ORG ));
                                        }
                                    break;
                                }
                            }
                        }
                        */
                        #endregion
                        #endregion
                    }
                    break;
                }
            }
        }
    }
}
