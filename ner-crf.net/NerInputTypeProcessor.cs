using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using lingvo.core;
using lingvo.tokenizing;

namespace lingvo.ner
{
    /// <summary>
    /// 
    /// </summary>
    internal struct NerInputTypeProcessorFactory : INerInputTypeProcessorFactory
    {
        public readonly INerInputTypeProcessor _NerInputTypeProcessor;

        public NerInputTypeProcessorFactory( LanguageTypeEnum languageType )
        {
            switch ( languageType )
            {
                case LanguageTypeEnum.En:
                    _NerInputTypeProcessor = new NerInputTypeProcessor_En();
                break;

                case LanguageTypeEnum.Ru:
                    _NerInputTypeProcessor = new NerInputTypeProcessor_Ru();
                break;

                default:
                    throw (new ArgumentException( languageType.ToString() ));
            }
        }

        public INerInputTypeProcessor CreateInstance()
        {
            return (_NerInputTypeProcessor);
        }
    }


    /// <summary>
    /// Обработчик Графематических характеристик для русского NER'а. Прикрепляет к словам определенные признаки
    /// </summary>
    unsafe public/*internal*/ sealed class NerInputTypeProcessor_Ru : INerInputTypeProcessor
    {
        #region [.description.]
        /*
		allC,   // Все заглавные буквы (больше одной) [МТС]
		allatC, // все буквы заглавные и все на латинице [POP]
		latC,   // Только первая заглавная на латинице [Fox]
        latNum, // Хотя бы одна римская цифра буква (без точки) [XVI] [X-XI]
        mixCP,  // Все заглавные буквы (больше одной) подряд с точкой (точками) [V.IV.I.PA]        
        mixC,   // Смешенные заглавные и прописные буквы; латиница + кириллица [СевКавГПУ]
        oneC,   // Одна заглавная буква без точки [F]
		oneCP,  // одна заглавная буква с точкой [F.]
        numC,   // Начинается с заглавной буквы и содержит хотябы одну цифру [МИГ-21]
		C,      // Только первая заглавная на кириллице [Вася]
        iProd,  // первая буква строчная; в слове нет точек; обязательно присутствует заглавная буква
		Q,      // кавычки ["«“”»]		
        NUM,    // цифры в любой комбинации со знаками препинаний без букв: ["2,4", "10000", "2.456.542", "8:45"]
        COMA,   // запятую и точку с запятой - COMA      
        */
        #endregion

        private readonly CharType* _CTM;

        public NerInputTypeProcessor_Ru()
        {
            _CTM = xlat_Unsafe.Inst._CHARTYPE_MAP;
        }

		/*/// <summary>
        /// Является признаком LowercasePoint?
		/// </summary>
        unsafe private static bool IsLowercasePoint( char* _base, int length )
        {
            var ctm = xlat_Unsafe.Inst._CHARTYPE_MAP;

            for ( int i = 0; i < length; i++ )
            {
                if ( xlat.IsDot( *(_base + i) ) )
                {
                    var nextIndex = i + 1;
                    if ( (nextIndex < length) && ((*(ctm + *(_base + nextIndex)) & CharType.IsUpper) == CharType.IsUpper) )
                    {
                        for ( i = 0; i < length; i++ )
                        {
                            var ch = *(_base + i);
                            var ct = *(ctm + ch);
                            if ( !( (ct & CharType.IsLetter) == CharType.IsLetter &&
                                    (ct & CharType.IsLower ) == CharType.IsLower )
                               )
                            {
                                return (xlat.IsDot( ch ));
                            }
                        }
                    }
                }
            }
            return (false);
        }*/

        /// <summary>
        /// Слово на латыни?
        /// </summary>
        unsafe private static bool IsLatin( char* _base, int length )
        {
            var hasLatinLetter = false;
            for ( int i = 0; i < length; i++ )
            {
                var ch = *(_base + i);

                if ( ('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') )
                {
                    hasLatinLetter = true;
                    continue;
                }

                if ( (*(xlat_Unsafe.Inst._CHARTYPE_MAP + ch) & CharType.IsLetter) == CharType.IsLetter )
                {
                    return (false);
                }
            }

            return (hasLatinLetter);

            /*
            var regex = new Regex("[a-zA-Z-]+");
			var match = regex.Match( word );
			return (match.Value.Length == word.Length);
            */
        }

        /// <summary>
        /// Римская цифра?
        /// </summary>
        private static bool IsRomanSymbol( char ch )
		{
            switch ( ch )
            {
                case 'I':
                case 'V':
                case 'X':
                case 'L':
                case 'C':
                case 'D':
                case 'M':
                    return (true);
            }
			return (false);
		}

        /// <summary>
        /// 
        /// </summary>
        unsafe public NerInputType GetNerInputType( char* _base, int length )
        {
            //-1-
            int digitCount       = 0,
                upperLetterCount = 0,
                hyphenCount      = 0,
                lowerLetterCount = 0,
                dotCount         = 0,
                romanNumberCount = 0;

            //-2-
            #region [.main cycle.]
            for ( int i = 0; i < length; i++ )
            {
                var ch = *(_base + i);
                var ct = *(_CTM  + ch);
                if ( (ct & CharType.IsDigit) == CharType.IsDigit )
                {
                    digitCount++;
                }
                else if ( (ct & CharType.IsLower) == CharType.IsLower )
                {
                    lowerLetterCount++;
                }
                else if ( (ct & CharType.IsUpper) == CharType.IsUpper )
                {
                    upperLetterCount++;
                    if ( IsRomanSymbol( ch ) )
                        romanNumberCount++;
                }
                else if ( (ct & CharType.IsHyphen) == CharType.IsHyphen ) //if ( xlat.IsHyphen( ch ) )
                {
                    hyphenCount++;
                }
                else if ( xlat.IsDot( ch ) )
                {
                    dotCount++;
                }
            }
            #endregion

            var first_ch = *_base;
            var first_ct = *(_CTM + first_ch);
            //-3-
            var isFirstUpper = (1 < length) && ((first_ct & CharType.IsUpper) == CharType.IsUpper);
            if ( (dotCount == 0) && (digitCount != 0) )
            {
                if ( isFirstUpper )
                    return (NerInputType.numC);

                //'3G', '3-GMS', '123/Xyz'
                if ( (1 < length) && (upperLetterCount != 0) )
                {
                    for ( int i = 1; i < length; i++ )
                    {
                        var ch = *(_base + i);
                        var ct = *(_CTM + ch);
                        if ( (ct & CharType.IsUpper) == CharType.IsUpper )
                        {
                            return (NerInputType.numC);
                        }
                        else
                        if ( (ct & CharType.IsLower) == CharType.IsLower )
                        {
                            break;    
                        }
                    }
                }
            }

            if ( (upperLetterCount != 0) && (lowerLetterCount == 0) && (dotCount == 0) )
            {
                if ( length == 1 )
                    return (NerInputType.oneC);

                if ( (romanNumberCount == length) || (romanNumberCount == length - hyphenCount) )
                    return (NerInputType.latNum);

                if ( (digitCount == 0) && (hyphenCount == 0) )
                {
                    if ( IsLatin( _base, length ) )
                        return (NerInputType.allatC);

                    return (NerInputType.allC);
                }
            }

            //(oneCP) - одна заглавная буква с точкой, перед буквой может быть тире: 'F.', 'Й.', '-И.';
            if ( dotCount == 1 )
            {
                switch ( length )
                {
                    case 2:
                        if ( (first_ct & CharType.IsUpper) == CharType.IsUpper )
                        {
                            return (NerInputType.oneCP);
                        }
                        break;

                    case 3:
                        if ( (first_ct & CharType.IsHyphen) == CharType.IsHyphen &&
                             (_CTM[ *(_base + 1) ] & CharType.IsUpper) == CharType.IsUpper
                           )
                        {
                            return (NerInputType.oneCP);
                        }
                        break;
                }
            }

            if ( isFirstUpper && (upperLetterCount == 1) && (1 < length) && !xlat.IsDot( *(_base + 1) ) && (hyphenCount == 0) )
            {
                if ( IsLatin( _base, length ) )
                    return (NerInputType.latC);

                return (NerInputType.C);
            }

            //(Q) - кавычки ["«“”»]
            if ( (first_ct & CharType.IsQuote) == CharType.IsQuote )
            {
                return (NerInputType.Q);
            }

            if ( (2 < length) && ((first_ct & CharType.IsLower) == CharType.IsLower) && (upperLetterCount != 0) && (dotCount == 0) && IsLatin( /*_CTM,*/ _base, length ) )
                return (NerInputType.iProd);

            if ( upperLetterCount != 0 )
            {
                if ( lowerLetterCount != 0 )
                {
                    if ( dotCount <= 1 )
                    {
                        return (NerInputType.mixC);
                    }
                }
                else
                if ( dotCount == 1 )
                {
                    return (NerInputType.mixCP);
                }
            }
            
            if ( (lowerLetterCount == 0) && (upperLetterCount == 0) )
            {
                //(NUM) - цифры в любой комбинации со знаками препинаний без букв: ["2,4", "10000", "2.456.542", "8:45"]
                if ( digitCount != 0 )
                {
                    return (NerInputType.NUM);
                }
                
                switch ( first_ch )
                {
                    //(COMA) - запятую и точку с запятой
                    case ',':
                    case ';':
                        return (NerInputType.COMA);
                }
            }

            return (NerInputType.O);
        }
    }

