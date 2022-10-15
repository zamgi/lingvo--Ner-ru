using System.Configuration;

using captcha;

namespace lingvo.ner.webService
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class Config : NerEnvironmentConfigImpl, IAntiBotConfig
    {
        public Config() { }

        public int? SameIpBannedIntervalInSeconds  { get; } = int.TryParse( ConfigurationManager.AppSettings[ "SAME_IP_BANNED_INTERVAL_IN_SECONDS"  ], out var i ) ? i : null;
        public int? SameIpIntervalRequestInSeconds { get; } = int.TryParse( ConfigurationManager.AppSettings[ "SAME_IP_INTERVAL_REQUEST_IN_SECONDS" ], out var i ) ? i : null;
        public int? SameIpMaxRequestInInterval     { get; } = int.TryParse( ConfigurationManager.AppSettings[ "SAME_IP_MAX_REQUEST_IN_INTERVAL"     ], out var i ) ? i : null;
        public string CaptchaPageTitle => "Определение именованных сущностей (NER) в тексте на русском языке";


        public int CONCURRENT_FACTORY_INSTANCE_COUNT { get; } = int.Parse( ConfigurationManager.AppSettings[ "CONCURRENT_FACTORY_INSTANCE_COUNT" ] );
        //public int MAX_INPUTTEXT_LENGTH { get; } = ConfigurationManager.AppSettings[ "MAX_INPUTTEXT_LENGTH" ].ToInt32();
    }
}
