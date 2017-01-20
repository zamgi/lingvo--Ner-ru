using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace ner
{
    /// <summary>
    /// 
    /// </summary>
    public enum NerInputType : byte
    {
        /// Другой
		O, // = "O";

		/// Начинается с заглавной буквы и содержит хотябы одну цифру [МИГ-21]
        numC, // = "numC";,

        /// Хотя бы одна римская цифра буква (без точки) [XVI] [X-XI]
        latNum, // = "latNum";

        /// Одна заглавная буква без точки [F]
		oneC, // ="oneC";

        /// Все заглавные буквы (больше одной) [МТС]
		allC, // ="allC";

        /// все буквы заглавные и все на латинице [POP]
		allatC, // ="allatC";

        /// одна заглавная буква с точкой [F.]
		oneCP, // ="oneCP";

        /// одна или несколько первых буквы заглавные с точкой + маленькие буквы [В.В.Пупкин]
		//---FirstCapitalPoint, // ="fstC";

        /// все заглавные буквы (больше одной) с точкой (точками) [V.I.P.]
		//---AllCapitalPoint, // ="allCP";

        /// 1. Все заглавные буквы (больше одной) подряд с точкой (точками) [V.IV.I.PA]        
        mixCP, // ="mixCP";
		/// 2. Смешенные заглавные и прописные буквы; латиница + кириллица [СевКавГПУ]
        mixC,  // ="mixC";

        /// Только первая заглавная на латинице [Fox]
		latC, // ="latC";

        /// Только первая заглавная на кириллице [Вася]
		C, // ="C";

        /// Первые строчные с точкой, затем заглавная [ул.Васильевская]
		//---LowercasePoint, // ="loP";

		/// кавычки ["«“”»]
		Q, // ="Q";

		/// первая буква строчная; в слове нет точек; обязательно присутствует заглавная буква
		iProd, // ="iProd";

        /// цифры в любой комбинации со знаками препинаний без букв: __NUM__
        NUM, // = "NUM";
        /// запятую и точку с запятой - COMA
        COMA,
        /// двоеточие - COLON
        //---COLON,
        /// открывающие скобки - BRA
        //---BRA,
        /// закрывающие скобки - KET
        //---KET,
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
    public static class NerExtensions
    {
        public static string ToText( this NerInputType  nerInputType  )
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

        internal static NerOutputType ToNerOutputType( this string value )
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
        }
        internal static string ToText( this BuildModelNerInputType buildModelNerInputType )
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
        }

        internal static BuildModelNerInputType ToBuildModelNerInputTypeB( this NerOutputType nerOutputType )
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
        internal static BuildModelNerInputType ToBuildModelNerInputTypeI( this NerOutputType nerOutputType )
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
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class word_t
    {
        public string valueOriginal;
        public int    startIndex;
        public int    length;

        public NerInputType  nerInputType;
        public NerOutputType nerOutputType;

        public override string ToString()
        {
            return ('\'' + valueOriginal + "'  [" + startIndex + ":" + length + "]  " +
                    '\'' + nerInputType.ToString() + "'  " +
                    '\'' + ((nerOutputType == NerOutputType.O) ? "-" : nerOutputType.ToString()) + '\''
                   );
        }

        internal byte Tag;
    }

    /// <summary>
    /// 
    /// </summary>
    internal enum BuildModelNerInputType
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
    internal struct buildmodel_word_t
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

}
