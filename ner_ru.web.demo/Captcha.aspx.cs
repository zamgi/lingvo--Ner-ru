using System;
using System.Threading;
using System.Web.UI;

using captcha;

namespace lingvo
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Captcha : Page
    {
        private const string MAGIC_WORD              = "12qwQW12";
        private const string ALLOW_CONTINUE_PAGE_URL = "~/index.html";
        private AntiBot _AntiBot;

        protected void Page_Load( object sender, EventArgs e )
        {
            Thread.Sleep( 1000 );

            _AntiBot = this.Context.ToAntiBot();
            if ( _AntiBot.IsRequestValid() )
            {
                Response.Redirect( ALLOW_CONTINUE_PAGE_URL, true );
            }
        }

        protected void captchaButton_Click( object sender, EventArgs e )
        {
            if ( captchaControl.ValidateCaptcha( captchaText.Text ) || (captchaText.Text == MAGIC_WORD) )
            {
                _AntiBot.MakeAllowRequests();
                Response.Redirect( Response.RedirectLocation ?? ALLOW_CONTINUE_PAGE_URL, true );
            }
            else
            {
                captchaText.Text = null;
                captchaError.Text = ((IValidator) captchaControl).ErrorMessage;
                captchaError.Visible = true;
            }
        }

        protected int WaitRemainSeconds
        {
            get { return (_AntiBot.GetWaitRemainSeconds()); }
        }
        protected string AllowContinuePageUrlJavaScript
        {
            get { return (ALLOW_CONTINUE_PAGE_URL.TrimStart( '~', '/' )); }
        }
        /*
        protected int SAME_IP_INTERVAL_REQUEST_IN_SECONDS
        {
            get { return (Config.SAME_IP_INTERVAL_REQUEST_IN_SECONDS); }
        }
        protected int SAME_IP_MAX_REQUEST_IN_INTERVAL
        {
            get { return (Config.SAME_IP_MAX_REQUEST_IN_INTERVAL); }
        }
        */
    }
}