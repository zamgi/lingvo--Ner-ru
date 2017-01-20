using System;

using lingvo.tokenizing;

namespace lingvo.ner
{
    /// <summary>
    /// 
    /// </summary>
    public enum NerInputType : byte
    {
        #region [.common.]
        O,      // other's (другой)
        allC,   // Все заглавные буквы (больше одной) [МТС]        
        latC,   // Только первая заглавная на латинице [Fox]
        mixC,   // Смешенные заглавные и прописные буквы; 
                   //русский   : {латиница + кириллица [СевКавГПУ]}, 
                   //английский: {заглавные и строчные, первая буква - заглавная, между буквами может быть тире, точка: St.-Petersburg , FireFox, Google.Maps}
        mixCP,  // Все заглавные буквы (больше одной) подряд с точкой (точками) [V.IV.I.PA]
        numC,   // Начинается с заглавной буквы и содержит хотябы одну цифру [МИГ-21]
        oneC,   // Одна заглавная буква без точки [F]
        oneCP,  // одна заглавная буква с точкой [F.]        
        iProd,  // первая буква строчная; в слове нет точек; обязательно присутствует заглавная буква
        Q,      // кавычки ["«“”»]
        NUM,    // цифры в любой комбинации со знаками препинаний без букв [2,4 ; 10000 ; 2.456.542 ; 8:45]
        #endregion

        #region [.russian-language.]
        allatC, // все буквы заглавные и все на латинице [POP]
        latNum, // Хотя бы одна римская цифра буква (без точки) [XVI] [X-XI]        
        C,      // Только первая заглавная на кириллице [Вася]            
        COMA,   // запятую и точку с запятой - COMA
        #endregion

