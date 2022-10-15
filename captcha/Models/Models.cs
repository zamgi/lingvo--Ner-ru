namespace captcha
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CaptchaVM
    {
        public int    WaitRemainSeconds    { get; init; }
        public string AllowContinueUrl     { get; init; } = "/"; // "/index.html";
        public string CaptchaImageUniqueId { get; init; }
        public string CaptchaPageTitle     { get; init; }

        public string ErrorMessage { get; init; }
        public bool HasError => (ErrorMessage != null);
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class ProcessCaptchaVM
    {
        public string CaptchaUserText      { get; set; }
        public string CaptchaImageUniqueId { get; set; }
        public string RedirectLocation     { get; set; } // = "~/index.html";
    }
}
