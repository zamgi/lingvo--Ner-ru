using System;
using System.Text;

namespace lingvo.tokenizing
{
    using lingvo.ner;
    using lingvo.postagger;
    using lingvo.syntax;
    using lingvo.morphology;

    /// <summary>
    /// 
    /// </summary>
    public sealed class word_t
    {
        public string valueOriginal;
        public string valueUpper;
        public int    startIndex;
        public int    length;

        #region [.pos-tagger.]
        public PosTaggerInputType  posTaggerInputType;
        public PosTaggerOutputType posTaggerOutputType;

        public PosTaggerExtraWordType posTaggerExtraWordType;

        //последнее слово (без цифр, только слово буквами) в цепочке 'Numeral'. по нему будет делаться морфоанализ.
        public string posTaggerLastValueUpperInNumeralChain;

        //первая буква в оригинальном слове (valueOriginal) - большая.
        public bool posTaggerFirstCharIsUpper;
        #endregion

        #region [.ner.]
        public NerInputType  nerInputType;
        public NerOutputType nerOutputType;

        //next ner-word in chain
        public word_t nerNext
        {
            get;
            private set;
        }
        //previous ner-word in chain
        public word_t nerPrev
        {
            get;
            private set;
        }

        public void   SetNextPrev( word_t next, NerOutputType nerOutputType )
        {
            nerNext = next;
            next.nerPrev = this;

            //System.Diagnostics.Debug.Assert( next != this );

            this.nerOutputType = next.nerOutputType = nerOutputType;
        }
        public bool   IsFirstWordInNerChain
        {
            get { return (nerNext != null && nerPrev == null); }
        }
        public bool   IsWordInNerChain
        {
            get { return (nerNext != null || nerPrev != null); }
        }
        public bool   HasNerPrevWord
        {
            get { return (nerPrev != null); }
        }
        public string GetNerValue()
        {
            return (GetNerValue( new StringBuilder() ));
        }
        public string GetNerValue( StringBuilder sb )
        {
            if ( nerNext != null )
            {
                sb.Clear();
                for ( var w = this; w != null; w = w.nerNext )
                {
                    sb.Append( w.valueOriginal ).Append( ' ' );
                }
                return (sb.Remove( sb.Length - 1, 1 ).ToString());
            }
            return (valueOriginal);
        }
        public int    GetNerLength()
        {
            if ( nerNext != null )
            {
                for ( var w = this; ; w = w.nerNext )
                {
                    if ( w.nerNext == null )
                    {
                        var len = ((w.startIndex - this.startIndex) + w.length);
                        return (len);
                    }
                }
            }
            return (length);
        }
        public int    GetNerChainLength()
        {
            if ( nerNext != null )
            {
                var len = 1;
                for ( var w = this; ; w = w.nerNext )
                {
                    if ( w.nerNext == null )
                    {
                        return (len);
                    }
                    len++;
                }
            }
            return (1);
        }

        //---public byte Tag;
        #endregion

        #region [.morphology.]
#if DEBUG
        public WordFormMorphology_t[] morphologies;
#endif
        public WordFormMorphology_t morphology;
        #endregion

        #region [.syntax-role.]
        public SyntaxRoleType syntaxRoleType;
        #endregion

        #region [.to-string's.]
        public override string ToString()
        {
            return ('\'' + valueUpper + " (" + valueOriginal + ")', [" + startIndex + ":" + length + "], " +

                    "N:['" + nerInputType.ToString() + "'  " +
                    '\'' + ((nerOutputType == NerOutputType.O) ? "-" : nerOutputType.ToString()) + '\'' +
                    ((nerNext != null) ? " {-chain-}" : string.Empty) + "], " +

                    "P:['" + posTaggerInputType.ToString() + "'  " +
                    '\'' + ((posTaggerOutputType == PosTaggerOutputType.Other) ? "-" : posTaggerOutputType.ToString()) + '\'' +
                    ((posTaggerExtraWordType != PosTaggerExtraWordType.__DEFAULT__) ? (" (is-" + posTaggerExtraWordType.ToString().ToLowerInvariant() + ')') : string.Empty) + "], " +

                    ("M:" + morphology.ToString()) //(morphology.IsEmptyMorphoAttribute() ? string.Empty : ("M:" + morphology.ToString()))
                    + ", " +
                    ("S:['" + ((syntaxRoleType == SyntaxRoleType.Other) ? "-" : syntaxRoleType.ToString()) + "']")
                   );
        }
        public string ToStringPosTagger( bool getShorty = false )
        {
            var str = string.Format( "{0}, {1}",
                                        (posTaggerInputType  == PosTaggerInputType .O    ) ? "-" : posTaggerInputType .ToText(),
                                        (posTaggerOutputType == PosTaggerOutputType.Other) ? "-" : posTaggerOutputType.ToText() +
                                        ((!getShorty && posTaggerExtraWordType != PosTaggerExtraWordType.__DEFAULT__) 
                                          ? (" [is-" + posTaggerExtraWordType.ToString().ToLowerInvariant() + ']') : string.Empty)
                                    );
            return (str);
        }
        public string ToStringNer( bool whenBothOtherReturnNull )
        {
            if ( !whenBothOtherReturnNull || (nerInputType != NerInputType.O) || (nerOutputType != NerOutputType.O) )
            {
                var str = string.Format( "{0}, {1}",
                                         (nerInputType  == NerInputType .O) ? "-" : nerInputType .ToText(),
                                         (nerOutputType == NerOutputType.O) ? "-" : nerOutputType.ToText()
                                       );
                return (str);
            }

            return (null);
        }
        public string ToStringMorphology( bool notEmptyMorphoAttribute )
        {
            if ( notEmptyMorphoAttribute && morphology.IsEmptyMorphoAttribute() )
            {
                return (null);
            }
            return (morphology.ToString());
        }
        #endregion
    }
}