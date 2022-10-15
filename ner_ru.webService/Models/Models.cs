using System;
using System.Collections.Generic;
using System.Linq;

using lingvo.tokenizing;
using JP = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace lingvo.ner.webService
{
    /// <summary>
    /// 
    /// </summary>
    public struct InitParamsVM
    {
        public string Text          { get; set; }
        public bool   SplitBySmiles { get; set; }
#if DEBUG
        public override string ToString() => Text;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    internal readonly struct ResultVM
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly struct word_info
        {
            [JP("i")]   public int    startIndex { get; init; }
            [JP("l")]   public int    length     { get; init; }            
            [JP("ner")] public string ner        { get; init; }
            //[JP("v")]   public string value      { get; init; }
        }

        public ResultVM( in InitParamsVM m, Exception ex ) : this() => (init_params, exception_message) = (m, ex.Message);
        public ResultVM( in InitParamsVM m, List< word_t[] > sents ) : this()
        {
            init_params = m;

            //var buf = new StringBuilder();
            wordInfos = (from words in sents
                         from word in words
                        where (!word.HasNerPrevWord)
                        select
                            new word_info()
                            {
                                startIndex = word.startIndex,
                                length     = word.GetNerLength(),
                                ner        = word.nerOutputType.ToString(),
                                //value      = word.GetNerValue( buf ),
                            }
                        ).ToArray();

            sentInfos = new List< word_info[] >( sents.Count );
            foreach ( var words_by_sent in sents )
            {
                var words = (from word in words_by_sent
                                select
                                    new word_info()
                                    {
                                        startIndex = word.startIndex,
                                        length     = word.GetNerLength(),
                                        ner        = word.nerOutputType.ToString(),
                                        //value      = word.GetNerValue( buf ),
                                    }
                            ).ToArray();
                sentInfos.Add( words );
            }
        }
        public ResultVM( in InitParamsVM m, List< word_t > words ) : this()
        {
            init_params = m;

            //var buf = new StringBuilder();
            wordInfos = (from word in words
                        where (!word.HasNerPrevWord)
                        select
                            new word_info()
                            {
                                startIndex = word.startIndex,
                                length     = word.GetNerLength(),
                                ner        = word.nerOutputType.ToString(),
                                //value      = word.GetNerValue( buf ),
                            }
                        ).ToList();
        }

        [JP("ip")   ] public InitParamsVM        init_params       { get; }
        [JP("sents")] public List< word_info[] > sentInfos         { get; }
        [JP("words")] public IList< word_info >  wordInfos         { get; }
        [JP("err") ] public string               exception_message { get; }
    }
}
