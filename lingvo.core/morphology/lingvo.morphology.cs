using System;
using System.Collections.Generic;
using System.Runtime;
using lingvo.core;

namespace lingvo.morphology
{
    /// <summary>
    /// 
    /// </summary>
    unsafe public interface IBaseMorphoFormNative
    {
        char* Base                    { get; }
        char*[] MorphoFormEndings     { get; }
        PartOfSpeechEnum PartOfSpeech { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IBaseMorphoForm
    {
        string NormalForm { get; }
        PartOfSpeechEnum PartOfSpeech { get; }
    }

	/// <summary>
    /// форма слова
	/// </summary>
	public struct WordForm_t
	{
        public WordForm_t( string form, PartOfSpeechEnum partOfSpeech )
        {
            Form         = form;
            PartOfSpeech = partOfSpeech;
        }

		/// форма
		public string Form;
		/// часть речи
		public PartOfSpeechEnum PartOfSpeech;

        public override string ToString()
        {
            return ('[' + Form + ", " + PartOfSpeech + ']');
        }
	}

	/// <summary>
    /// формы слова
	/// </summary>
	public struct WordForms_t
	{
        private static readonly List< WordForm_t > EMPTY = new List< WordForm_t >( 0 );

        public WordForms_t( string word )
        {
            Word  = word;
            Forms = EMPTY;
        }

		/// исходное слово
		public string Word;
		/// формы слова
		public List< WordForm_t > Forms;

        public bool HasForms
        {
            get { return (Forms != null && Forms.Count != 0); }
        }

        public override string ToString()
        {
            return ('[' + Word + ", {" + string.Join( ",", Forms ) + "}]");
        }
	}

    /// <summary>
    /// морфохарактеристики формы слова
    /// </summary>
    unsafe public struct WordFormMorphology_t
    {
        private char* _Base;
        private char* _Ending;

        public WordFormMorphology_t( IBaseMorphoFormNative baseMorphoForm, MorphoAttributeEnum morphoAttribute ) 
            : this()
        {
            _Base           = baseMorphoForm.Base;
            _Ending         = baseMorphoForm.MorphoFormEndings[ 0 ];
            PartOfSpeech    = baseMorphoForm.PartOfSpeech;
            MorphoAttribute = morphoAttribute;
        }
        public WordFormMorphology_t( IBaseMorphoForm baseMorphoForm, MorphoAttributeEnum morphoAttribute )
            : this()
        {
            _NormalForm     = baseMorphoForm.NormalForm;
            PartOfSpeech    = baseMorphoForm.PartOfSpeech;
            MorphoAttribute = morphoAttribute;
        }
        public WordFormMorphology_t( PartOfSpeechEnum partOfSpeech )
            : this()
        {
            PartOfSpeech = partOfSpeech;
        }
        public WordFormMorphology_t( PartOfSpeechEnum partOfSpeech, MorphoAttributeEnum morphoAttribute )
            : this()
        {
            PartOfSpeech    = partOfSpeech;
            MorphoAttribute = morphoAttribute;
        }

        private string _NormalForm;
        /// нормальная форма
        public string NormalForm
        {
            get
            {
                if ( _NormalForm == null )
                {
                    if ( (IntPtr) _Base != IntPtr.Zero )
                    {
                        _NormalForm = StringsHelper.CreateWordForm( _Base, _Ending );
                    }
                }
                return (_NormalForm);
            }
        }
        /// часть речи
        public readonly PartOfSpeechEnum PartOfSpeech;
        /// морфохарактеристики
        public readonly MorphoAttributeEnum MorphoAttribute;

        public bool IsEmpty()
        {
            return ((MorphoAttribute == MorphoAttributeEnum.__UNDEFINED__) &&
                    (PartOfSpeech == PartOfSpeechEnum.Other)               &&
                    ((_NormalForm == null) && ((IntPtr) _Base == IntPtr.Zero))
                   );
        }
        public bool IsEmptyMorphoAttribute()
        {
            return (MorphoAttribute == MorphoAttributeEnum.__UNDEFINED__);
        }
        public bool IsEmptyNormalForm()
        {
            return ((_NormalForm == null) && ((IntPtr) _Base == IntPtr.Zero));
        }

        public override string ToString()
        {
            return ('[' + NormalForm + ", " + PartOfSpeech + ", " + MorphoAttribute + "]");
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static bool Equals( WordFormMorphology_t x, WordFormMorphology_t y )
        {
            return (Equals( ref x, ref y ));
        }
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static bool Equals( ref WordFormMorphology_t x, ref WordFormMorphology_t y )
        {
            return ( (x.MorphoAttribute == y.MorphoAttribute) &&
                     (x.PartOfSpeech    == y.PartOfSpeech   ) &&
                     (x._Base           == y._Base          ) &&
                     (x._Ending         == y._Ending        ) &&
                     (string .CompareOrdinal( x._NormalForm, y._NormalForm ) == 0)
                   );
        }
    }

	/// <summary>
    /// информация о морфологических свойствах слова
	/// </summary>
	public struct WordMorphology_t
	{
		/// массив морфохарактеристик
		public List< WordFormMorphology_t > WordFormMorphologies;
		/// часть речи
		public PartOfSpeechEnum PartOfSpeech;
        public bool             IsSinglePartOfSpeech;

        public bool HasWordFormMorphologies
        {
            get { return (WordFormMorphologies != null && WordFormMorphologies.Count != 0); }
        }

        public override string ToString()
        {
            return ("[" /*+ Word + ", "*/ + PartOfSpeech + ", {" +
                    (HasWordFormMorphologies ? string.Join( ",", WordFormMorphologies ) : "NULL") + 
                    "}]"
                   );
        }
	}
}
