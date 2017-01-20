using System;

namespace lingvo.morphology
{
    /// <summary>
    /// морфологическое совойство
    /// </summary>
    [Flags]
    public enum MorphoAttributeEnum : ulong
    {
        /// по умолчанию
        __UNDEFINED__ = 0x0UL,


        /// Person - первое
        First  = 0x1UL,
        /// Person - второе	
        Second = (1UL << 1),
        /// Person - третье
        Third  = (1UL << 2),


        /// Case - именительный
        Nominative    = (1UL << 3),
        /// Case - родительный
        Genitive      = (1UL << 4),
        /// Case - дательный
        Dative        = (1UL << 5),
        /// Case - винительный
        Accusative    = (1UL << 6),
        /// Case - творительный
        Instrumental  = (1UL << 7),
        /// Case - предложный
        Prepositional = (1UL << 8),
        /// Case - местный
        Locative      = (1UL << 9),
        /// Case - любой
        Anycase       = (1UL << 10),


        /// Number - единственное
        Singular = (1UL << 11),
        /// Number - множественное
        Plural   = (1UL << 12),


        /// Gender - женский
        Feminine  = (1UL << 13),
        /// Gender - мужской
        Masculine = (1UL << 14),
        /// Gender - средний
        Neuter    = (1UL << 15),
        /// Gender - общий
        General   = (1UL << 16),


        /// Animateness - одушевленный
        Animate   = (1UL << 17),
        /// Animateness - неодушевленный
        Inanimate = (1UL << 18),


        /// NounType - имя собственное
        Proper    = (1UL << 19),
        /// NounType - имя нарицательное
        Common    = (1UL << 20),


        /// Tense - будущее
        Future          = (1UL << 21),
        /// Tense - прошедшее
        Past            = (1UL << 22),
        /// Tense - настоящее
        Present         = (1UL << 23),
        /// Tense - будущее в прошедшем
        FutureInThePast = (1UL << 24),


        /// Mood - повелительное
        Imperative = (1UL << 25),
        /// Mood - изъявительное
        Indicative = (1UL << 26),
        /// Mood - сослагательное
        Subjunctive = (1UL << 27),
        /// Mood - личный глагол
        Personal    = (1UL << 28),
        /// Mood - безличный глагол
        Impersonal  = (1UL << 29),
        /// Mood - деепричастие
        Gerund      = (1UL << 30),
        /// Mood - причастие
        Participle  = (1UL << 31),


        /// Voice - действительный
        Active  = (1UL << 32),
        /// Voice - страдательный
        Passive = (1UL << 33),

        /// VerbTransitivity - (Verb, Predicative, Infinitive, AdverbialParticiple, AuxiliaryVerb, Participle)
        /// VerbTransitivity - переходный
        Transitive   = (1UL << 34),
        /// VerbTransitivity - непереходный
        Intransitive = (1UL << 35),


        /// VerbForm - несовершенная
        Imperfective = (1UL << 36),
        /// VerbForm - совершенная
        Perfective   = (1UL << 37),
        /// VerbForm - совершенная и несовершенная
        PerfImPerf   = (1UL << 38),


        /// NumeralType - порядковое
        Ordinal    = (1UL << 39),
        /// NumeralType - количественное
        Cardinal   = (1UL << 40),
        /// NumeralType - собирательное
        Collective = (1UL << 41),


        /// Form - краткая
        Short = (1UL << 42),


        /// DegreeOfComparison - сравнительная
        Comparative = (1UL << 43),
        /// DegreeOfComparison - превосходная
        Superlative = (1UL << 44),


        /// ConjunctionType - сочинительный
        Subordinating = (1UL << 45),
        /// ConjunctionType - подчинительный
        Coordinating  = (1UL << 46),


        /// PronounType - вопросительное
        Interrogative         = (1UL << 47),
        /// PronounType - относительное
        Relative              = (1UL << 48),
        /// PronounType - относительное и вопросительное
        InterrogativeRelative = (1UL << 49),
        /// PronounType - отрицательное
        Negative              = (1UL << 50),
        /// PronounType - возвратное
        Reflexive             = (1UL << 51),
        /// PronounType - неопределенное 1
        Indefinitive1         = (1UL << 52),
        /// PronounType - неопределенное 2
        Indefinitive2         = (1UL << 53),
        /// PronounType - указательное
        //Indicative,
        /// PronounType - притяжательное
        Possessive            = (1UL << 54),
        /// PronounType - личное
        //Personal,


        /// Article - определенный
        Definite              = (1UL << 55),
        /// Article - неопределенный
        Indefinite            = (1UL << 56),


        /// VerbType - инфинитив
        Infinitive            = (1UL << 57),
        /// VerbType - деепричастие
        AdverbialParticiple   = (1UL << 58),
        /// VerbType - вспомогательный глагол
        AuxiliaryVerb         = (1UL << 59),
        /// VerbType - причастие
        //Participle
    }
}