    /// <summary>
    /// Обработчик Графематических характеристик для английского NER'а. Прикрепляет к словам определенные признаки
    /// </summary>
    unsafe public/*internal*/ sealed class NerInputTypeProcessor_En : INerInputTypeProcessor
    {
        #region [.description.]
        /*
        (allC)  - все заглавные буквы на латинице (больше одной), нет точек, не содержит цифр, между буквами может быть тире : NATO;
        (allCP) - все заглавные буквы (больше одной) с точкой (точками), без тире: U.N.;
        (mixC)  - смешенные буквы: заглавные и строчные, первая буква - заглавная, между буквами может быть тире, точка: St.-Petersburg , FireFox, Google.Maps
        (mixCP) - две и более заглавных подряд  с точкой (точками). Между буквами может быть тире: V.IVI.PA;
        (latC)  - хотя бы одна римская цифра буква (без точки), не содержит арабских цифр, между буквами может быть тире: XXI;
        (oneC)  - одна заглавная буква без точки и не цифра: F;
        (oneCP) - одна заглавная буква с точкой, перед буквой может быть тире: F. ;
        (Z)     - только первая заглавная: Thatcher;
        (numC)  - начинается с заглавной буквы и содержит хотя бы одну цифру, может содержать строчные, между буквами может быть тире: G8;
        (iProd) - первые строчные латиница без точки + заглавная, между буквами может быть тире: iPod.
        (Q)     - кавычки ["«“”»]
        (NUM)   - цифры в любой комбинации со знаками препинаний без букв: ["2,4", "10000", "2.456.542", "8:45"]
        (O)     - other's
        */
        #endregion

        private readonly CharType* _CTM;

        public NerInputTypeProcessor_En()
        {
            _CTM = xlat_Unsafe.Inst._CHARTYPE_MAP;
        }

        /// <summary>
        /// Слово на латыни?
        /// </summary>
        unsafe private static bool IsLatin( char* _base, int length )
        {
            var hasLatinLetter = false;
            for ( int i = 0; i < length; i++ )
            {
                var ch = *(_base + i);

                if ( ('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') )
                {
                    hasLatinLetter = true;
                    continue;
                }

                if ( (*(xlat_Unsafe.Inst._CHARTYPE_MAP + ch) & CharType.IsLetter) == CharType.IsLetter )
                {
                    return (false);
                }
            }

            return (hasLatinLetter);

            /*
            var regex = new Regex("[a-zA-Z-]+");
			var match = regex.Match( word );
			return (match.Value.Length == word.Length);
            */
        }

        /// <summary>
        /// Римская цифра?
        /// </summary>
        private static bool IsRomanSymbol( char ch )
		{
            switch ( ch )
            {
                case 'I':
                case 'V':
                case 'X':
                case 'L':
                case 'C':
                case 'D':
                case 'M':
                    return (true);
            }
			return (false);
		}

        /// <summary>
        /// 
        /// </summary>
        unsafe public NerInputType GetNerInputType( char* _base, int length )
        {
            //-1-
            int digitCount       = 0,
                upperLetterCount = 0,
                hyphenCount      = 0,
                lowerLetterCount = 0,
                dotCount         = 0,
                romanNumberCount = 0;

            //-2-
            #region [.main cycle.]
            for ( int i = 0; i < length; i++ )
            {
                var ch = *(_base + i);
                var ct = *(_CTM  + ch);
                if ( (ct & CharType.IsDigit) == CharType.IsDigit )
                {
                    digitCount++;
                }
                else if ( (ct & CharType.IsLower) == CharType.IsLower )
                {
                    lowerLetterCount++;
                }
                else if ( (ct & CharType.IsUpper) == CharType.IsUpper )
                {
                    upperLetterCount++;
                    if ( IsRomanSymbol( ch ) )
                        romanNumberCount++;
                }
                else if ( (ct & CharType.IsHyphen) == CharType.IsHyphen )
                {
                    hyphenCount++;
                }
                else if ( xlat.IsDot( ch ) )
                {
                    dotCount++;
                }
            }
            #endregion

            var first_ch = *_base;
            var first_ct = *(_CTM + first_ch);
            //-3-
            var isFirstUpper = (1 < length) && ((first_ct & CharType.IsUpper) == CharType.IsUpper);
            if ( (dotCount == 0) && (digitCount != 0) )
            {
                if ( isFirstUpper )
                    return (NerInputType.numC);

                //'3G', '3-GMS', '123/Xyz'
                if ( (1 < length) && (upperLetterCount != 0) )
                {
                    for ( int i = 1; i < length; i++ )
                    {
                        var ch = *(_base + i);
                        var ct = *(_CTM + ch);
                        if ( (ct & CharType.IsUpper) == CharType.IsUpper )
                        {
                            return (NerInputType.numC);
                        }
                        else
                        if ( (ct & CharType.IsLower) == CharType.IsLower )
                        {
                            break;    
                        }
                    }
                }
            }

            if ( upperLetterCount != 0 )
            {
                //(allC), (allCP), (mixCP) - все заглавные буквы на латинице (больше одной)
                if ( (1 < upperLetterCount) )
                {
                    if ( dotCount == 0 )
                    {
                        //(latC) - хотя бы одна римская цифра буква (без точки), не содержит арабских цифр, между буквами может быть тире: XXI;
                        if ( (romanNumberCount == length) || (romanNumberCount + hyphenCount == length) )
                        {
                            return (NerInputType.latC);
                        }

                        //(allC)  - все заглавные буквы на латинице (больше одной), нет точек, не содержит цифр, между буквами может быть тире : NATO;
                        if ( (upperLetterCount == length) || (upperLetterCount + hyphenCount == length) )
                        {
                            return (NerInputType.allC);
                        }
                    }
                    else
                    {
                        //(allCP) - все заглавные буквы (больше одной) с точкой (точками), без тире: U.N.;
                        if ( (upperLetterCount + dotCount == length) && (hyphenCount == 0) )
                        {
                            return (NerInputType.allCP);
                        }

                        //(mixCP) - две и более заглавных подряд  с точкой (точками). Между буквами может быть тире: V.IVI.PA;
                        if ( (upperLetterCount + dotCount == length) || (upperLetterCount + dotCount + hyphenCount == length) )
                        {
                            return (NerInputType.mixCP);
                        }
                    }
                }

                //(latC) - хотя бы одна римская цифра буква (без точки), не содержит арабских цифр, между буквами может быть тире: XXI;
                if ( (dotCount == 0) && ((romanNumberCount == length) || (romanNumberCount + hyphenCount == length)) )
                {
                    return (NerInputType.latC);
                }

                //(oneC) - одна заглавная буква без точки и не цифра: F;
                if ( (upperLetterCount == 1) && (length == 1) )
                {
                    return (NerInputType.oneC);
                }

                //(oneCP) - одна заглавная буква с точкой, перед буквой может быть тире: F. ;
                if ( dotCount == 1 )
                {
                    switch ( length )
                    {
                        case 2:
                            if ( (first_ct & CharType.IsUpper) == CharType.IsUpper )
                            {
                                return (NerInputType.oneCP);
                            }
                        break;

                        case 3:
                            if ( (first_ct             & CharType.IsHyphen) == CharType.IsHyphen &&
                                 (_CTM[ *(_base + 1) ] & CharType.IsUpper ) == CharType.IsUpper 
                               )
                            {
                                return (NerInputType.oneCP);
                            }
                        break;
                    }
                }


                //(mixC), (Z) - начинается с заглавной буквы
                if ( (first_ct & CharType.IsUpper) == CharType.IsUpper )
                {
                    //(Z) - только первая заглавная: Thatcher;
                    if ( (upperLetterCount == 1) && (lowerLetterCount + 1 == length) )
                    {
                        return (NerInputType.Z);
                    }

                    //(mixC) - смешенные буквы: заглавные и строчные, первая буква - заглавная, между буквами может быть тире: St.-Petersburg , FireFox, Google.Maps
                    return (NerInputType.mixC);
                }

                //(iProd) - первые строчные латиница без точки + заглавная, между буквами может быть тире: iPod.
                if ( (first_ct & CharType.IsLower) == CharType.IsLower )
                {
                    if ( (digitCount == 0) && (dotCount == 0) )
                    {
                        return (NerInputType.iProd);
                    }
                }
            }

            //(Q) - кавычки ["«“”»]
            if ( (first_ct & CharType.IsQuote) == CharType.IsQuote )
            {
                return (NerInputType.Q);
            }

            if ( (lowerLetterCount == 0) && (upperLetterCount == 0) )
            {
                //(NUM) - цифры в любой комбинации со знаками препинаний без букв: ["2,4", "10000", "2.456.542", "8:45"]
                if ( digitCount != 0 )
                {
                    return (NerInputType.NUM);
                }
            }

            return (NerInputType.O);
        }
    }
}