        #region [.english-language.]
        allCP, // все заглавные буквы (больше одной) с точкой (точками), без тире: [U.N.]
        Z,     // только первая заглавная:  [Thatcher]
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public enum NerOutputType : byte
    {
        O    = 0,
        NAME = 1,
        ORG  = 2,
        GEO  = 3,
        ENTR = 4,
        PROD = 5,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum BuildModelNerInputType : byte
    {
        __UNDEFINED__ = 0,

        O,
        B_NAME, I_NAME,
        B_ORG,  I_ORG,
        B_GEO,  I_GEO,
        B_ENTR, I_ENTR,
        B_PROD, I_PROD,

        __UNKNOWN__
    }

    /// <summary>
    /// 
    /// </summary>
    public struct buildmodel_word_t
    {
        public word_t                 word;
        public BuildModelNerInputType buildModelNerInputType;

        public override string ToString()
        {
            return ('\'' + word.valueOriginal + "'  [" + word.startIndex + ":" + word.length + "]  " +
                    '\'' + word.nerInputType.ToString() + "'  " +
                    '\'' + ((buildModelNerInputType == BuildModelNerInputType.O) ? "-" : buildModelNerInputType.ToString()) + '\''
                   );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class NerExtensions
    {
        public static string ToText( this NerInputType  nerInputType  )
        {
            switch ( nerInputType )    
            {
                case NerInputType.numC:   return ("numC");
                case NerInputType.latNum: return ("latNum");
                case NerInputType.oneC:   return ("oneC");
                case NerInputType.allC:   return ("allC");
                case NerInputType.allatC: return ("allatC");
                case NerInputType.oneCP:  return ("oneCP");
                case NerInputType.mixCP:  return ("mixCP");
                case NerInputType.mixC:   return ("mixC");
                case NerInputType.latC:   return ("latC");
                case NerInputType.C:      return ("C");
                case NerInputType.Q:      return ("Q");
                case NerInputType.iProd:  return ("iProd");
                case NerInputType.NUM:    return ("NUM");
                case NerInputType.COMA:   return ("COMA");
                
                case NerInputType.allCP:  return ("allCP");
                case NerInputType.Z:      return ("C");
                default: //case NerInputType.Other:  
                        return ("O");
            }
        }
        public static string ToText( this NerOutputType nerOutputType )
        {
            switch ( nerOutputType )    
            {
                case NerOutputType.NAME: return "NAME";
                case NerOutputType.ORG : return "ORG";
                case NerOutputType.GEO : return "GEO";
                case NerOutputType.ENTR: return "ENTR";
                case NerOutputType.PROD: return "PROD";

                default: //case NerOutputType.O:  
                         return "O";
            }
        }

        public static char ToCrfChar( this NerInputType nerInputType )
        {
            switch ( nerInputType )    
            {
                case NerInputType.allC:   return ('A');
                case NerInputType.allatC: return ('B');
                case NerInputType.latNum: return ('D');
                case NerInputType.mixCP:  return ('C');
                case NerInputType.numC:   return ('K');
                case NerInputType.oneC:   return ('F');
                case NerInputType.oneCP:  return ('H');
                case NerInputType.mixC:   return ('X');
                case NerInputType.latC:   return ('S');
                case NerInputType.C:      return ('Z');
                case NerInputType.Q:      return ('Q');
                case NerInputType.iProd:  return ('L');
                case NerInputType.NUM:    return ('N');
                case NerInputType.COMA:   return ('Y');
                
                case NerInputType.allCP:  return ('B');
                case NerInputType.Z:      return ('C');
                default: //case NerInputType.Other:  
                         return ('O');
            }
        }
        public static char ToCrfChar( this NerOutputType nerOutputType )
        {
            switch ( nerOutputType )    
            {
                case NerOutputType.NAME: return ('N');
                case NerOutputType.ORG : return ('J');
                case NerOutputType.GEO : return ('G');
                case NerOutputType.ENTR: return ('E');
                case NerOutputType.PROD: return ('P');

                default: //case NerOutputType.O:  
                         return ('O');
            }
        }

        unsafe public static NerOutputType ToNerOutputType( byte* value )
        {
            switch ( ((char) *value++) )
            {
                case 'B': //"B-N", "B-J", "B-G", "B-E", "B-P"
                case 'I': //"I-N", "I-J", "I-G", "I-E", "I-P"
                {
                    var ch = ((char) *value++);
                    if ( ch != '-' ) break;

                    switch ( ((char) *value++) )
                    {
                        case 'N': return (NerOutputType.NAME);
                        case 'J': return (NerOutputType.ORG);
                        case 'G': return (NerOutputType.GEO);
                        case 'E': return (NerOutputType.ENTR);
                        case 'P': return (NerOutputType.PROD);
                    }
                }
                break;
            }

            return (NerOutputType.O);
        }

        public static string ToText( this BuildModelNerInputType buildModelNerInputType )
        {
            switch ( buildModelNerInputType )
            {
                case BuildModelNerInputType.B_NAME: return ("B-N");
                case BuildModelNerInputType.I_NAME: return ("I-N");

                case BuildModelNerInputType.B_ORG:  return ("B-J");  
                case BuildModelNerInputType.I_ORG:  return ("I-J");

                case BuildModelNerInputType.B_GEO:  return ("B-G");  
                case BuildModelNerInputType.I_GEO:  return ("I-G");

                case BuildModelNerInputType.B_ENTR: return ("B-E");
                case BuildModelNerInputType.I_ENTR: return ("I-E");

                case BuildModelNerInputType.B_PROD: return ("B-P");
                case BuildModelNerInputType.I_PROD: return ("I-P");

                default: //BuildModelNerInputType.O: 
                                                    return ("O");
            }
        }
        public static BuildModelNerInputType ToBuildModelNerInputTypeB( this NerOutputType nerOutputType )
        {
            switch ( nerOutputType )
            {
                case NerOutputType.NAME: return (BuildModelNerInputType.B_NAME);
                case NerOutputType.ORG:  return (BuildModelNerInputType.B_ORG);
                case NerOutputType.GEO:  return (BuildModelNerInputType.B_GEO);
                case NerOutputType.ENTR: return (BuildModelNerInputType.B_ENTR);
                case NerOutputType.PROD: return (BuildModelNerInputType.B_PROD);
                default: //case NerOutputType.O: 
                                         return (BuildModelNerInputType.O);
            }
        }
        public static BuildModelNerInputType ToBuildModelNerInputTypeI( this NerOutputType nerOutputType )
        {
            switch ( nerOutputType )
            {
                case NerOutputType.NAME: return (BuildModelNerInputType.I_NAME);
                case NerOutputType.ORG:  return (BuildModelNerInputType.I_ORG);
                case NerOutputType.GEO:  return (BuildModelNerInputType.I_GEO);
                case NerOutputType.ENTR: return (BuildModelNerInputType.I_ENTR);
                case NerOutputType.PROD: return (BuildModelNerInputType.I_PROD);
                default: //case NerOutputType.O: 
                                         return (BuildModelNerInputType.O);
            }
        }        

        #region [.previous. commented.]
        /*public static string ToText( this NerInputType  nerInputType  )
        {
            switch ( nerInputType )    
            {
                /// Начинается с заглавной буквы и содержит хотябы одну цифру [МИГ-21]
                case NerInputType.numC: return "numC";
                /// Хотя бы одна римская цифра буква (без точки) [XVI] [X-XI]
                case NerInputType.latNum: return "latNum";
                /// Одна заглавная буква без точки [F]
                case NerInputType.oneC: return "oneC";
                /// Все заглавные буквы (больше одной) [МТС]
                case NerInputType.allC: return "allC";
                /// все буквы заглавные и все на латинице [POP]
                case NerInputType.allatC: return "allatC";
                /// одна заглавная буква с точкой [F.]
                case NerInputType.oneCP: return "oneCP";
                /// одна или несколько первых буквы заглавные с точкой + маленькие буквы [В.В.Пупкин]
                //---case NerInputType.FirstCapitalPoint: return "fstC";
                /// все заглавные буквы (больше одной) с точкой (точками) [V.I.P.]
                //---case NerInputType.AllCapitalPoint: return "allCP";
                /// 1. Все заглавные буквы (больше одной) подряд с точкой (точками) [V.IV.I.PA]
                case NerInputType.mixCP: return "mixCP";
		        /// 2. Смешенные заглавные и прописные буквы; латиница + кириллица [СевКавГПУ]
                case NerInputType.mixC: return "mixC";
                /// Только первая заглавная на латинице [Fox]
                case NerInputType.latC: return "latC";
                /// Только первая заглавная на кириллице [Вася]
                case NerInputType.C: return "C";
                /// Первые строчные с точкой, затем заглавная [ул.Васильевская]
                //---case NerInputType.LowercasePoint: return "loP";
		        /// кавычки ["«“”»]
                case NerInputType.Q: return "Q";
		        /// первая буква строчная; в слове нет точек; обязательно присутствует заглавная буква
                case NerInputType.iProd: return "iProd";
                /// цифры в любой комбинации со знаками препинаний без букв - NUM
                case NerInputType.NUM: return "NUM";
                /// запятую и точку с запятой - COMA
                case NerInputType.COMA: return "COMA";
                /// двоеточие - COLON
                //---case NerInputType.COLON: return "COLON";
                /// открывающие скобки - BRA
                //---case NerInputType.BRA: return "BRA";
                /// закрывающие скобки - KET
                //---case NerInputType.KET: return "KET";
                /// Другой
                default: //case NerInputType.Other:  
                        return "O";
            }
        }*/
        /*public static string ToText( this NerOutputType nerOutputType )
        {
            switch ( nerOutputType )    
            {
                case NerOutputType.NAME: return "NAME";
                case NerOutputType.ORG : return "ORG";
                case NerOutputType.GEO : return "GEO";
                case NerOutputType.ENTR: return "ENTR";
                case NerOutputType.PROD: return "PROD";

                default: //case NerOutputType.O:  
                    return "O";
            }
        }*/

        /*public static NerOutputType ToNerOutputType( this string value )
        {
            switch ( value )
            {               
                case "B-NAME":
                case "I-NAME": return (NerOutputType.NAME);

                case "B-ORG":  
                case "I-ORG":  return (NerOutputType.ORG);

                case "B-GEO":  
                case "I-GEO":  return (NerOutputType.GEO);

                case "B-ENTR":
                case "I-ENTR": return (NerOutputType.ENTR);

                case "B-PROD":
                case "I-PROD": return (NerOutputType.PROD);

                default: //case "O": 
                               return (NerOutputType.O);
            }
        }*/
        /*unsafe public static NerOutputType ToNerOutputType( byte* value )
        {
            switch ( ((char) *value++) )
            {
                case 'B': //"B-NAME", "B-ORG", "B-GEO", "B-ENTR", "B-PROD"
                case 'I': //"I-NAME", "I-ORG", "I-GEO", "I-ENTR", "I-PROD"
                #region
                {
                    var ch = ((char) *value++);
                    if ( ch != '-' ) break;

                    switch ( ((char) *value++) )
                    {
                        case 'N': //"NAME"
                            ch = ((char) *value++);
                            if ( ch != 'A' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'M' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'E' ) break;
                            ch = ((char) *value);
                            if ( ch != '\0' ) break;

                            return (NerOutputType.NAME);

                        case 'O': //"ORG"
                            ch = ((char) *value++);
                            if ( ch != 'R' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'G' ) break;
                            ch = ((char) *value);
                            if ( ch != '\0' ) break;

                            return (NerOutputType.ORG);

                        case 'G': //"GEO"
                            ch = ((char) *value++);
                            if ( ch != 'E' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'O' ) break;
                            ch = ((char) *value);
                            if ( ch != '\0' ) break;

                            return (NerOutputType.GEO);

                        case 'E': //"ENTR"
                            ch = ((char) *value++);
                            if ( ch != 'N' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'T' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'R' ) break;
                            ch = ((char) *value);
                            if ( ch != '\0' ) break;

                            return (NerOutputType.ENTR);

                        case 'P': //"PROD"
                            ch = ((char) *value++);
                            if ( ch != 'R' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'O' ) break;
                            ch = ((char) *value++);
                            if ( ch != 'D' ) break;
                            ch = ((char) *value);
                            if ( ch != '\0' ) break;

                            return (NerOutputType.PROD);
                    }
                }
                #endregion
                break;
            }

            return (NerOutputType.O);
        }*/

        /*public static string ToText( this BuildModelNerInputType buildModelNerInputType )
        {
            switch ( buildModelNerInputType )
            {
                case BuildModelNerInputType.B_NAME: return ("B-NAME");
                case BuildModelNerInputType.I_NAME: return ("I-NAME");

                case BuildModelNerInputType.B_ORG:  return ("B-ORG");  
                case BuildModelNerInputType.I_ORG:  return ("I-ORG");

                case BuildModelNerInputType.B_GEO:  return ("B-GEO");  
                case BuildModelNerInputType.I_GEO:  return ("I-GEO");

                case BuildModelNerInputType.B_ENTR: return ("B-ENTR");
                case BuildModelNerInputType.I_ENTR: return ("I-ENTR");

                case BuildModelNerInputType.B_PROD: return ("B-PROD");
                case BuildModelNerInputType.I_PROD: return ("I-PROD");

                default: //BuildModelNerInputType.O: 
                                                    return ("O");
            }
        }*/

        /*public static BuildModelNerInputType ToBuildModelNerInputTypeB( this NerOutputType nerOutputType )
        {
            switch ( nerOutputType )
            {
                case NerOutputType.NAME: return (BuildModelNerInputType.B_NAME);
                case NerOutputType.ORG:  return (BuildModelNerInputType.B_ORG);
                case NerOutputType.GEO:  return (BuildModelNerInputType.B_GEO);
                case NerOutputType.ENTR: return (BuildModelNerInputType.B_ENTR);
                case NerOutputType.PROD: return (BuildModelNerInputType.B_PROD);
                default: //case NerOutputType.O: 
                                         return (BuildModelNerInputType.O);
            }
        }*/
        /*public static BuildModelNerInputType ToBuildModelNerInputTypeI( this NerOutputType nerOutputType )
        {
            switch ( nerOutputType )
            {
                case NerOutputType.NAME: return (BuildModelNerInputType.I_NAME);
                case NerOutputType.ORG:  return (BuildModelNerInputType.I_ORG);
                case NerOutputType.GEO:  return (BuildModelNerInputType.I_GEO);
                case NerOutputType.ENTR: return (BuildModelNerInputType.I_ENTR);
                case NerOutputType.PROD: return (BuildModelNerInputType.I_PROD);
                default: //case NerOutputType.O: 
                                         return (BuildModelNerInputType.O);
            }
        }        
        */
        #endregion
    }
}
