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
        public static Dummy_NerInputTypeProcessor Inst { get; } = new Dummy_NerInputTypeProcessor();
        private Dummy_NerInputTypeProcessor() { }

        public unsafe NerInputType GetNerInputType( char* _base, int length ) => NerInputType.O;
    }
}
