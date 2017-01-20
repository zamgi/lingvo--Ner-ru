/* Aho-Corasick text search algorithm for string's implementation 
 * 
 * For more information visit
 *		- http://www.cs.uku.fi/~kilpelai/BSA05/lectures/slides04.pdf
 */
using System;
using System.Collections;
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

    /*/// <summary>
    /// Combining types of chains of entities
    /// </summary>
    internal static class ___CombiningTypesOfChainsOfEntities___
    {
        private static readonly string SPACE = " ";
        private static readonly AhoCorasick       _AhoCorasick;
        private static readonly HashSet< string > _SingleWordsBetween2NAME;
        private static readonly HashSet< string > _SingleWordsBeforeGEO;
        private static readonly int               _MaxLengthSingleWordsBeforeGEO;

        static CombiningTypesOfChainsOfEntities()
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
                ngram = new ngram_t( Enumerable.Repeat< NerOutputType >( NerOutputType.NAME, i ).ToArray(), NerOutputType.NAME );

                ngrams.Add( ngram );
            }

            //7.  Два и более подряд, без знаков препинания ENTR = ENTR;
            //8.  Два и более подряд, без знаков препинания PROD = PROD;
            //15. Два и более подряд, без знаков препинания ORG  = ORG;
            //---(16. Два и более подряд, без знаков препинания GEO  = GEO;)---
            for ( var i = 2; i <= 50; i++ )
            {
                ngram = new ngram_t( Enumerable.Repeat< NerOutputType >( NerOutputType.ENTR, i ).ToArray(), NerOutputType.ENTR );
                ngrams.Add( ngram );

                ngram = new ngram_t( Enumerable.Repeat< NerOutputType >( NerOutputType.PROD, i ).ToArray(), NerOutputType.PROD );
                ngrams.Add( ngram );

                ngram = new ngram_t( Enumerable.Repeat< NerOutputType >( NerOutputType.ORG, i ).ToArray(), NerOutputType.ORG );
                ngrams.Add( ngram );

                //ngram = new ngram_t( Enumerable.Repeat< NerOutputType >( NerOutputType.GEO, i ).ToArray(), NerOutputType.GEO );
                //ngrams.Add( ngram );
            }

            //11. GEO + ORG или ORG + GEO  = ORG;
            ngram = new ngram_t( new[] { NerOutputType.GEO, NerOutputType.ORG }, NerOutputType.ORG );
            ngrams.Add( ngram );
            ngram = new ngram_t( new[] { NerOutputType.ORG, NerOutputType.GEO }, NerOutputType.ORG );
            ngrams.Add( ngram );

            _AhoCorasick = new AhoCorasick( ngrams );

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
            for ( ; ; )
            {
                var _ss = _AhoCorasick.FindAll( words );
                if ( _ss.HasValue )
                {
                    Merge4AhoCorasick( _ss.Value, words );
                    continue;
                }

                _ss = FindByRules( words );
                if ( _ss.HasValue )
                {
                    Merge( _ss.Value, words );
                    continue;
                }

                break;
            }
        }

        private static void Merge4AhoCorasick( SearchResult ss, List< word_t > words )
        {
            if ( ss.NerOutputType == NerOutputType.NAME )
            {
                var w1 = words[ ss.StartIndex ];
                w1.Tag = AhoCorasick.DONT_MERGE_WITH_NAME_ANOTHER;

                switch ( ss.Length )
                {                   
                    case 2:
                    #region
                    {
                        var w2 = words[ ss.StartIndex + 1 ];
                    
                        w1.nerOutputType = ss.NerOutputType;
                        w1.valueOriginal = string.Concat( w1.valueOriginal, SPACE, w2.valueOriginal );
                        w1.length += (w2.startIndex - (w1.startIndex + w1.length)) + w2.length;
                        words.RemoveAt( ss.StartIndex + 1 );
                    }
                    #endregion
                    break;

                    case 3:
                    #region
                    {
                        var w2 = words[ ss.StartIndex + 1 ];
                        var w3 = words[ ss.StartIndex + 2 ];
                    
                        w1.nerOutputType = ss.NerOutputType;
                        w1.valueOriginal = string.Concat( w1.valueOriginal, SPACE, w2.valueOriginal, SPACE, w3.valueOriginal );
                        w1.length += (w3.startIndex - (w1.startIndex + w1.length)) + w3.length;
                        words.RemoveAt( ss.StartIndex + 2 );
                        words.RemoveAt( ss.StartIndex + 1 );
                    }
                    #endregion
                    break;

                    case 4:
                    #region
                    {
                        var w2 = words[ ss.StartIndex + 1 ];
                        var w3 = words[ ss.StartIndex + 2 ];
                        var w4 = words[ ss.StartIndex + 3 ];

                        w3.Tag = AhoCorasick.DONT_MERGE_WITH_NAME_ANOTHER;
                        w3.nerOutputType = ss.NerOutputType;
                        w3.valueOriginal = string.Concat( w3.valueOriginal, SPACE, w4.valueOriginal );
                        w3.length += (w4.startIndex - (w3.startIndex + w3.length)) + w4.length;
                        words.RemoveAt( ss.StartIndex + 3 );

                        w1.nerOutputType = ss.NerOutputType;
                        w1.valueOriginal = string.Concat( w1.valueOriginal, SPACE, w2.valueOriginal );
                        w1.length += (w2.startIndex - (w1.startIndex + w1.length)) + w2.length;
                        words.RemoveAt( ss.StartIndex + 1 );
                    }
                    #endregion
                    break;

                    case 5:
                    #region
                    {
                        var w2 = words[ ss.StartIndex + 1 ];
                        var w3 = words[ ss.StartIndex + 2 ];
                        var w4 = words[ ss.StartIndex + 3 ];
                        var w5 = words[ ss.StartIndex + 4 ];
                    
                        w4.Tag = AhoCorasick.DONT_MERGE_WITH_NAME_ANOTHER;
                        w4.nerOutputType = ss.NerOutputType;
                        w4.valueOriginal = string.Concat( w4.valueOriginal, SPACE, w5.valueOriginal );
                        w4.length += (w5.startIndex - (w4.startIndex + w4.length)) + w5.length;
                        words.RemoveAt( ss.StartIndex + 4 );

                        w1.nerOutputType = ss.NerOutputType;
                        w1.valueOriginal = string.Concat( w1.valueOriginal, SPACE, w2.valueOriginal, SPACE, w3.valueOriginal );
                        w1.length += (w3.startIndex - (w1.startIndex + w1.length)) + w3.length;
                        words.RemoveAt( ss.StartIndex + 2 );
                        words.RemoveAt( ss.StartIndex + 1 );
                    }
                    #endregion
                    break;

                    case 6:
                    #region
                    {
                        var w2 = words[ ss.StartIndex + 1 ];
                        var w3 = words[ ss.StartIndex + 2 ];
                        var w4 = words[ ss.StartIndex + 3 ];
                        var w5 = words[ ss.StartIndex + 4 ];
                        var w6 = words[ ss.StartIndex + 5 ];

                        w4.Tag = AhoCorasick.DONT_MERGE_WITH_NAME_ANOTHER;
                        w4.nerOutputType = ss.NerOutputType;
                        w4.valueOriginal = string.Concat( w4.valueOriginal, SPACE, w5.valueOriginal, SPACE, w6.valueOriginal );
                        w4.length += (w6.startIndex - (w4.startIndex + w4.length)) + w6.length;
                        words.RemoveAt( ss.StartIndex + 5 );
                        words.RemoveAt( ss.StartIndex + 4 );

                        w1.nerOutputType = ss.NerOutputType;
                        w1.valueOriginal = string.Concat( w1.valueOriginal, SPACE, w2.valueOriginal, SPACE, w3.valueOriginal );
                        w1.length += (w3.startIndex - (w1.startIndex + w1.length)) + w3.length;
                        words.RemoveAt( ss.StartIndex + 2 );
                        words.RemoveAt( ss.StartIndex + 1 );
                    }
                    #endregion
                    break;

                    default:
                    #region
                    {
                        //Merge( ss, words );

                        var w = default(word_t);
                        for ( int i = ss.StartIndex + 1, j = ss.Length; 1 < j; j-- )
                        {
                            w = words[ i ];
                            w1.valueOriginal = string.Concat( w1.valueOriginal, SPACE, w.valueOriginal );
                            words.RemoveAt( i );
                        }
                        w1.nerOutputType = ss.NerOutputType;
                        w1.length += (w.startIndex - (w1.startIndex + w1.length)) + w.length;
                    }
                    #endregion
                    break;
                }
            }
            else
            {
                Merge( ss, words );
            }
        }
        private static void Merge( SearchResult ss, List< word_t > words )
        {
            var w1 = words[ ss.StartIndex ];

            switch ( ss.Length )
            {
                case 2:
                #region
                {
                    var w2 = words[ ss.StartIndex + 1 ];
                    
                    w1.nerOutputType = ss.NerOutputType;
                    w1.valueOriginal = string.Concat( w1.valueOriginal, SPACE, w2.valueOriginal );
                    w1.length += (w2.startIndex - (w1.startIndex + w1.length)) + w2.length;
                    words.RemoveAt( ss.StartIndex + 1 );
                }
                #endregion
                break;

                case 3:
                #region
                {
                    var w2 = words[ ss.StartIndex + 1 ];
                    var w3 = words[ ss.StartIndex + 2 ];
                    
                    w1.nerOutputType = ss.NerOutputType;
                    w1.valueOriginal = string.Concat( w1.valueOriginal, SPACE, w2.valueOriginal, SPACE, w3.valueOriginal );
                    w1.length += (w3.startIndex - (w1.startIndex + w1.length)) + w3.length;
                    words.RemoveAt( ss.StartIndex + 2 );
                    words.RemoveAt( ss.StartIndex + 1 );
                }
                #endregion
                break;

                case 4:
                #region
                {
                    var w2 = words[ ss.StartIndex + 1 ];
                    var w3 = words[ ss.StartIndex + 2 ];
                    var w4 = words[ ss.StartIndex + 3 ];
                    
                    w1.nerOutputType = ss.NerOutputType;
                    w1.valueOriginal = string.Concat( w1.valueOriginal, SPACE, w2.valueOriginal, SPACE, w3.valueOriginal, SPACE, w4.valueOriginal );
                    w1.length += (w4.startIndex - (w1.startIndex + w1.length)) + w4.length;
                    words.RemoveAt( ss.StartIndex + 3 );
                    words.RemoveAt( ss.StartIndex + 2 );
                    words.RemoveAt( ss.StartIndex + 1 );
                }
                #endregion
                break;

                default:
                #region
                {
                    var w = default(word_t);
                    for ( int i = ss.StartIndex + 1, j = ss.Length; 1 < j; j-- )
                    {
                        w = words[ i ];
                        w1.valueOriginal = string.Concat( w1.valueOriginal, SPACE, w.valueOriginal );
                        words.RemoveAt( i );
                    }
                    w1.nerOutputType = ss.NerOutputType;
                    w1.length += (w.startIndex - (w1.startIndex + w1.length)) + w.length;
                }
                #endregion
                break;
            }
        }

        private static SearchResult? FindByRules( List< word_t > words )
        {
            var _ss = Rules4NAME( words );
            if ( _ss.HasValue ) return (_ss);

            _ss = Rules4GEO( words );
            if ( _ss.HasValue ) return (_ss);

            _ss = Rules4ORG( words );
            return (_ss);
        }

        /// <summary>
        /// 5.	Инициалы + NAME; (инициалы: слово с заглавной буквы с точкой на конце, не более трех букв, стоящие перед NAME) = NAME;
        /// 6.	NAME + бен, ван, ван дер, да, дель, дер, де, ди, дю, ла, ле, фон, эль, ben, da, de, del, der, des, du, el, la, le, les, van, van der, von + NAME = NAME;
        /// </summary>
        private static SearchResult? Rules4NAME( List< word_t > words )
        {
            var word_name = default(word_t);
            int i = 0, len = words.Count;
            for ( ; i < len; i++ )
            {
                var w = words[ i ];
                if ( w.nerOutputType == NerOutputType.NAME )
                {
                    word_name = w;
                    break;
                }
            }

            if ( word_name != null )
            {
                // 5.	Инициалы + NAME; (инициалы: слово с заглавной буквы с точкой на конце, не более трех букв, стоящие перед NAME) = NAME;
                if ( 0 < i )
                {
                    var i_prev = i - 1;
                    var word_prev = words[ i_prev ];
                    if ( (word_prev.nerOutputType == NerOutputType.O) &&
                         (word_prev.length <= 3) && 
                         xlat.IsUpper( word_prev.valueOriginal[ 0 ] ) &&
                         xlat.IsDot( word_prev.valueOriginal[ word_prev.length - 1 ] ) 
                       )
                    {
                        return (new SearchResult( i_prev, 2, NerOutputType.NAME ));
                    }
                }

                // 6.	 NAME + бен, ван, да, дель, дер, де, ди, дю, ла, ле, фон, эль, ben, da, de, del, der, des, du, el, la, le, les, van, van der, von + NAME = NAME;
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
                            return (new SearchResult( i, 3, NerOutputType.NAME ));
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
                                return (new SearchResult( i, 4, NerOutputType.NAME ));
                            }
                        }
                    }
                }
            }
            return (null);
        }
        /// <summary>
        /// 9.	Сокращения (м.б. прописными и заглавными): г.; д.; н.; наб.; о.; пер.; пл.; пос.; п.; пр.; р.; ст.; с.; ул.; х.; + GEO = GEO;
        /// </summary>
        private static SearchResult? Rules4GEO( List< word_t > words )
        {
            var word_geo = default(word_t);
            int i = 0, len = words.Count;
            for ( ; i < len; i++ )
            {
                var w = words[ i ];
                if ( w.nerOutputType == NerOutputType.GEO )
                {
                    word_geo = w;
                    break;
                }
            }

            if ( word_geo != null )
            {
                // 9.	Сокращения (м.б. прописными и заглавными): г.; д.; н.; наб.; о.; пер.; пл.; пос.; п.; пр.; р.; ст.; с.; ул.; х.; + GEO = GEO;
                if ( 0 < i )
                {
                    var i_prev = i - 1;
                    var word_prev = words[ i_prev ];
                    if ( (word_prev.nerOutputType == NerOutputType.O) &&
                         (word_prev.length <= _MaxLengthSingleWordsBeforeGEO) &&
                         xlat.IsDot( word_prev.valueOriginal[ word_prev.length - 1 ] ) && 
                        _SingleWordsBeforeGEO.Contains( word_prev.valueOriginal.ToUpperInvariant() )
                       )
                    {
                        return (new SearchResult( i_prev, 2, NerOutputType.GEO ));
                    }
                }
            }
            return (null);
        }
        /// <summary>
        /// 10.	ORG +им.+ NAME  или ORG +имени+ NAME  = ORG ;
        /// 12.	Слово с заглавной буквы, но не первое слово в предложении + ORG = ORG;
        /// 13.	ORG + слово с заглавной буквы = ORG;
        /// 14.	Слово: все буквы заглавные + ORG = ORG;
        /// </summary>
        private static SearchResult? Rules4ORG( List< word_t > words )
        {
            var word_org = default(word_t);
            int i = 0, len = words.Count;
            for ( ; i < len; i++ )
            {
                var w = words[ i ];
                if ( w.nerOutputType == NerOutputType.ORG )
                {
                    word_org = w;
                    break;
                }
            }

            if ( word_org != null )
            {
                // 14.	Слово: все буквы заглавные + ORG = ORG;
                if ( 0 < i )
                {
                    var i_prev = i - 1;
                    var word_prev = words[ i_prev ];
                    if ( (word_prev.nerOutputType == NerOutputType.O) &&
                         (word_prev.nerInputType == NerInputType.allC) )
                    {
                        return (new SearchResult( i_prev, 2, NerOutputType.ORG ));
                    }
                }

                // 12.	Слово с заглавной буквы, но не первое слово в предложении + ORG = ORG;
                if ( 1 < i )
                {
                    var i_prev = i - 1;
                    var word_prev = words[ i_prev ];
                    switch ( word_prev.nerInputType )
                    {
                        case NerInputType.C:
                        case NerInputType.latC:
                            if ( word_prev.nerOutputType == NerOutputType.O )
                            {
                                return (new SearchResult( i_prev, 2, NerOutputType.GEO ));
                            }
                        break;
                    }
                }

                // 13.	ORG + слово с заглавной буквы = ORG;
                if ( i < len - 1 )
                {
                    var word_next = words[ i + 1 ];
                    switch ( word_next.nerInputType )
                    {
                        case NerInputType.C:
                        case NerInputType.latC:
                            if ( word_next.nerOutputType == NerOutputType.O )
                            {
                                return (new SearchResult( i, 2, NerOutputType.ORG ));
                            }
                        break;
                    }
                }

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
                //* /
                #endregion
            }
            return (null);
        }
    }*/

    /// <summary>
    /// Combining types of chains of entities
    /// </summary>
    public static class NerPostMerging
    {
        private static readonly AhoCorasick       _AhoCorasick;
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
                ngram = new ngram_t( Enumerable.Repeat< NerOutputType >( NerOutputType.NAME, i ).ToArray(), NerOutputType.NAME );

                ngrams.Add( ngram );
            }

            //7.  Два и более подряд, без знаков препинания ENTR = ENTR;
            //8.  Два и более подряд, без знаков препинания PROD = PROD;
            //15. Два и более подряд, без знаков препинания ORG  = ORG;
            //---(16. Два и более подряд, без знаков препинания GEO  = GEO;)---
            for ( var i = 2; i <= 50; i++ )
            {
                ngram = new ngram_t( Enumerable.Repeat< NerOutputType >( NerOutputType.ENTR, i ).ToArray(), NerOutputType.ENTR );
                ngrams.Add( ngram );

                ngram = new ngram_t( Enumerable.Repeat< NerOutputType >( NerOutputType.PROD, i ).ToArray(), NerOutputType.PROD );
                ngrams.Add( ngram );

                ngram = new ngram_t( Enumerable.Repeat< NerOutputType >( NerOutputType.ORG, i ).ToArray(), NerOutputType.ORG );
                ngrams.Add( ngram );

                //ngram = new ngram_t( Enumerable.Repeat< NerOutputType >( NerOutputType.GEO, i ).ToArray(), NerOutputType.GEO );
                //ngrams.Add( ngram );
            }

            //11. GEO + ORG или ORG + GEO  = ORG;
            ngram = new ngram_t( new[] { NerOutputType.GEO, NerOutputType.ORG }, NerOutputType.ORG );
            ngrams.Add( ngram );
            ngram = new ngram_t( new[] { NerOutputType.ORG, NerOutputType.GEO }, NerOutputType.ORG );
            ngrams.Add( ngram );

            _AhoCorasick = new AhoCorasick( ngrams );

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
            var sr = _AhoCorasick.FindAll( words );
            if ( sr != null )
            {
                Merge4AhoCorasick( sr, words );
            }

            OtherRules( words );

            /*
            for ( ; ; )
            {
                var sr = _AhoCorasick.FindAll( words );
                if ( sr != null )
                {
                    Merge4AhoCorasick( sr, words );
                }

                var r = FindByRules( words );
                if ( !r )
                {
                    break;
                }
            }
            */
        }

        private static void Merge4AhoCorasick( ICollection< SearchResult > sr, List< word_t > words )
        {
            foreach ( var ss in sr )
            {
                var w1 = words[ ss.StartIndex ];
                if ( w1.IsWordInNerChain )
                    continue;

                if ( ss.NerOutputType == NerOutputType.NAME )
                {
                    switch ( ss.Length )
                    {
                        case 2:
                        #region
                        {
                            var w2 = words[ ss.StartIndex + 1 ];
                                            
                            w1.SetNextPrev( w2, ss.NerOutputType );
                        }
                        #endregion
                        break;

                        case 3:
                        #region
                        {
                            var w2 = words[ ss.StartIndex + 1 ];
                            var w3 = words[ ss.StartIndex + 2 ];
                                            
                            w1.SetNextPrev( w2, ss.NerOutputType );
                            w2.SetNextPrev( w3, ss.NerOutputType );
                        }
                        #endregion
                        break;

                        case 4:
                        #region
                        {
                            var w2 = words[ ss.StartIndex + 1 ];
                            var w3 = words[ ss.StartIndex + 2 ];
                            var w4 = words[ ss.StartIndex + 3 ];
                        
                            w3.SetNextPrev( w4, ss.NerOutputType );

                            w1.SetNextPrev( w2, ss.NerOutputType );
                        }
                        #endregion
                        break;

                        case 5:
                        #region
                        {
                            var w2 = words[ ss.StartIndex + 1 ];
                            var w3 = words[ ss.StartIndex + 2 ];
                            var w4 = words[ ss.StartIndex + 3 ];
                            var w5 = words[ ss.StartIndex + 4 ];

                            w4.SetNextPrev( w5, ss.NerOutputType );

                            w1.SetNextPrev( w2, ss.NerOutputType );
                            w2.SetNextPrev( w3, ss.NerOutputType );
                        }
                        #endregion
                        break;

                        case 6:
                        #region
                        {
                            var w2 = words[ ss.StartIndex + 1 ];
                            var w3 = words[ ss.StartIndex + 2 ];
                            var w4 = words[ ss.StartIndex + 3 ];
                            var w5 = words[ ss.StartIndex + 4 ];
                            var w6 = words[ ss.StartIndex + 5 ];
                        
                            w4.SetNextPrev( w5, ss.NerOutputType );
                            w5.SetNextPrev( w6, ss.NerOutputType );

                            w1.SetNextPrev( w2, ss.NerOutputType );
                            w2.SetNextPrev( w3, ss.NerOutputType );
                        }
                        #endregion
                        break;

                        default:
                        #region
                        {
                            /*Merge( ss, words, w1 );*/
                            for ( int i = ss.StartIndex + 1, len = i + ss.Length - 1; i < len; i++ )
                            {
                                var w = words[ i ];
                                w1.SetNextPrev( w, ss.NerOutputType );
                                w1 = w;
                            }
                        }
                        #endregion
                        break;
                    }
                }
                else
                {
                    Merge( ss, words, w1 );
                }
            }
        }
        private static void Merge( SearchResult ss, List< word_t > words, word_t _w1 )
        {
            var w1 = _w1 ?? words[ ss.StartIndex ];

            switch ( ss.Length )
            {
                case 2:
                #region
                {
                    var w2 = words[ ss.StartIndex + 1 ];
                                        
                    w1.SetNextPrev( w2, ss.NerOutputType );
                }
                #endregion
                break;

                case 3:
                #region
                {
                    var w2 = words[ ss.StartIndex + 1 ];
                    var w3 = words[ ss.StartIndex + 2 ];

                    w1.SetNextPrev( w2, ss.NerOutputType );
                    w2.SetNextPrev( w3, ss.NerOutputType );
                }
                #endregion
                break;

                case 4:
                #region
                {
                    var w2 = words[ ss.StartIndex + 1 ];
                    var w3 = words[ ss.StartIndex + 2 ];
                    var w4 = words[ ss.StartIndex + 3 ];

                    w1.SetNextPrev( w2, ss.NerOutputType );
                    w2.SetNextPrev( w3, ss.NerOutputType );
                    w3.SetNextPrev( w4, ss.NerOutputType );
                }
                #endregion
                break;

                case 5:
                #region
                {
                    var w2 = words[ ss.StartIndex + 1 ];
                    var w3 = words[ ss.StartIndex + 2 ];
                    var w4 = words[ ss.StartIndex + 3 ];
                    var w5 = words[ ss.StartIndex + 4 ];

                    w1.SetNextPrev( w2, ss.NerOutputType );
                    w2.SetNextPrev( w3, ss.NerOutputType );
                    w3.SetNextPrev( w4, ss.NerOutputType );
                    w4.SetNextPrev( w5, ss.NerOutputType );
                }
                #endregion
                break;

                default:
                #region
                {
                    for ( int i = ss.StartIndex + 1, len = i + ss.Length - 1; i < len; i++ )
                    {
                        var w = words[ i ];
                        w1.SetNextPrev( w, ss.NerOutputType );
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



        private static bool FindByRules( List< word_t > words )
        {
            var _ss = Rules4NAME( words );
            if ( _ss.HasValue )
            {
                Merge( _ss.Value, words, null );
                return (true);
            }

            _ss = Rules4GEO( words );
            if ( _ss.HasValue )
            {
                Merge( _ss.Value, words, null );
                return (true);
            }

            _ss = Rules4ORG( words );
            if ( _ss.HasValue )
            {
                Merge( _ss.Value, words, null );
                return (true);
            }

            return (false);
        }
        /// <summary>
        /// 5.	Инициалы + NAME; (инициалы: слово с заглавной буквы с точкой на конце, не более трех букв, стоящие перед NAME) = NAME;
        /// 6.	NAME + бен, ван, ван дер, да, дель, дер, де, ди, дю, ла, ле, фон, эль, ben, da, de, del, der, des, du, el, la, le, les, van, van der, von + NAME = NAME;
        /// </summary>
        private static SearchResult? Rules4NAME( List< word_t > words )
        {
            for ( int i = 0, len = words.Count; i < len; i++ )
            {
                var w = words[ i ];
                if ( w.nerOutputType == NerOutputType.NAME )
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
                            return (new SearchResult( i_prev, 2, NerOutputType.NAME ));
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
                                return (new SearchResult( i, 3, NerOutputType.NAME ));
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
                                    return (new SearchResult( i, 4, NerOutputType.NAME ));
                                }
                            }
                        }
                    }
                    #endregion
                    #endregion
                }
            }

            return (null);
        }
        /// <summary>
        /// 9.	Сокращения (м.б. прописными и заглавными): г.; д.; н.; наб.; о.; пер.; пл.; пос.; п.; пр.; р.; ст.; с.; ул.; х.; + GEO = GEO;
        /// </summary>
        private static SearchResult? Rules4GEO( List< word_t > words )
        {
            for ( int i = 1, len = words.Count; i < len; i++ )
            {
                var w = words[ i ];
                if ( w.nerOutputType == NerOutputType.GEO )
                {
                    #region [. 9. Сокращения (м.б. прописными и заглавными): г.; д.; н.; наб.; о.; пер.; пл.; пос.; п.; пр.; р.; ст.; с.; ул.; х.; + GEO = GEO .]
                    var i_prev = i - 1;

                    var word_prev = words[ i_prev ];
                    if ( (!word_prev.IsWordInNerChain) && //---(word_prev.nerOutputType == NerOutputType.O) &&
                         (word_prev.length <= _MaxLengthSingleWordsBeforeGEO) &&
                         xlat.IsDot( word_prev.valueOriginal[ word_prev.length - 1 ] ) &&
                         _SingleWordsBeforeGEO.Contains( word_prev.valueUpper /*word_prev.valueOriginal.ToUpperInvariant()*/ )
                       )
                    {
                        return (new SearchResult( i_prev, 2, NerOutputType.GEO ));
                    }
                    #endregion
                }
            }
            
            return (null);
        }
        /// <summary>
        /// 10.	ORG +им.+ NAME  или ORG +имени+ NAME  = ORG ;
        /// 12.	Слово с заглавной буквы, но не первое слово в предложении + ORG = ORG;
        /// 13.	ORG + слово с заглавной буквы = ORG;
        /// 14.	Слово: все буквы заглавные + ORG = ORG;
        /// </summary>
        private static SearchResult? Rules4ORG( List< word_t > words )
        {
            for ( int i = 0, len = words.Count; i < len; i++ )
            {
                var w = words[ i ];
                if ( w.nerOutputType == NerOutputType.ORG )
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
                            return (new SearchResult( i_prev, 2, NerOutputType.ORG ));
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
                                    return (new SearchResult( i_prev, 2, NerOutputType.GEO ));
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
                                    return (new SearchResult( i, 2, NerOutputType.ORG ));
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
            }

            return (null);
        }
    }
}
