using System;
using System.Runtime.InteropServices;

namespace lingvo.crfsuite
{
    #region [.learner. not used.]
    ///// <summary>
    ///// 
    ///// </summary>
    //public enum AlgorithmEnum : int
    //{
    //    /*algorithm_*/ lbfgs = 0,
    //    /*algorithm_*/ l2sgd = 1,
    //    /*algorithm_*/ ap    = 2,
    //    /*algorithm_*/ pa    = 3,
    //    /*algorithm_*/ arow  = 4,
    //}
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public enum NgramTypeEnum : int
    {
        Ngram_First  = 0,
        Ngram_Middle = 1,
        Ngram_Last   = 2,
    }
    /// <summary>
    /// 
    /// </summary>
    public enum NgramOrderTypeEnum : int
    {
        NgramOrder_Default = 0,
        NgramOrder_BOS     = 1,
        NgramOrder_EOS     = 2,
    }

    /// <summary>
    /// 
    /// </summary>
    unsafe public static class Native
    {
        static Native() => load_native_crf_suite();

        private static bool IsLinux()
        {
            var p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }
        private static bool Isx64() => (IntPtr.Size == 8);

        private const string DLL_WIN_x64 = "crfsuite_x64.dll";
        private const string DLL_WIN_x86 = "crfsuite_x86.dll";
        private const string DLL_LIN_x64 = "libcrfsuite.so";
        private const string DLL_LIN_x86 = DLL_LIN_x64;
		
        private const string crf_tagger_initialize_name               = "crf_tagger_initialize";
        private const string crf_tagger_beginAddItemSequence_name     = "crf_tagger_beginAddItemSequence";
        private const string crf_tagger_beginAddItemAttribute_name    = "crf_tagger_beginAddItemAttribute";
        private const string crf_tagger_addItemAttribute_name         = "crf_tagger_addItemAttribute";
        private const string crf_tagger_addItemAttributeNameOnly_name = "crf_tagger_addItemAttributeNameOnly";
        private const string crf_tagger_endAddItemAttribute_name      = "crf_tagger_endAddItemAttribute";
        private const string crf_tagger_endAddItemSequence_name       = "crf_tagger_endAddItemSequence";
        private const string crf_tagger_tag_name                      = "crf_tagger_tag";
        private const string crf_tagger_tag_with_probability_name     = "crf_tagger_tag_with_probability";
        private const string crf_tagger_tag_with_marginal_name        = "crf_tagger_tag_with_marginal";        
        private const string crf_tagger_getResultLength_name          = "crf_tagger_getResultLength";
        private const string crf_tagger_getResultValue_name           = "crf_tagger_getResultValue";
        private const string crf_tagger_getResultMarginal_name        = "crf_tagger_getResultMarginal";
        private const string crf_tagger_uninitialize_name             = "crf_tagger_uninitialize";

        private const string crf_tagger_ma_initialize_name                = "crf_tagger_ma_initialize";
        private const string crf_tagger_ma_beginAddNgramSequence_name     = "crf_tagger_ma_beginAddNgramSequence";
        private const string crf_tagger_ma_addNgramSequence_name          = "crf_tagger_ma_addNgramSequence";
        private const string crf_tagger_ma_endAddNgramSequence_name       = "crf_tagger_ma_endAddNgramSequence";
        private const string crf_tagger_ma_setNgramValue_name             = "crf_tagger_ma_setNgramValue";
        private const string crf_tagger_ma_tagNgram_with_probability_name = "crf_tagger_ma_tagNgram_with_probability";
        private const string crf_tagger_ma_getResultValue_name            = "crf_tagger_ma_getResultValue";
        private const string crf_tagger_ma_uninitialize_name              = "crf_tagger_ma_uninitialize";

        #region [.learner. not used.]
        /*
        private const string crf_learner_initialize_name            = "crf_learner_initialize";
        private const string crf_learner_beginAddItemSequence_name  = "crf_learner_beginAddItemSequence";
        private const string crf_learner_beginAddItemAttribute_name = "crf_learner_beginAddItemAttribute";
        private const string crf_learner_addItemAttribute_name      = "crf_learner_addItemAttribute";
        private const string crf_learner_endAddItemAttribute_name   = "crf_learner_endAddItemAttribute";
        private const string crf_learner_endAddItemSequence_name    = "crf_learner_endAddItemSequence";
        private const string crf_learner_beginAddStringList_name    = "crf_learner_beginAddStringList";
        private const string crf_learner_addString_name             = "crf_learner_addString";
        private const string crf_learner_endAddStringList_name      = "crf_learner_endAddStringList";
        private const string crf_learner_append_name                = "crf_learner_append";
        private const string crf_learner_train_name                 = "crf_learner_train";
        private const string crf_learner_uninitialize_name          = "crf_learner_uninitialize";
        */
        #endregion

        public delegate IntPtr crf_tagger_initialize_Delegate( IntPtr/*const char* */ name );
        public delegate void   crf_tagger_beginAddItemSequence_Delegate( IntPtr taggerWrapper );
        public delegate void   crf_tagger_beginAddItemAttribute_Delegate( IntPtr taggerWrapper );
        public delegate bool   crf_tagger_addItemAttribute_Delegate( IntPtr taggerWrapper, byte*/*IntPtr*/ /*const char* */ name, double val );
        public delegate void   crf_tagger_addItemAttributeNameOnly_Delegate( IntPtr taggerWrapper, byte*/*IntPtr*/ /*const char* */ name );
        public delegate void   crf_tagger_endAddItemAttribute_Delegate( IntPtr taggerWrapper );
        public delegate void   crf_tagger_endAddItemSequence_Delegate( IntPtr taggerWrapper );
        public delegate void   crf_tagger_tag_Delegate( IntPtr taggerWrapper );
        public delegate double crf_tagger_tag_with_probability_Delegate( IntPtr taggerWrapper );
        public delegate double crf_tagger_tag_with_marginal_Delegate( IntPtr taggerWrapper );        
        public delegate uint/*size_t*/ crf_tagger_getResultLength_Delegate( IntPtr taggerWrapper );
        public delegate IntPtr/*const char* */ crf_tagger_getResultValue_Delegate( IntPtr taggerWrapper, uint/*size_t*/ index );
        public delegate double crf_tagger_getResultMarginal_Delegate( IntPtr taggerWrapper, uint/*size_t*/ index );
        public delegate void   crf_tagger_uninitialize_Delegate( IntPtr taggerWrapper );

        public delegate IntPtr crf_tagger_ma_initialize_Delegate( IntPtr/*const char* */ name );
        public delegate void   crf_tagger_ma_beginAddNgramSequence_Delegate( IntPtr taggerWrapper, NgramTypeEnum ngramType );
        public delegate void   crf_tagger_ma_addNgramSequence_Delegate( IntPtr taggerWrapper, byte*/*IntPtr*/ /*const char* */ ngram );
        public delegate void   crf_tagger_ma_endAddNgramSequence_Delegate( IntPtr taggerWrapper );
        public delegate void   crf_tagger_ma_setNgramValue_Delegate( IntPtr taggerWrapper, NgramTypeEnum ngramType, int attrIndex, int attrValueIndex, byte*/*IntPtr*/ /*const char* */ value );
        public delegate double crf_tagger_ma_tagNgram_with_probability_Delegate( IntPtr taggerWrapper, NgramTypeEnum ngramType, NgramOrderTypeEnum ngramOrderType );        
        public delegate IntPtr/*const char* */ crf_tagger_ma_getResultValue_Delegate( IntPtr taggerWrapper );
        public delegate void   crf_tagger_ma_uninitialize_Delegate ( IntPtr taggerWrapper );

