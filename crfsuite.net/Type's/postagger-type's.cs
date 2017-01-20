using System;

namespace lingvo.postagger
{
    /// <summary>
    /// 
    /// </summary>
    public enum PosTaggerInputType : byte
    {
        #region [.common.]
        O, // other's (другой)

        //skip-ignore url's
        //---Url,    // – все url & e-mail;
        Com,    // – запятая;
        Dush,   // – тире;
        Col,    // – двоеточие.        
        OneCP,  // - первая заглавная с точкой;
        FstC,   // - первая заглавная, не содержит пробелов;
        Num,    // – содержит хотя бы одну цифру и не содержит букв;
        CompPh, // - составные (имеющие хотя бы один пробел);
        #endregion

        #region [.russian-language.]
        AllLat, // - только латиница: нет строчных и точек;
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public enum PosTaggerOutputType : byte
    {
        Other = 0, //  = Other

        Article,                // = Article
        Adjective,              // = Adj
        AdjectivePronoun,       // = AdjPron
        Adverb,                 // = Adv
        AdverbialParticiple,    // = AdvPart
        AdverbialPronoun,       // = AdvPron
        AuxiliaryVerb,          // = AuxVerb
        Conjunction,            // = Conj
        Gerund,
        Infinitive,             // = Inf
        Interjection,           // = Intr
        ModalVerb,              
        Noun,                   // = Noun
        Numeral,                // = Num
        PastParticiple,
        Participle,             // = Part
        Particle,               // = Pr
        PossessivePronoun,      // = PosPron
        Predicate,              // = Pred
        Preposition,            // = Prep
        Pronoun,                // = Pron
        Punctuation,            // = Punct
        Verb,                   // = Verb

        #region [.en.]
        /*
        Gerund         (L) – Герундий
        ModalVerb      (K) – модальный глагол
        PastParticiple (B) – Причастие прошедшего времени

        отсутствуют 
        AdverbialParticiple –  Деепричастие
        AdverbialPronoun    –  Местоимённое наречие          
        */
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public enum PosTaggerExtraWordType : byte
    {
        __DEFAULT__,

        Abbreviation,
        Punctuation,
    }

    /// <summary>
    /// 
    /// </summary>
    public static class PosTaggerExtensions
    {
        public static string ToText( this PosTaggerInputType  posTaggerInputType  )
        {
            switch ( posTaggerInputType )    
            {
                case PosTaggerInputType.Num:    return ("Num");    // – содержит хотя бы одну цифру и не содержит букв;
                case PosTaggerInputType.AllLat: return ("AllLat"); // - только латиница: нет строчных и точек;
                case PosTaggerInputType.OneCP:  return ("OneCP");  // - первая заглавная с точкой;
                case PosTaggerInputType.CompPh: return ("CompPh"); // - составные (имеющие хотя бы один пробел);
                case PosTaggerInputType.FstC:   return ("FstC");   // - первая заглавная, не содержит пробелов;
                case PosTaggerInputType.Com:    return ("Com");    // – запятая;
                case PosTaggerInputType.Dush:   return ("Dush");   // – тире;
                case PosTaggerInputType.Col:    return ("Col");    // – двоеточие.	
                //---case PosTaggerInputType.Url:    return ("Url");    // – все url & e-mail;
                // Другой
                default: //case PosTaggerInputType.O:  
                                                return ("O");
            }
        }
        public static string ToText( this PosTaggerOutputType posTaggerOutputType )
        {
            switch ( posTaggerOutputType )    
            {
                case PosTaggerOutputType.Article:             return ("Article");
                case PosTaggerOutputType.Adjective:           return ("Adj");
                case PosTaggerOutputType.AdjectivePronoun:    return ("AdjPron");
                case PosTaggerOutputType.Adverb:              return ("Adv");
                case PosTaggerOutputType.AdverbialParticiple: return ("AdvPart");
                case PosTaggerOutputType.AdverbialPronoun:    return ("AdvPron");
                case PosTaggerOutputType.AuxiliaryVerb:       return ("AuxVerb");
                case PosTaggerOutputType.Conjunction:         return ("Conj");
                case PosTaggerOutputType.Gerund:              return ("Gerund");
                case PosTaggerOutputType.Infinitive:          return ("Inf");
                case PosTaggerOutputType.Interjection:        return ("Intr");
                case PosTaggerOutputType.ModalVerb:           return ("ModalVerb"); 
                case PosTaggerOutputType.Noun:                return ("Noun");
                case PosTaggerOutputType.Numeral:             return ("Num");
                case PosTaggerOutputType.PastParticiple:      return ("PastParticiple");
                case PosTaggerOutputType.Participle:          return ("Part");
                case PosTaggerOutputType.Particle:            return ("Pr");
                case PosTaggerOutputType.PossessivePronoun:   return ("PosPron");
                case PosTaggerOutputType.Predicate:           return ("Pred");
                case PosTaggerOutputType.Preposition:         return ("Prep");
                case PosTaggerOutputType.Pronoun:             return ("Pron");
                case PosTaggerOutputType.Punctuation:         return ("Punct");
                case PosTaggerOutputType.Verb:                return ("Verb");

                default: //case PosTaggerOutputType.Other:  
                                                              return ("Other");
            }
        }

        public static char ToCrfChar( this PosTaggerInputType  posTaggerInputType  )
        {
            return ((char) posTaggerInputType.ToCrfByte());
        }
        public static byte ToCrfByte( this PosTaggerInputType  posTaggerInputType  )
        {
            switch ( posTaggerInputType )    
            {
                case PosTaggerInputType.Num:    return ((byte) 'N');  // – содержит хотя бы одну цифру и не содержит букв;
                case PosTaggerInputType.AllLat: return ((byte) 'L');  // - только латиница: нет строчных и точек;
                case PosTaggerInputType.OneCP:  return ((byte) 'P');  // - первая заглавная с точкой;
                case PosTaggerInputType.CompPh: return ((byte) 'H');  // - составные (имеющие хотя бы один пробел);
                case PosTaggerInputType.FstC:   return ((byte) 'F');  // - первая заглавная, не содержит пробелов;
                case PosTaggerInputType.Com:    return ((byte) 'M');  // – запятая;
                case PosTaggerInputType.Dush:   return ((byte) 'D');  // – тире;
                case PosTaggerInputType.Col:    return ((byte) 'C');  // – двоеточие.	
                //---case PosTaggerInputType.Url:    return ((byte) 'U');    // – все url & e-mail;
                // Другой
                default: //case PosTaggerInputType.O:  
                                                return ((byte) 'O');
            }
        }

        public static char ToCrfChar( this PosTaggerOutputType posTaggerOutputType )
        {
            return ((char) posTaggerOutputType.ToCrfByte());
        }
        public static byte ToCrfByte( this PosTaggerOutputType posTaggerOutputType )
        {
            switch ( posTaggerOutputType )    
            {
                case PosTaggerOutputType.Adjective:           return ((byte) 'J');
                case PosTaggerOutputType.AdjectivePronoun:    return ((byte) 'R');
                case PosTaggerOutputType.Adverb:              return ((byte) 'D');
                case PosTaggerOutputType.AdverbialParticiple: return ((byte) 'X');
                case PosTaggerOutputType.AdverbialPronoun:    return ((byte) 'H');
                case PosTaggerOutputType.Article:             return ((byte) 'A');
                case PosTaggerOutputType.AuxiliaryVerb:       return ((byte) 'G');
                case PosTaggerOutputType.Conjunction:         return ((byte) 'C');
                case PosTaggerOutputType.Gerund:              return ((byte) 'L');
                case PosTaggerOutputType.Infinitive:          return ((byte) 'F');
                case PosTaggerOutputType.Interjection:        return ((byte) 'I');
                case PosTaggerOutputType.ModalVerb:           return ((byte) 'K'); 
                case PosTaggerOutputType.Noun:                return ((byte) 'N');
                case PosTaggerOutputType.Numeral:             return ((byte) 'M');
                case PosTaggerOutputType.PastParticiple:      return ((byte) 'B');
                case PosTaggerOutputType.Participle:          return ((byte) 'Z');
                case PosTaggerOutputType.Particle:            return ((byte) 'W');
                case PosTaggerOutputType.PossessivePronoun:   return ((byte) 'S');
                case PosTaggerOutputType.Preposition:         return ((byte) 'E');
                case PosTaggerOutputType.Pronoun:             return ((byte) 'Y');
                case PosTaggerOutputType.Punctuation:         return ((byte) 'T');
                case PosTaggerOutputType.Verb:                return ((byte) 'V');                

                default: //case PosTaggerOutputType.Other:  
                                                              return ((byte) 'O');
            }
        }

        unsafe public static PosTaggerOutputType ToPosTaggerOutputType( byte* value )
        {
            switch ( *value )
            {
                case (byte) 'J': return (PosTaggerOutputType.Adjective);
                case (byte) 'R': return (PosTaggerOutputType.AdjectivePronoun);
                case (byte) 'D': return (PosTaggerOutputType.Adverb);
                case (byte) 'X': return (PosTaggerOutputType.AdverbialParticiple);
                case (byte) 'H': return (PosTaggerOutputType.AdverbialPronoun);
                case (byte) 'A': return (PosTaggerOutputType.Article);
                case (byte) 'G': return (PosTaggerOutputType.AuxiliaryVerb);
                case (byte) 'C': return (PosTaggerOutputType.Conjunction);
                case (byte) 'L': return (PosTaggerOutputType.Gerund);
                case (byte) 'F': return (PosTaggerOutputType.Infinitive);
                case (byte) 'I': return (PosTaggerOutputType.Interjection);
                case (byte) 'K': return (PosTaggerOutputType.ModalVerb);
                case (byte) 'N': return (PosTaggerOutputType.Noun);
                case (byte) 'M': return (PosTaggerOutputType.Numeral);
                case (byte) 'B': return (PosTaggerOutputType.PastParticiple);
                case (byte) 'Z': return (PosTaggerOutputType.Participle);
                case (byte) 'W': return (PosTaggerOutputType.Particle);
                case (byte) 'S': return (PosTaggerOutputType.PossessivePronoun);
                case (byte) 'E': return (PosTaggerOutputType.Preposition);
                case (byte) 'Y': return (PosTaggerOutputType.Pronoun);
                case (byte) 'V': return (PosTaggerOutputType.Verb);
                case (byte) 'T': return (PosTaggerOutputType.Punctuation);

                default: //((byte) 'O') || '\0' || ...any...
                                 return (PosTaggerOutputType.Other);
            }
        }

        #region [.previous. commented.]
        /*unsafe public static PosTaggerOutputType ToPosTaggerOutputType( byte* value )
        {
            switch ( ((char) *value++) )
            {
                case 'A': //"Article", "Adj", "AdjPron", "Adv", "AdvPart", "AdvPron", "AuxVerb"
                #region
                {
                    var ch = ((char) *value++);
                    switch ( ch )
                    {
                        case 'r': //"Article"
                            ch = ((char) *value++);
                            if ( ch != 't' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'i' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'c' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'l' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'e' ) break;
                            ch = ((char) *value);
                            if ( ch != '\0' ) break;

                            return (PosTaggerOutputType.Article);

                        case 'u': //"AuxVerb"
                            ch = ((char) *value++);
                            if ( ch != 'x' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'V' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'e' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'r' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'b' ) break;
                            ch = ((char) *value);
                            if ( ch != '\0' ) break;

                            return (PosTaggerOutputType.AuxiliaryVerb);

                        case 'd': //"Adj", "AdjPron", "Adv", "AdvPart", "AdvPron"
                            switch ( ((char) *value++) )
                            {
                                case 'j': //"Adj", "AdjPron"
                                    switch ( ((char) *value++) )
                                    {
                                        case 'P': //"AdjPron"
                                            ch = ((char) *value++);
                                            if ( ch != 'r' ) break;
                                            ch = ((char) *value++);
                                            if ( ch != 'o' ) break;
                                            ch = ((char) *value++);
                                            if ( ch != 'n' ) break;
                                            ch = ((char) *value);
                                            if ( ch != '\0' ) break;

                                            return (PosTaggerOutputType.AdjectivePronoun);

                                        case '\0': //"Adj"
                                            return (PosTaggerOutputType.Adjective);
                                    }
                                break;

                                case 'v': //"Adv", "AdvPart", "AdvPron"
                                    switch ( ((char) *value++) )
                                    {
                                        case 'P': //"AdvPart", "AdvPron"
                                            switch ( ((char) *value++) )
                                            {
                                                case 'a': //"AdvPart"
                                                    ch = ((char) *value++);
                                                    if ( ch != 'r' ) break;
                                                    ch = ((char) *value++);
                                                    if ( ch != 't' ) break;
                                                    ch = ((char) *value);
                                                    if ( ch != '\0' ) break;

                                                    return (PosTaggerOutputType.AdverbialParticiple);

                                                case 'r': //"AdvPron"
                                                    ch = ((char) *value++);
                                                    if ( ch != 'o' ) break;
                                                    ch = ((char) *value++);
                                                    if ( ch != 'n' ) break;
                                                    ch = ((char) *value);
                                                    if ( ch != '\0' ) break;

                                                    return (PosTaggerOutputType.AdverbialPronoun);
                                            }
                                        break;

                                        case '\0': //"Adv"
                                            return (PosTaggerOutputType.Adverb);
                                    }
                                break;
                            }
                        break;
                    }
                }
                #endregion
                break;

                case 'C': //"Conj"
                #region
                {
                    var ch = ((char) *value++);
                    if ( ch != 'o' ) break;
                    ch = ((char) *value++);
                    if ( ch != 'n' ) break;
                    ch = ((char) *value++);
                    if ( ch != 'j' ) break;
                    ch = ((char) *value);
                    if ( ch != '\0' ) break;

                    return (PosTaggerOutputType.Conjunction);
                }
                #endregion

                case 'I': //"Inf", "Intr"
                #region
                {
                    var ch = ((char) *value++);
                    if ( ch != 'n' ) break;

                    switch ( ((char) *value++) )
                    {
                        case 'f': //"Inf"
                            ch = ((char) *value);
                            if ( ch != '\0' ) break;

                            return (PosTaggerOutputType.Infinitive);

                        case 't': //"Intr"
                            ch = ((char) *value++);
                            if ( ch != 'r' ) break;
                            ch = ((char) *value);
                            if ( ch != '\0' ) break;

                            return (PosTaggerOutputType.Interjection);
                    }
                }
                #endregion
                break;

                case 'N': //"Noun", "Num"
                #region
                {
                    var ch = ((char) *value++);
                    switch ( ch )
                    {
                        case 'o': //"Noun"
                            ch = ((char) *value++);
                            if ( ch != 'u' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'n' ) break;
                            ch = ((char) *value);
                            if ( ch != '\0' ) break;

                            return (PosTaggerOutputType.Noun);

                        case 'u': //"Num"
                            ch = ((char) *value++);
                            if ( ch != 'm' ) break;
                            ch = ((char) *value);
                            if ( ch != '\0' ) break;

                            return (PosTaggerOutputType.Numeral);
                    }
                }
                #endregion
                break;

                case 'P': //"Part", "Pr", "PosPron", "Pred", "Prep", "Pron", "Punct"
                #region
                {
                    var ch = ((char) *value++);
                    switch ( ch )
                    {
                        case 'a': //"Part"
                            ch = ((char) *value++);
                            if ( ch != 'r' ) break;
                            ch = ((char) *value++);
                            if ( ch != 't' ) break;
                            ch = ((char) *value);
                            if ( ch != '\0' ) break;

                            return (PosTaggerOutputType.Participle);

                        case 'o': //"PosPron"
                            ch = ((char) *value++);
                            if ( ch != 's' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'P' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'r' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'o' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'n' ) break;
                            ch = ((char) *value);
                            if ( ch != '\0' ) break;

                            return (PosTaggerOutputType.PossessivePronoun);

                        case 'u': //"Punct"
                            ch = ((char) *value++);
                            if ( ch != 'n' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'c' ) break;
                            ch = ((char) *value++);
                            if ( ch != 't' ) break;
                            ch = ((char) *value);
                            if ( ch != '\0' ) break;

                            return (PosTaggerOutputType.Punctuation);

                        case 'r': //"Pr", "Pred", "Prep", "Pron"
                            switch ( ((char) *value++) )
                            {
                                case 'e': //"Pred", "Prep"
                                    switch ( ((char) *value++) )
                                    {
                                        case 'd': //"Pred"
                                            ch = ((char) *value);
                                            if ( ch != '\0' ) break;

                                            return (PosTaggerOutputType.Predicate);

                                        case 'p': //"Prep"
                                            ch = ((char) *value);
                                            if ( ch != '\0' ) break;

                                            return (PosTaggerOutputType.Preposition);                                        
                                    }
                                break;

                                case 'o': //"Pron"
                                    ch = ((char) *value++);
                                    if ( ch != 'n' ) break;
                                    ch = ((char) *value);
                                    if ( ch != '\0' ) break;

                                    return (PosTaggerOutputType.Pronoun);

                                case '\0': //"Pr"
                                    return (PosTaggerOutputType.Particle);
                            }
                            break;
                    }
                }
                #endregion
                break;

                case 'V': //"Verb"
                #region
                {
                    var ch = ((char) *value++);
                    if ( ch != 'e' ) break;
                    ch = ((char) *value++);
                    if ( ch != 'r' ) break;
                    ch = ((char) *value++);
                    if ( ch != 'b' ) break;
                    ch = ((char) *value);
                    if ( ch != '\0' ) break;

                    return (PosTaggerOutputType.Verb);
                }
                #endregion
            }

            return (PosTaggerOutputType.Other);
        }
        */
        /*public static PosTaggerOutputType ToPosTaggerOutputType( this string value )
        {
            switch ( value )
            {
                case "Article":      return (PosTaggerOutputType.Article);
                case "Adj":          return (PosTaggerOutputType.Adjective);
                case "AdjPron":      return (PosTaggerOutputType.AdjectivePronoun);
                case "Adv":          return (PosTaggerOutputType.Adverb);
                case "AdvPart":      return (PosTaggerOutputType.AdverbialParticiple);
                case "AdvPron":      return (PosTaggerOutputType.AdverbialPronoun);
                case "AuxVerb":      return (PosTaggerOutputType.AuxiliaryVerb);
                case "Conj":         return (PosTaggerOutputType.Conjunction);
                case "Inf":          return (PosTaggerOutputType.Infinitive);
                case "Intr":         return (PosTaggerOutputType.Interjection);
                case "Noun":         return (PosTaggerOutputType.Noun);
                case "Num":          return (PosTaggerOutputType.Numeral);
                case "Part":         return (PosTaggerOutputType.Participle);
                case "Pr":           return (PosTaggerOutputType.Particle);
                case "PosPron":      return (PosTaggerOutputType.PossessivePronoun);
                case "Pred":         return (PosTaggerOutputType.Predicate);
                case "Prep":         return (PosTaggerOutputType.Preposition);
                case "Pron":         return (PosTaggerOutputType.Pronoun);
                case "Punct":        return (PosTaggerOutputType.Punctuation);
                case "Verb":         return (PosTaggerOutputType.Verb);

                //case "Other":        return (PosTaggerOutputType.Other);
                default: //case "Other": 
                                     //System.Diagnostics.Debug.WriteLine( '\'' + value + '\'' );
                                     return (PosTaggerOutputType.Other);
                                     //throw (new ArgumentException(value));
            }
        }*/
        #endregion
    }
}
