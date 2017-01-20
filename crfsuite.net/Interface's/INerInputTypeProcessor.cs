using System;

using lingvo.core;

namespace lingvo.ner
{
    /// <summary>
    /// 
    /// </summary>
    public interface INerInputTypeProcessor
    {
        unsafe NerInputType GetNerInputType( char* _base, int length );
    }

    /// <summary>
    /// 
    /// </summary>
    public interface INerInputTypeProcessorFactory
    {
        INerInputTypeProcessor CreateInstance();
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class Dummy_NerInputTypeProcessor : INerInputTypeProcessor
    {
        public static readonly Dummy_NerInputTypeProcessor Instance = new Dummy_NerInputTypeProcessor();
        private Dummy_NerInputTypeProcessor() { }

        public unsafe NerInputType GetNerInputType( char* _base, int length )
        {
            return (NerInputType.O);
        }
    }
}
