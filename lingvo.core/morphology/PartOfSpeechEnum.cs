using System;

namespace lingvo.morphology
{
	/// <summary>
    /// части речи
	/// </summary>
    [Flags]
	public enum PartOfSpeechEnum : ushort
	{
        Other             = 0x0,

		Noun              = 0x1,
		Adjective         = (1 << 1),
		Pronoun           = (1 << 2),
		Numeral           = (1 << 3),
		Verb              = (1 << 4),
		Adverb            = (1 << 5),
		Conjunction       = (1 << 6),
		Preposition       = (1 << 7),
		Interjection      = (1 << 8),
		Particle          = (1 << 9),
		Article           = (1 << 10),
		Predicate         = (1 << 11),

        //---AdverbialPronoun  = (1 << 12),
        //---AdjectivePronoun  = (1 << 13),
        //---PossessivePronoun = (1 << 14),
	}
}
