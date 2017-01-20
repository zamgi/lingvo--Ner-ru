using System;

namespace lingvo.syntax
{
    #region [.description.]
    /*
    Часть речи - Обозначение 

    AdverbialModifier        - D
    AgreedAttribute          - T
    Apostrophe               - ' //‘
    Apposition               - F
    Article                  - A
    AuxiliaryVerb            - G
    Colon                    - N
    Comma                    - ,
    Complement               - C
    CoordinatingConjunction  - U
    Dash                     - -
    Exclamation_mark         - !
    Dot                      - .
    LeftBoxBracket           - [
    LeftRoundBracket         - (
    NonAgreedAttribute       - R
    Object                   - W
    ObjectIndirect           - H
    Other                    - O
    ParentheticalWord        - P
    Particle                 - Z
    Predicate                - V
    Preposition              - E
    QuestionMark             - ?
    QuotationMark            - Q
    RightBoxBracket          - ]
    RightRoundBracket        - )
    Semicolon                - ;
    Slash                    - L
    Subject                  - S
    SubordinatingConjunction - B
   
    Space                    - M
    */
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public enum SyntaxRoleType : byte
    {
        Other = 0,                //Иная функция 

        AdverbialModifier,        //Обстоятельства указывают место, время, образ действия (или сопутствующие обстоятельства), причину, цель, степень (или меру) действия или состояния, обозначенного глаголом в его личной или неличной формах. Обстоятельства могут также относиться к прилагательным и наречиям.
        AgreedAttribute,          //дочерний элемент существительного, выраженный прилагательным, причастием, местоимением, числительным.
        Apostrophe,               // ' //‘
        Apposition,               //пояснение существительного или дополнения (имя собственное и пр., например, «Вася, наш сантехник, …»).
        Article,                  //Артикль (для русского нет)
        AuxiliaryVerb,            //Вспомогательный глагол 
        Colon,                    // N
        Comma,                    // ,
        Complement,               //слово или группа, которая завершает предикат в предложении (может включать в себя предикат (например, "странно,..."), выраженный, например, кратким прилагательным  "что-то он печален").
        CoordinatingConjunction,  //Союз (сочинит.)
        Dash,                     // -
        ExclamationMark,          // !
        Dot,                      // .
        LeftBoxBracket,           // [
        LeftRoundBracket,         // (
        NonAgreedAttribute,       //несвязанный элемент  
        Object,                   //прямое дополнение 
        ObjectIndirect,           //непрямое дополнение
        ParentheticalWord,        //вводное слово или словосочетание
        Particle,                 //Частица
        Predicate,                //Глагол
        Preposition,              //Предлог
        QuestionMark,             // ?
        QuotationMark,            // Q
        RightBoxBracket,          // ]
        RightRoundBracket,        // )
        Semicolon,                // ;
        Slash,                    // L
        Subject,                  //Существительное
        SubordinatingConjunction, //Союз (подчинит.)
    }

    /// <summary>
    /// 
    /// </summary>
    public static class SyntaxExtensions
    {
        public static string ToText( this SyntaxRoleType syntaxRoleType )
        {
            return (syntaxRoleType.ToString());
        }