        #region [.learner. not used.]
        //public delegate IntPtr crf_learner_initialize_Delegate( IntPtr/*const char* */ modelFilename, AlgorithmEnum algorithmEnum );
        //public delegate void   crf_learner_beginAddItemSequence_Delegate( IntPtr learnerWrapper );
        //public delegate void   crf_learner_beginAddItemAttribute_Delegate( IntPtr learnerWrapper );
        //public delegate bool   crf_learner_addItemAttribute_Delegate( IntPtr learnerWrapper, byte*/*IntPtr*/ /*const char* */ name, double val );
        //public delegate void   crf_learner_endAddItemAttribute_Delegate( IntPtr learnerWrapper );
        //public delegate void   crf_learner_endAddItemSequence_Delegate( IntPtr learnerWrapper );
        //public delegate void   crf_learner_beginAddStringList_Delegate( IntPtr learnerWrapper );
        //public delegate bool   crf_learner_addString_Delegate( IntPtr learnerWrapper, byte*/*IntPtr*/ /*const char* */ name );
        //public delegate void   crf_learner_endAddStringList_Delegate( IntPtr learnerWrapper );
        //public delegate bool   crf_learner_append_Delegate( IntPtr learnerWrapper );        
        //public delegate bool   crf_learner_train_Delegate( IntPtr learnerWrapper );
        //public delegate void   crf_learner_uninitialize_Delegate( IntPtr learnerWrapper );
        #endregion

        #region [.win.]
        #region [.x64.]
        [DllImport(DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_initialize_name)]
        private extern static IntPtr crf_tagger_initialize_win_x64( IntPtr/*string*/ name );

        [DllImport(DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemSequence_name)]
        private extern static void crf_tagger_beginAddItemSequence_win_x64( IntPtr taggerWrapper );

        [DllImport(DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemAttribute_name)]
        private extern static void crf_tagger_beginAddItemAttribute_win_x64( IntPtr taggerWrapper );

        [DllImport(DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttribute_name)]
        private extern static bool crf_tagger_addItemAttribute_win_x64( IntPtr taggerWrapper, byte*/*IntPtr*/ name, double val );

        [DllImport( DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttributeNameOnly_name )]
        private extern static void crf_tagger_addItemAttributeNameOnly_win_x64( IntPtr taggerWrapper, byte*/*IntPtr*/ name );

        [DllImport(DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemAttribute_name)]
        private extern static void crf_tagger_endAddItemAttribute_win_x64( IntPtr taggerWrapper );

        [DllImport(DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemSequence_name)]
        private extern static void crf_tagger_endAddItemSequence_win_x64( IntPtr taggerWrapper );

        [DllImport(DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_name)]
        private extern static void crf_tagger_tag_win_x64( IntPtr taggerWrapper );

        [DllImport( DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_probability_name )]
        private extern static double crf_tagger_tag_with_probability_win_x64( IntPtr taggerWrapper );

        [DllImport( DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_marginal_name )]
        private extern static double crf_tagger_tag_with_marginal_win_x64( IntPtr taggerWrapper );  

        [DllImport(DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultLength_name)]
        private extern static uint crf_tagger_getResultLength_win_x64( IntPtr taggerWrapper );

        [DllImport(DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultValue_name)]
        private extern static IntPtr crf_tagger_getResultValue_win_x64( IntPtr taggerWrapper, uint index );

        [DllImport(DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultMarginal_name)]
        private extern static double crf_tagger_getResultMarginal_win_x64( IntPtr taggerWrapper, uint index );        

        [DllImport(DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_uninitialize_name)]
        private extern static void crf_tagger_uninitialize_win_x64( IntPtr taggerWrapper );


        [DllImport( DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_initialize_name )]
        private extern static IntPtr crf_tagger_ma_initialize_win_x64( IntPtr name );

        [DllImport( DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_beginAddNgramSequence_name )]
        private extern static void crf_tagger_ma_beginAddNgramSequence_win_x64( IntPtr taggerWrapper, NgramTypeEnum ngramType );

        [DllImport( DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_addNgramSequence_name )]
        private extern static void crf_tagger_ma_addNgramSequence_win_x64( IntPtr taggerWrapper, byte* ngram );

        [DllImport( DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_endAddNgramSequence_name )]
        private extern static void crf_tagger_ma_endAddNgramSequence_win_x64( IntPtr taggerWrapper );

        [DllImport( DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_setNgramValue_name )]
        private extern static void crf_tagger_ma_setNgramValue_win_x64( IntPtr taggerWrapper, NgramTypeEnum ngramType, int attrIndex, int attrValueIndex, byte* value );

        [DllImport( DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_tagNgram_with_probability_name )]
        private extern static double crf_tagger_ma_tagNgram_with_probability_win_x64( IntPtr taggerWrapper, NgramTypeEnum ngramType, NgramOrderTypeEnum ngramOrderType );

        [DllImport( DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_getResultValue_name )]
        private extern static IntPtr crf_tagger_ma_getResultValue_win_x64( IntPtr taggerWrapper );

        [DllImport( DLL_WIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_uninitialize_name )]
        private extern static void crf_tagger_ma_uninitialize_win_x64( IntPtr taggerWrapper );

        #region [.learner. not used.]
        /*
        [DllImport( DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_initialize_name )]
        private extern static IntPtr crf_learner_initialize_win_x64( IntPtr modelFilename, AlgorithmEnum algorithmEnum );

        [DllImport( DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_beginAddItemSequence_name )]
        private extern static void crf_learner_beginAddItemSequence_win_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_beginAddItemAttribute_name )]
        private extern static void crf_learner_beginAddItemAttribute_win_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_addItemAttribute_name )]
        private extern static bool crf_learner_addItemAttribute_win_x64( IntPtr learnerWrapper, byte* name, double val );

        [DllImport( DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_endAddItemAttribute_name )]
        private extern static void crf_learner_endAddItemAttribute_win_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_endAddItemSequence_name )]
        private extern static void crf_learner_endAddItemSequence_win_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_beginAddStringList_name )]
        private extern static void crf_learner_beginAddStringList_win_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_addString_name )]
        private extern static bool crf_learner_addString_win_x64( IntPtr learnerWrapper, byte* name );

        [DllImport( DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_endAddStringList_name )]
        private extern static void crf_learner_endAddStringList_win_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_append_name )]
        private extern static bool crf_learner_append_win_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_train_name )]
        private extern static bool crf_learner_train_win_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_uninitialize_name )]
        private extern static void crf_learner_uninitialize_win_x64( IntPtr learnerWrapper );
        */
        #endregion

        #endregion