        public static char ToCrfChar( this SyntaxRoleType syntaxRoleType )
        {
            switch ( syntaxRoleType )
            {
                case SyntaxRoleType.AdverbialModifier       : return ('D');
                case SyntaxRoleType.AgreedAttribute         : return ('T');
                //case SyntaxRoleType.Apostrophe              : return ('‘');
                case SyntaxRoleType.Apostrophe              : return ('\'');
                case SyntaxRoleType.Apposition              : return ('F');
                case SyntaxRoleType.Article                 : return ('A');
                case SyntaxRoleType.AuxiliaryVerb           : return ('G');
                case SyntaxRoleType.Colon                   : return ('N');
                case SyntaxRoleType.Comma                   : return (',');
                case SyntaxRoleType.Complement              : return ('C');
                case SyntaxRoleType.CoordinatingConjunction : return ('U');
                case SyntaxRoleType.Dash                    : return ('-');
                case SyntaxRoleType.ExclamationMark         : return ('!');
                case SyntaxRoleType.Dot                     : return ('.');
                case SyntaxRoleType.LeftBoxBracket          : return ('[');
                case SyntaxRoleType.LeftRoundBracket        : return ('(');
                case SyntaxRoleType.NonAgreedAttribute      : return ('R');
                case SyntaxRoleType.Object                  : return ('W');
                case SyntaxRoleType.ObjectIndirect          : return ('H');
                case SyntaxRoleType.ParentheticalWord       : return ('P');
                case SyntaxRoleType.Particle                : return ('Z');
                case SyntaxRoleType.Predicate               : return ('V');
                case SyntaxRoleType.Preposition             : return ('E');
                case SyntaxRoleType.QuestionMark            : return ('?');
                case SyntaxRoleType.QuotationMark           : return ('Q');
                case SyntaxRoleType.RightBoxBracket         : return (']');
                case SyntaxRoleType.RightRoundBracket       : return (')');
                case SyntaxRoleType.Semicolon               : return (';');
                case SyntaxRoleType.Slash                   : return ('L');
                case SyntaxRoleType.Subject                 : return ('S');
                case SyntaxRoleType.SubordinatingConjunction: return ('B');

                default: //case SyntaxRoleType.Other:  
                         return ('O');
            }
        }
        public static byte ToCrfByte( this SyntaxRoleType syntaxRoleType )
        {
            return ((byte) syntaxRoleType.ToCrfChar());
        }

        unsafe public static SyntaxRoleType ToSyntaxRoleType( byte* value )
        {
            switch ( *value )
            {
                case (byte) 'D': return (SyntaxRoleType.AdverbialModifier);
                case (byte) 'T': return (SyntaxRoleType.AgreedAttribute);
                //case (byte) '‘': return (SyntaxRoleType.Apostrophe     );
                case (byte) '\'': return (SyntaxRoleType.Apostrophe     );
                case (byte) 'F': return (SyntaxRoleType.Apposition     );
                case (byte) 'A': return (SyntaxRoleType.Article        );
                case (byte) 'G': return (SyntaxRoleType.AuxiliaryVerb  );
                case (byte) 'N': return (SyntaxRoleType.Colon          );
                case (byte) ',': return (SyntaxRoleType.Comma          );
                case (byte) 'C': return (SyntaxRoleType.Complement     );
                case (byte) 'U': return (SyntaxRoleType.CoordinatingConjunction);
                case (byte) '-': return (SyntaxRoleType.Dash           );
                case (byte) '!': return (SyntaxRoleType.ExclamationMark);
                case (byte) '.': return (SyntaxRoleType.Dot            );
                case (byte) '[': return (SyntaxRoleType.LeftBoxBracket );
                case (byte) '(': return (SyntaxRoleType.LeftRoundBracket);
                case (byte) 'R': return (SyntaxRoleType.NonAgreedAttribute);
                case (byte) 'W': return (SyntaxRoleType.Object         );
                case (byte) 'H': return (SyntaxRoleType.ObjectIndirect );
                case (byte) 'P': return (SyntaxRoleType.ParentheticalWord);
                case (byte) 'Z': return (SyntaxRoleType.Particle       );
                case (byte) 'V': return (SyntaxRoleType.Predicate      );
                case (byte) 'E': return (SyntaxRoleType.Preposition    );
                case (byte) '?': return (SyntaxRoleType.QuestionMark   );
                case (byte) 'Q': return (SyntaxRoleType.QuotationMark  );
                case (byte) ']': return (SyntaxRoleType.RightBoxBracket);
                case (byte) ')': return (SyntaxRoleType.RightRoundBracket);
                case (byte) ';': return (SyntaxRoleType.Semicolon      );
                case (byte) 'L': return (SyntaxRoleType.Slash          );
                case (byte) 'S': return (SyntaxRoleType.Subject        );
                case (byte) 'B': return (SyntaxRoleType.SubordinatingConjunction);

                default: //((byte) 'O') || '\0' || ...any...
                                 return (SyntaxRoleType.Other);
            }
        }
    }
}