        #region [.x86.]
        [DllImport(DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_initialize_name)]
        private extern static IntPtr crf_tagger_initialize_win_x86(IntPtr/*string*/ name);

        [DllImport(DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemSequence_name)]
        private extern static void crf_tagger_beginAddItemSequence_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemAttribute_name)]
        private extern static void crf_tagger_beginAddItemAttribute_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttribute_name)]
        private extern static bool crf_tagger_addItemAttribute_win_x86(IntPtr taggerWrapper, byte*/*IntPtr*/ name, double val);

        [DllImport( DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttributeNameOnly_name )]
        private extern static void crf_tagger_addItemAttributeNameOnly_win_x86( IntPtr taggerWrapper, byte*/*IntPtr*/ name );

        [DllImport(DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemAttribute_name)]
        private extern static void crf_tagger_endAddItemAttribute_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemSequence_name)]
        private extern static void crf_tagger_endAddItemSequence_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_name)]
        private extern static void crf_tagger_tag_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_probability_name)]
        private extern static double crf_tagger_tag_with_probability_win_x86( IntPtr taggerWrapper );

        [DllImport( DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_marginal_name )]
        private extern static double crf_tagger_tag_with_marginal_win_x86( IntPtr taggerWrapper );  

        [DllImport(DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultLength_name)]
        private extern static uint crf_tagger_getResultLength_win_x86(IntPtr taggerWrapper);

        [DllImport(DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultValue_name)]
        private extern static IntPtr crf_tagger_getResultValue_win_x86(IntPtr taggerWrapper, uint index);

        [DllImport( DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultMarginal_name )]
        private extern static double crf_tagger_getResultMarginal_win_x86( IntPtr taggerWrapper, uint index );        

        [DllImport(DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_uninitialize_name)]
        private extern static void crf_tagger_uninitialize_win_x86(IntPtr taggerWrapper);


        [DllImport( DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_initialize_name )]
        private extern static IntPtr crf_tagger_ma_initialize_win_x86( IntPtr name );

        [DllImport( DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_beginAddNgramSequence_name )]
        private extern static void crf_tagger_ma_beginAddNgramSequence_win_x86( IntPtr taggerWrapper, NgramTypeEnum ngramType );

        [DllImport( DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_addNgramSequence_name )]
        private extern static void crf_tagger_ma_addNgramSequence_win_x86( IntPtr taggerWrapper, byte* ngram );

        [DllImport( DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_endAddNgramSequence_name )]
        private extern static void crf_tagger_ma_endAddNgramSequence_win_x86( IntPtr taggerWrapper );

        [DllImport( DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_setNgramValue_name )]
        private extern static void crf_tagger_ma_setNgramValue_win_x86( IntPtr taggerWrapper, NgramTypeEnum ngramType, int attrIndex, int attrValueIndex, byte* value );

        [DllImport( DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_tagNgram_with_probability_name )]
        private extern static double crf_tagger_ma_tagNgram_with_probability_win_x86( IntPtr taggerWrapper, NgramTypeEnum ngramType, NgramOrderTypeEnum ngramOrderType );

        [DllImport( DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_getResultValue_name )]
        private extern static IntPtr crf_tagger_ma_getResultValue_win_x86( IntPtr taggerWrapper );

        [DllImport( DLL_WIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_uninitialize_name )]
        private extern static void crf_tagger_ma_uninitialize_win_x86( IntPtr taggerWrapper );

        #region [.learner. not used.]
        /*
        [DllImport( DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_initialize_name )]
        private extern static IntPtr crf_learner_initialize_win_x86( IntPtr modelFilename, AlgorithmEnum algorithmEnum );

        [DllImport( DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_beginAddItemSequence_name )]
        private extern static void crf_learner_beginAddItemSequence_win_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_beginAddItemAttribute_name )]
        private extern static void crf_learner_beginAddItemAttribute_win_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_addItemAttribute_name )]
        private extern static bool crf_learner_addItemAttribute_win_x86( IntPtr learnerWrapper, byte* name, double val );

        [DllImport( DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_endAddItemAttribute_name )]
        private extern static void crf_learner_endAddItemAttribute_win_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_endAddItemSequence_name )]
        private extern static void crf_learner_endAddItemSequence_win_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_beginAddStringList_name )]
        private extern static void crf_learner_beginAddStringList_win_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_addString_name )]
        private extern static bool crf_learner_addString_win_x86( IntPtr learnerWrapper, byte* name );

        [DllImport( DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_endAddStringList_name )]
        private extern static void crf_learner_endAddStringList_win_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_append_name )]
        private extern static bool crf_learner_append_win_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_train_name )]
        private extern static bool crf_learner_train_win_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_WINDOWS_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_uninitialize_name )]
        private extern static void crf_learner_uninitialize_win_x86( IntPtr learnerWrapper );
        */
        #endregion

        #endregion
        #endregion

        #region [.linux.]
        #region [.x64.]
        [DllImport(DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_initialize_name)]
        private extern static IntPtr crf_tagger_initialize_lin_x64(IntPtr/*string*/ name);

        [DllImport(DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemSequence_name)]
        private extern static void crf_tagger_beginAddItemSequence_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemAttribute_name)]
        private extern static void crf_tagger_beginAddItemAttribute_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttribute_name)]
        private extern static bool crf_tagger_addItemAttribute_lin_x64(IntPtr taggerWrapper, byte*/*IntPtr*/ name, double val);

        [DllImport( DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttributeNameOnly_name )]
        private extern static void crf_tagger_addItemAttributeNameOnly_lin_x64( IntPtr taggerWrapper, byte*/*IntPtr*/ name );

        [DllImport(DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemAttribute_name)]
        private extern static void crf_tagger_endAddItemAttribute_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemSequence_name)]
        private extern static void crf_tagger_endAddItemSequence_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_name)]
        private extern static void crf_tagger_tag_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_probability_name)]
        private extern static double crf_tagger_tag_with_probability_lin_x64( IntPtr taggerWrapper );

        [DllImport(DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_marginal_name)]
        private extern static double crf_tagger_tag_with_marginal_lin_x64( IntPtr taggerWrapper );

        [DllImport(DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultLength_name)]
        private extern static uint crf_tagger_getResultLength_lin_x64(IntPtr taggerWrapper);

        [DllImport(DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultValue_name)]
        private extern static IntPtr crf_tagger_getResultValue_lin_x64(IntPtr taggerWrapper, uint index);

        [DllImport( DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultMarginal_name )]
        private extern static double crf_tagger_getResultMarginal_lin_x64( IntPtr taggerWrapper, uint index );        

        [DllImport(DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_uninitialize_name)]
        private extern static void crf_tagger_uninitialize_lin_x64(IntPtr taggerWrapper);


        [DllImport( DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_initialize_name )]
        private extern static IntPtr crf_tagger_ma_initialize_lin_x64( IntPtr name );

        [DllImport( DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_beginAddNgramSequence_name )]
        private extern static void crf_tagger_ma_beginAddNgramSequence_lin_x64( IntPtr taggerWrapper, NgramTypeEnum ngramType );

        [DllImport( DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_addNgramSequence_name )]
        private extern static void crf_tagger_ma_addNgramSequence_lin_x64( IntPtr taggerWrapper, byte* ngram );

        [DllImport( DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_endAddNgramSequence_name )]
        private extern static void crf_tagger_ma_endAddNgramSequence_lin_x64( IntPtr taggerWrapper );

        [DllImport( DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_setNgramValue_name )]
        private extern static void crf_tagger_ma_setNgramValue_lin_x64( IntPtr taggerWrapper, NgramTypeEnum ngramType, int attrIndex, int attrValueIndex, byte* value );

        [DllImport( DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_tagNgram_with_probability_name )]
        private extern static double crf_tagger_ma_tagNgram_with_probability_lin_x64( IntPtr taggerWrapper, NgramTypeEnum ngramType, NgramOrderTypeEnum ngramOrderType );

        [DllImport( DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_getResultValue_name )]
        private extern static IntPtr crf_tagger_ma_getResultValue_lin_x64( IntPtr taggerWrapper );

        [DllImport( DLL_LIN_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_uninitialize_name )]
        private extern static void crf_tagger_ma_uninitialize_lin_x64( IntPtr taggerWrapper );

        #region [.learner. not used.]
        /*
        [DllImport( DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_initialize_name )]
        private extern static IntPtr crf_learner_initialize_lin_x64( IntPtr modelFilename, AlgorithmEnum algorithmEnum );

        [DllImport( DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_beginAddItemSequence_name )]
        private extern static void crf_learner_beginAddItemSequence_lin_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_beginAddItemAttribute_name )]
        private extern static void crf_learner_beginAddItemAttribute_lin_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_addItemAttribute_name )]
        private extern static bool crf_learner_addItemAttribute_lin_x64( IntPtr learnerWrapper, byte* name, double val );

        [DllImport( DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_endAddItemAttribute_name )]
        private extern static void crf_learner_endAddItemAttribute_lin_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_endAddItemSequence_name )]
        private extern static void crf_learner_endAddItemSequence_lin_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_beginAddStringList_name )]
        private extern static void crf_learner_beginAddStringList_lin_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_addString_name )]
        private extern static bool crf_learner_addString_lin_x64( IntPtr learnerWrapper, byte* name );

        [DllImport( DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_endAddStringList_name )]
        private extern static void crf_learner_endAddStringList_lin_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_append_name )]
        private extern static bool crf_learner_append_lin_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_train_name )]
        private extern static bool crf_learner_train_lin_x64( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x64, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_uninitialize_name )]
        private extern static void crf_learner_uninitialize_lin_x64( IntPtr learnerWrapper );
        */
        #endregion

        #endregion

        #region [.x86.]
        [DllImport(DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_initialize_name)]
        private extern static IntPtr crf_tagger_initialize_lin_x86(IntPtr/*string*/ name);

        [DllImport(DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemSequence_name)]
        private extern static void crf_tagger_beginAddItemSequence_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_beginAddItemAttribute_name)]
        private extern static void crf_tagger_beginAddItemAttribute_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttribute_name)]
        private extern static bool crf_tagger_addItemAttribute_lin_x86(IntPtr taggerWrapper, byte*/*IntPtr*/ name, double val);

        [DllImport( DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_addItemAttributeNameOnly_name )]
        private extern static void crf_tagger_addItemAttributeNameOnly_lin_x86( IntPtr taggerWrapper, byte*/*IntPtr*/ name );

        [DllImport(DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemAttribute_name)]
        private extern static void crf_tagger_endAddItemAttribute_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_endAddItemSequence_name)]
        private extern static void crf_tagger_endAddItemSequence_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_name)]
        private extern static void crf_tagger_tag_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_probability_name)]
        private extern static double crf_tagger_tag_with_probability_lin_x86( IntPtr taggerWrapper );

        [DllImport( DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_tag_with_marginal_name )]
        private extern static double crf_tagger_tag_with_marginal_lin_x86( IntPtr taggerWrapper );  

        [DllImport(DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultLength_name)]
        private extern static uint crf_tagger_getResultLength_lin_x86(IntPtr taggerWrapper);

        [DllImport(DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultValue_name)]
        private extern static IntPtr crf_tagger_getResultValue_lin_x86( IntPtr taggerWrapper, uint index );

        [DllImport( DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_getResultMarginal_name )]
        private extern static double crf_tagger_getResultMarginal_lin_x86( IntPtr taggerWrapper, uint index ); 

        [DllImport(DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_uninitialize_name)]
        private extern static void crf_tagger_uninitialize_lin_x86(IntPtr taggerWrapper);


        [DllImport( DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_initialize_name )]
        private extern static IntPtr crf_tagger_ma_initialize_lin_x86( IntPtr name );

        [DllImport( DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_beginAddNgramSequence_name )]
        private extern static void crf_tagger_ma_beginAddNgramSequence_lin_x86( IntPtr taggerWrapper, NgramTypeEnum ngramType );

        [DllImport( DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_addNgramSequence_name )]
        private extern static void crf_tagger_ma_addNgramSequence_lin_x86( IntPtr taggerWrapper, byte* ngram );

        [DllImport( DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_endAddNgramSequence_name )]
        private extern static void crf_tagger_ma_endAddNgramSequence_lin_x86( IntPtr taggerWrapper );

        [DllImport( DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_setNgramValue_name )]
        private extern static void crf_tagger_ma_setNgramValue_lin_x86( IntPtr taggerWrapper, NgramTypeEnum ngramType, int attrIndex, int attrValueIndex, byte* value );

        [DllImport( DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_tagNgram_with_probability_name )]
        private extern static double crf_tagger_ma_tagNgram_with_probability_lin_x86( IntPtr taggerWrapper, NgramTypeEnum ngramType, NgramOrderTypeEnum ngramOrderType );

        [DllImport( DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_getResultValue_name )]
        private extern static IntPtr crf_tagger_ma_getResultValue_lin_x86( IntPtr taggerWrapper );

        [DllImport( DLL_LIN_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_tagger_ma_uninitialize_name )]
        private extern static void crf_tagger_ma_uninitialize_lin_x86( IntPtr taggerWrapper );

        #region [.learner. not used.]
        /*
        [DllImport( DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_initialize_name )]
        private extern static IntPtr crf_learner_initialize_lin_x86( IntPtr modelFilename, AlgorithmEnum algorithmEnum );

        [DllImport( DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_beginAddItemSequence_name )]
        private extern static void crf_learner_beginAddItemSequence_lin_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_beginAddItemAttribute_name )]
        private extern static void crf_learner_beginAddItemAttribute_lin_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_addItemAttribute_name )]
        private extern static bool crf_learner_addItemAttribute_lin_x86( IntPtr learnerWrapper, byte* name, double val );

        [DllImport( DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_endAddItemAttribute_name )]
        private extern static void crf_learner_endAddItemAttribute_lin_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_endAddItemSequence_name )]
        private extern static void crf_learner_endAddItemSequence_lin_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_beginAddStringList_name )]
        private extern static void crf_learner_beginAddStringList_lin_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_addString_name )]
        private extern static bool crf_learner_addString_lin_x86( IntPtr learnerWrapper, byte* name );

        [DllImport( DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_endAddStringList_name )]
        private extern static void crf_learner_endAddStringList_lin_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_append_name )]
        private extern static bool crf_learner_append_lin_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_train_name )]
        private extern static bool crf_learner_train_lin_x86( IntPtr learnerWrapper );

        [DllImport( DLL_NAME_LINUX_x86, CallingConvention = CallingConvention.Cdecl, EntryPoint = crf_learner_uninitialize_name )]
        private extern static void crf_learner_uninitialize_lin_x86( IntPtr learnerWrapper );
        */
        #endregion

        #endregion
        #endregion

        public static crf_tagger_initialize_Delegate               crf_tagger_initialize { get; private set; }
        public static crf_tagger_beginAddItemSequence_Delegate     crf_tagger_beginAddItemSequence { get; private set; }
        public static crf_tagger_beginAddItemAttribute_Delegate    crf_tagger_beginAddItemAttribute { get; private set; }
        public static crf_tagger_addItemAttribute_Delegate         crf_tagger_addItemAttribute { get; private set; }
        public static crf_tagger_addItemAttributeNameOnly_Delegate crf_tagger_addItemAttributeNameOnly { get; private set; }
        public static crf_tagger_endAddItemAttribute_Delegate      crf_tagger_endAddItemAttribute { get; private set; }
        public static crf_tagger_endAddItemSequence_Delegate       crf_tagger_endAddItemSequence { get; private set; }
        public static crf_tagger_tag_Delegate                      crf_tagger_tag { get; private set; }
        public static crf_tagger_tag_with_probability_Delegate     crf_tagger_tag_with_probability { get; private set; }
        public static crf_tagger_tag_with_marginal_Delegate        crf_tagger_tag_with_marginal { get; private set; }
        public static crf_tagger_getResultLength_Delegate          crf_tagger_getResultLength { get; private set; }
        public static crf_tagger_getResultValue_Delegate           crf_tagger_getResultValue { get; private set; }
        public static crf_tagger_getResultMarginal_Delegate        crf_tagger_getResultMarginal { get; private set; }
        public static crf_tagger_uninitialize_Delegate             crf_tagger_uninitialize { get; private set; }

        public static crf_tagger_ma_initialize_Delegate                crf_tagger_ma_initialize { get; private set; }
        public static crf_tagger_ma_beginAddNgramSequence_Delegate     crf_tagger_ma_beginAddNgramSequence { get; private set; }
        public static crf_tagger_ma_addNgramSequence_Delegate          crf_tagger_ma_addNgramSequence { get; private set; }
        public static crf_tagger_ma_endAddNgramSequence_Delegate       crf_tagger_ma_endAddNgramSequence { get; private set; }
        public static crf_tagger_ma_setNgramValue_Delegate             crf_tagger_ma_setNgramValue { get; private set; }
        public static crf_tagger_ma_tagNgram_with_probability_Delegate crf_tagger_ma_tagNgram_with_probability { get; private set; }
        public static crf_tagger_ma_getResultValue_Delegate            crf_tagger_ma_getResultValue { get; private set; }
        public static crf_tagger_ma_uninitialize_Delegate              crf_tagger_ma_uninitialize { get; private set; }

        #region [.learner. not used.]
        /*
        public static crf_learner_initialize_Delegate            crf_learner_initialize;
        public static crf_learner_beginAddItemSequence_Delegate  crf_learner_beginAddItemSequence;
        public static crf_learner_beginAddItemAttribute_Delegate crf_learner_beginAddItemAttribute;
        public static crf_learner_addItemAttribute_Delegate      crf_learner_addItemAttribute;
        public static crf_learner_endAddItemAttribute_Delegate   crf_learner_endAddItemAttribute;
        public static crf_learner_endAddItemSequence_Delegate    crf_learner_endAddItemSequence;
        public static crf_learner_beginAddStringList_Delegate    crf_learner_beginAddStringList;
        public static crf_learner_addString_Delegate             crf_learner_addString;
        public static crf_learner_endAddStringList_Delegate      crf_learner_endAddStringList;
        public static crf_learner_append_Delegate                crf_learner_append;
        public static crf_learner_train_Delegate                 crf_learner_train;
        public static crf_learner_uninitialize_Delegate          crf_learner_uninitialize;
        */
        #endregion

        private static void load_native_crf_suite()
		{
            if ( IsLinux() )
            {
                if ( Isx64() )
                {
                    crf_tagger_initialize               = crf_tagger_initialize_lin_x64;
                    crf_tagger_beginAddItemSequence     = crf_tagger_beginAddItemSequence_lin_x64;
                    crf_tagger_beginAddItemAttribute    = crf_tagger_beginAddItemAttribute_lin_x64;
                    crf_tagger_addItemAttribute         = crf_tagger_addItemAttribute_lin_x64;
                    crf_tagger_addItemAttributeNameOnly = crf_tagger_addItemAttributeNameOnly_lin_x64;
                    crf_tagger_endAddItemAttribute      = crf_tagger_endAddItemAttribute_lin_x64;
                    crf_tagger_endAddItemSequence       = crf_tagger_endAddItemSequence_lin_x64;
                    crf_tagger_tag                      = crf_tagger_tag_lin_x64;
                    crf_tagger_tag_with_probability     = crf_tagger_tag_with_probability_lin_x64;
                    crf_tagger_tag_with_marginal        = crf_tagger_tag_with_marginal_lin_x64;
                    crf_tagger_getResultLength          = crf_tagger_getResultLength_lin_x64;
                    crf_tagger_getResultValue           = crf_tagger_getResultValue_lin_x64;
                    crf_tagger_getResultMarginal        = crf_tagger_getResultMarginal_lin_x64;
                    crf_tagger_uninitialize             = crf_tagger_uninitialize_lin_x64;

                    crf_tagger_ma_initialize                = crf_tagger_ma_initialize_lin_x64;
                    crf_tagger_ma_beginAddNgramSequence     = crf_tagger_ma_beginAddNgramSequence_lin_x64;
                    crf_tagger_ma_addNgramSequence          = crf_tagger_ma_addNgramSequence_lin_x64;
                    crf_tagger_ma_endAddNgramSequence       = crf_tagger_ma_endAddNgramSequence_lin_x64;
                    crf_tagger_ma_setNgramValue             = crf_tagger_ma_setNgramValue_lin_x64;
                    crf_tagger_ma_tagNgram_with_probability = crf_tagger_ma_tagNgram_with_probability_lin_x64;
                    crf_tagger_ma_getResultValue            = crf_tagger_ma_getResultValue_lin_x64;
                    crf_tagger_ma_uninitialize              = crf_tagger_ma_uninitialize_lin_x64;

                    #region [.learner. not used.]
                    /*
                    crf_learner_initialize            = crf_learner_initialize_lin_x64;
                    crf_learner_beginAddItemSequence  = crf_learner_beginAddItemSequence_lin_x64;
                    crf_learner_beginAddItemAttribute = crf_learner_beginAddItemAttribute_lin_x64;
                    crf_learner_addItemAttribute      = crf_learner_addItemAttribute_lin_x64;
                    crf_learner_endAddItemAttribute   = crf_learner_endAddItemAttribute_lin_x64;
                    crf_learner_endAddItemSequence    = crf_learner_endAddItemSequence_lin_x64;
                    crf_learner_beginAddStringList    = crf_learner_beginAddStringList_lin_x64;
                    crf_learner_addString             = crf_learner_addString_lin_x64;
                    crf_learner_endAddStringList      = crf_learner_endAddStringList_lin_x64;
                    crf_learner_append                = crf_learner_append_lin_x64;
                    crf_learner_train                 = crf_learner_train_lin_x64;
                    crf_learner_uninitialize          = crf_learner_uninitialize_lin_x64;
                    */
                    #endregion
                }
                else
                {
                    crf_tagger_initialize               = crf_tagger_initialize_lin_x86;
                    crf_tagger_beginAddItemSequence     = crf_tagger_beginAddItemSequence_lin_x86;
                    crf_tagger_beginAddItemAttribute    = crf_tagger_beginAddItemAttribute_lin_x86;
                    crf_tagger_addItemAttribute         = crf_tagger_addItemAttribute_lin_x86;
                    crf_tagger_addItemAttributeNameOnly = crf_tagger_addItemAttributeNameOnly_lin_x86;
                    crf_tagger_endAddItemAttribute      = crf_tagger_endAddItemAttribute_lin_x86;
                    crf_tagger_endAddItemSequence       = crf_tagger_endAddItemSequence_lin_x86;
                    crf_tagger_tag                      = crf_tagger_tag_lin_x86;
                    crf_tagger_tag_with_probability     = crf_tagger_tag_with_probability_lin_x86;
                    crf_tagger_tag_with_marginal        = crf_tagger_tag_with_marginal_lin_x86;
                    crf_tagger_getResultLength          = crf_tagger_getResultLength_lin_x86;
                    crf_tagger_getResultValue           = crf_tagger_getResultValue_lin_x86;
                    crf_tagger_getResultMarginal        = crf_tagger_getResultMarginal_lin_x86;
                    crf_tagger_uninitialize             = crf_tagger_uninitialize_lin_x86;

                    crf_tagger_ma_initialize                = crf_tagger_ma_initialize_lin_x86;
                    crf_tagger_ma_beginAddNgramSequence     = crf_tagger_ma_beginAddNgramSequence_lin_x86;
                    crf_tagger_ma_addNgramSequence          = crf_tagger_ma_addNgramSequence_lin_x86;
                    crf_tagger_ma_endAddNgramSequence       = crf_tagger_ma_endAddNgramSequence_lin_x86;
                    crf_tagger_ma_setNgramValue             = crf_tagger_ma_setNgramValue_lin_x86;
                    crf_tagger_ma_tagNgram_with_probability = crf_tagger_ma_tagNgram_with_probability_lin_x86;
                    crf_tagger_ma_getResultValue            = crf_tagger_ma_getResultValue_lin_x86;
                    crf_tagger_ma_uninitialize              = crf_tagger_ma_uninitialize_lin_x86;

                    #region [.learner. not used.]
                    /*
                    crf_learner_initialize            = crf_learner_initialize_lin_x86;
                    crf_learner_beginAddItemSequence  = crf_learner_beginAddItemSequence_lin_x86;
                    crf_learner_beginAddItemAttribute = crf_learner_beginAddItemAttribute_lin_x86;
                    crf_learner_addItemAttribute      = crf_learner_addItemAttribute_lin_x86;
                    crf_learner_endAddItemAttribute   = crf_learner_endAddItemAttribute_lin_x86;
                    crf_learner_endAddItemSequence    = crf_learner_endAddItemSequence_lin_x86;
                    crf_learner_beginAddStringList    = crf_learner_beginAddStringList_lin_x86;
                    crf_learner_addString             = crf_learner_addString_lin_x86;
                    crf_learner_endAddStringList      = crf_learner_endAddStringList_lin_x86;
                    crf_learner_append                = crf_learner_append_lin_x86;
                    crf_learner_train                 = crf_learner_train_lin_x86;
                    crf_learner_uninitialize          = crf_learner_uninitialize_lin_x86;
                    */
                    #endregion
                }
            } 
            else 
            {
                if ( Isx64() )
                {
                    crf_tagger_initialize               = crf_tagger_initialize_win_x64;
                    crf_tagger_beginAddItemSequence     = crf_tagger_beginAddItemSequence_win_x64;
                    crf_tagger_beginAddItemAttribute    = crf_tagger_beginAddItemAttribute_win_x64;
                    crf_tagger_addItemAttribute         = crf_tagger_addItemAttribute_win_x64;
                    crf_tagger_addItemAttributeNameOnly = crf_tagger_addItemAttributeNameOnly_win_x64;
                    crf_tagger_endAddItemAttribute      = crf_tagger_endAddItemAttribute_win_x64;
                    crf_tagger_endAddItemSequence       = crf_tagger_endAddItemSequence_win_x64;
                    crf_tagger_tag                      = crf_tagger_tag_win_x64;
                    crf_tagger_tag_with_probability     = crf_tagger_tag_with_probability_win_x64;
                    crf_tagger_tag_with_marginal        = crf_tagger_tag_with_marginal_win_x64;
                    crf_tagger_getResultLength          = crf_tagger_getResultLength_win_x64;
                    crf_tagger_getResultValue           = crf_tagger_getResultValue_win_x64;
                    crf_tagger_getResultMarginal        = crf_tagger_getResultMarginal_win_x64;
                    crf_tagger_uninitialize             = crf_tagger_uninitialize_win_x64;

                    crf_tagger_ma_initialize                = crf_tagger_ma_initialize_win_x64;
                    crf_tagger_ma_beginAddNgramSequence     = crf_tagger_ma_beginAddNgramSequence_win_x64;
                    crf_tagger_ma_addNgramSequence          = crf_tagger_ma_addNgramSequence_win_x64;
                    crf_tagger_ma_endAddNgramSequence       = crf_tagger_ma_endAddNgramSequence_win_x64;
                    crf_tagger_ma_setNgramValue             = crf_tagger_ma_setNgramValue_win_x64;
                    crf_tagger_ma_tagNgram_with_probability = crf_tagger_ma_tagNgram_with_probability_win_x64;
                    crf_tagger_ma_getResultValue            = crf_tagger_ma_getResultValue_win_x64;
                    crf_tagger_ma_uninitialize              = crf_tagger_ma_uninitialize_win_x64;

                    #region [.learner. not used.]
                    /*
                    crf_learner_initialize            = crf_learner_initialize_win_x64;
                    crf_learner_beginAddItemSequence  = crf_learner_beginAddItemSequence_win_x64;
                    crf_learner_beginAddItemAttribute = crf_learner_beginAddItemAttribute_win_x64;
                    crf_learner_addItemAttribute      = crf_learner_addItemAttribute_win_x64;
                    crf_learner_endAddItemAttribute   = crf_learner_endAddItemAttribute_win_x64;
                    crf_learner_endAddItemSequence    = crf_learner_endAddItemSequence_win_x64;
                    crf_learner_beginAddStringList    = crf_learner_beginAddStringList_win_x64;
                    crf_learner_addString             = crf_learner_addString_win_x64;
                    crf_learner_endAddStringList      = crf_learner_endAddStringList_win_x64;
                    crf_learner_append                = crf_learner_append_win_x64;
                    crf_learner_train                 = crf_learner_train_win_x64;
                    crf_learner_uninitialize          = crf_learner_uninitialize_win_x64;
                    */
                    #endregion
                }
                else
                {
                    crf_tagger_initialize               = crf_tagger_initialize_win_x86;
                    crf_tagger_beginAddItemSequence     = crf_tagger_beginAddItemSequence_win_x86;
                    crf_tagger_beginAddItemAttribute    = crf_tagger_beginAddItemAttribute_win_x86;
                    crf_tagger_addItemAttribute         = crf_tagger_addItemAttribute_win_x86;
                    crf_tagger_addItemAttributeNameOnly = crf_tagger_addItemAttributeNameOnly_win_x86;
                    crf_tagger_endAddItemAttribute      = crf_tagger_endAddItemAttribute_win_x86;
                    crf_tagger_endAddItemSequence       = crf_tagger_endAddItemSequence_win_x86;
                    crf_tagger_tag                      = crf_tagger_tag_win_x86;
                    crf_tagger_tag_with_probability     = crf_tagger_tag_with_probability_win_x86;
                    crf_tagger_tag_with_marginal        = crf_tagger_tag_with_marginal_win_x86;
                    crf_tagger_getResultLength          = crf_tagger_getResultLength_win_x86;
                    crf_tagger_getResultValue           = crf_tagger_getResultValue_win_x86;
                    crf_tagger_getResultMarginal        = crf_tagger_getResultMarginal_win_x86;
                    crf_tagger_uninitialize             = crf_tagger_uninitialize_win_x86;

                    crf_tagger_ma_initialize                = crf_tagger_ma_initialize_win_x86;
                    crf_tagger_ma_beginAddNgramSequence     = crf_tagger_ma_beginAddNgramSequence_win_x86;
                    crf_tagger_ma_addNgramSequence          = crf_tagger_ma_addNgramSequence_win_x86;
                    crf_tagger_ma_endAddNgramSequence       = crf_tagger_ma_endAddNgramSequence_win_x86;
                    crf_tagger_ma_setNgramValue             = crf_tagger_ma_setNgramValue_win_x86;
                    crf_tagger_ma_tagNgram_with_probability = crf_tagger_ma_tagNgram_with_probability_win_x86;
                    crf_tagger_ma_getResultValue            = crf_tagger_ma_getResultValue_win_x86;
                    crf_tagger_ma_uninitialize              = crf_tagger_ma_uninitialize_win_x86;

                    #region [.learner. not used.]
                    /*
                    crf_learner_initialize            = crf_learner_initialize_win_x86;
                    crf_learner_beginAddItemSequence  = crf_learner_beginAddItemSequence_win_x86;
                    crf_learner_beginAddItemAttribute = crf_learner_beginAddItemAttribute_win_x86;
                    crf_learner_addItemAttribute      = crf_learner_addItemAttribute_win_x86;
                    crf_learner_endAddItemAttribute   = crf_learner_endAddItemAttribute_win_x86;
                    crf_learner_endAddItemSequence    = crf_learner_endAddItemSequence_win_x86;
                    crf_learner_beginAddStringList    = crf_learner_beginAddStringList_win_x86;
                    crf_learner_addString             = crf_learner_addString_win_x86;
                    crf_learner_endAddStringList      = crf_learner_endAddStringList_win_x86;
                    crf_learner_append                = crf_learner_append_win_x86;
                    crf_learner_train                 = crf_learner_train_win_x86;
                    crf_learner_uninitialize          = crf_learner_uninitialize_win_x86;
                    */
                    #endregion
                }
            }
		}
    }

    #region commented
    /*
    /// <summary>
    /// 
    /// </summary>
    internal interface IDllLoader 
    {
        IntPtr LoadLibrary( string fileName );
        void FreeLibrary( IntPtr handle );
        IntPtr GetProcAddress( IntPtr dllHandle, string name );
    }

    /// <summary>
    /// 
    /// </summary>
    internal class DllLoaderWindows : IDllLoader 
    {
        IntPtr IDllLoader.LoadLibrary( string fileName ) 
        {
            return LoadLibrary(fileName);
        }
        void IDllLoader.FreeLibrary( IntPtr handle ) 
        {
            FreeLibrary(handle);
        }
        IntPtr IDllLoader.GetProcAddress( IntPtr dllHandle, string name )
        {
            return GetProcAddress(dllHandle, name);
        }

        [DllImport("kernel32")]
        private static extern IntPtr LoadLibrary(string fileName);

        [DllImport("kernel32.dll")]
        private static extern int FreeLibrary(IntPtr handle);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress (IntPtr handle, string procedureName);
    }

    /// <summary>
    /// 
    /// </summary>
    internal class DllLoaderLinux : IDllLoader 
    {
        IntPtr IDllLoader.LoadLibrary( string fileName ) 
        {
            return dlopen(fileName, RTLD_NOW);
        }
        void IDllLoader.FreeLibrary( IntPtr handle ) 
        {
            dlclose(handle);
        }
        IntPtr IDllLoader.GetProcAddress( IntPtr dllHandle, string name )
        {
            // clear previous errors if any
            dlerror();
            var res = dlsym(dllHandle, name);
            var errPtr = dlerror();
            if (errPtr != IntPtr.Zero) {
                throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(errPtr));
            }
            return res;
        }

        private const int RTLD_NOW = 2;

        [DllImport("libdl.so")]
        private static extern IntPtr dlopen(String fileName, int flags);
        
        [DllImport("libdl.so")]
        private static extern IntPtr dlsym(IntPtr handle, String symbol);

        [DllImport("libdl.so")]
        private static extern int dlclose(IntPtr handle);

        [DllImport("libdl.so")]
        private static extern IntPtr dlerror();
    }

    //Separating Linux and Windows implementations in different classes ensures that CLR won't try to load Linux implementation on windows and vice versa.
    //Now we can use this code to dynamically load library:

    /// <summary>
    /// 
    /// </summary>
    internal static class __native__
    {
        private static bool IsLinux()
        {
            var p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }
        private static bool Isx64()
        {
            return (IntPtr.Size == 8);
        }
        private static string GetNativeDLLPath()
		{
            return (Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? string.Empty));
		}

        private const string DLL_NAME_x64    = "crf_x64";
        private const string DLL_NAME_x86    = "crf_x86";
        private const string DLL_EXT_WINDOWS = ".dll";
        private const string DLL_EXT_LINUX   = ".so";
        private static IntPtr DLL_HANDLE     = IntPtr.Zero;
        private static readonly object _Lock = new object();

        private const string crf_tagger_initialize_name            = "crf_tagger_initialize";
        private const string crf_tagger_beginAddItemSequence_name  = "crf_tagger_beginAddItemSequence";
        private const string crf_tagger_beginAddItemAttribute_name = "crf_tagger_beginAddItemAttribute";
        private const string crf_tagger_addItemAttribute_name      = "crf_tagger_addItemAttribute";
        private const string crf_tagger_endAddItemAttribute_name   = "crf_tagger_endAddItemAttribute";
        private const string crf_tagger_endAddItemSequence_name    = "crf_tagger_endAddItemSequence";
        private const string crf_tagger_tag_name                   = "crf_tagger_tag";
        private const string crf_tagger_getResultLength_name       = "crf_tagger_getResultLength";
        private const string crf_tagger_getResultValue_name        = "crf_tagger_getResultValue";
        private const string crf_tagger_uninitialize_name          = "crf_tagger_uninitialize";

        internal delegate IntPtr crf_tagger_initialize_Delegate( IntPtr name );
        internal delegate void   crf_tagger_beginAddItemSequence_Delegate( IntPtr taggerWrapper );
        internal delegate void   crf_tagger_beginAddItemAttribute_Delegate( IntPtr taggerWrapper );
        internal delegate bool   crf_tagger_addItemAttribute_Delegate( IntPtr taggerWrapper, string name, double val );
        internal delegate void   crf_tagger_endAddItemAttribute_Delegate( IntPtr taggerWrapper );
        internal delegate void   crf_tagger_endAddItemSequence_Delegate( IntPtr taggerWrapper );
        internal delegate void   crf_tagger_tag_Delegate( IntPtr taggerWrapper );
        internal delegate uint crf_tagger_getResultLength_Delegate( IntPtr taggerWrapper );
        internal delegate string crf_tagger_getResultValue_Delegate( IntPtr taggerWrapper, uint index );
        internal delegate void   crf_tagger_uninitialize_Delegate( IntPtr taggerWrapper );

        internal static crf_tagger_initialize_Delegate            crf_tagger_initialize;
        internal static crf_tagger_beginAddItemSequence_Delegate  crf_tagger_beginAddItemSequence;
        internal static crf_tagger_beginAddItemAttribute_Delegate crf_tagger_beginAddItemAttribute;
        internal static crf_tagger_addItemAttribute_Delegate      crf_tagger_addItemAttribute;
        internal static crf_tagger_endAddItemAttribute_Delegate   crf_tagger_endAddItemAttribute;
        internal static crf_tagger_endAddItemSequence_Delegate    crf_tagger_endAddItemSequence;
        internal static crf_tagger_tag_Delegate                   crf_tagger_tag;
        internal static crf_tagger_getResultLength_Delegate       crf_tagger_getResultLength;
        internal static crf_tagger_getResultValue_Delegate        crf_tagger_getResultValue;
        internal static crf_tagger_uninitialize_Delegate          crf_tagger_uninitialize;

		internal static void load_native_crf_suite()
		{
			if ( DLL_HANDLE != IntPtr.Zero )
			{
				return;
			}

            lock ( _Lock )
            {
			    if ( DLL_HANDLE != IntPtr.Zero )
			    {
				    return;
			    }

                var dllLoader = default(IDllLoader);
			    try
			    {                
                    var libraryName  = default(string);
                    var isLinux      = IsLinux();
                    var isx64        = Isx64();

                    if ( isLinux )
                    {
                        dllLoader   = new DllLoaderLinux();
                        libraryName = (isx64 ? DLL_NAME_x64 : DLL_NAME_x86) + DLL_EXT_LINUX;
                    } 
                    else 
                    {
                        dllLoader   = new DllLoaderWindows();
                        libraryName = (isx64 ? DLL_NAME_x64 : DLL_NAME_x86) + DLL_EXT_WINDOWS;
                    }

                    libraryName = Path.Combine( GetNativeDLLPath(), libraryName );
                    DLL_HANDLE = dllLoader.LoadLibrary( libraryName );
				    if ( DLL_HANDLE == IntPtr.Zero )
				    {
					    throw (new DllNotFoundException( "Dll/So not found: '" + libraryName + '\'' ));
				    }

                    crf_tagger_initialize            = (crf_tagger_initialize_Delegate)            dllLoader.GetDelegate( DLL_HANDLE, crf_tagger_initialize_name           , typeof(crf_tagger_initialize_Delegate) );
                    crf_tagger_beginAddItemSequence  = (crf_tagger_beginAddItemSequence_Delegate)  dllLoader.GetDelegate( DLL_HANDLE, crf_tagger_beginAddItemSequence_name , typeof(crf_tagger_beginAddItemSequence_Delegate) );
                    crf_tagger_beginAddItemAttribute = (crf_tagger_beginAddItemAttribute_Delegate) dllLoader.GetDelegate( DLL_HANDLE, crf_tagger_beginAddItemAttribute_name, typeof(crf_tagger_beginAddItemAttribute_Delegate) );
                    crf_tagger_addItemAttribute      = (crf_tagger_addItemAttribute_Delegate)      dllLoader.GetDelegate( DLL_HANDLE, crf_tagger_addItemAttribute_name     , typeof(crf_tagger_addItemAttribute_Delegate) );
                    crf_tagger_endAddItemAttribute   = (crf_tagger_endAddItemAttribute_Delegate)   dllLoader.GetDelegate( DLL_HANDLE, crf_tagger_endAddItemAttribute_name  , typeof(crf_tagger_endAddItemAttribute_Delegate) );
                    crf_tagger_endAddItemSequence    = (crf_tagger_endAddItemSequence_Delegate)    dllLoader.GetDelegate( DLL_HANDLE, crf_tagger_endAddItemSequence_name   , typeof(crf_tagger_endAddItemSequence_Delegate) );
                    crf_tagger_tag                   = (crf_tagger_tag_Delegate)                   dllLoader.GetDelegate( DLL_HANDLE, crf_tagger_tag_name                  , typeof(crf_tagger_tag_Delegate) );
                    crf_tagger_getResultLength       = (crf_tagger_getResultLength_Delegate)       dllLoader.GetDelegate( DLL_HANDLE, crf_tagger_getResultLength_name      , typeof(crf_tagger_getResultLength_Delegate) );
                    crf_tagger_getResultValue        = (crf_tagger_getResultValue_Delegate)        dllLoader.GetDelegate( DLL_HANDLE, crf_tagger_getResultValue_name       , typeof(crf_tagger_getResultValue_Delegate) );
                    crf_tagger_uninitialize          = (crf_tagger_uninitialize_Delegate)          dllLoader.GetDelegate( DLL_HANDLE, crf_tagger_uninitialize_name         , typeof(crf_tagger_uninitialize_Delegate) );
			    }
			    catch ( Exception )
			    {
				    if ( DLL_HANDLE != IntPtr.Zero && dllLoader != null )
				    {
					    dllLoader.FreeLibrary( DLL_HANDLE );
				    }
				    throw;
			    }
            }
		}
    }
    
    /// <summary>
    /// 
    /// </summary>
    internal static class Ext
    {
		public static Delegate GetDelegate( this IDllLoader dllLoader, IntPtr dllHandle, string procName, Type delegateType )
		{
			IntPtr procAddress = dllLoader.GetProcAddress( dllHandle, procName );
			if (procAddress == IntPtr.Zero)
			{
				throw (new EntryPointNotFoundException("Function: " + procName));
			}
			return (Marshal.GetDelegateForFunctionPointer( procAddress, delegateType ));
		}
    }
    */
    #endregion
}
