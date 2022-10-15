using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace captcha
{
    public sealed class CaptchaController : Controller
    {
        #region [.ctor().]
        private const string CAPTCHA_PAGE_LOCATION = "~/Views/Captcha.cshtml";

        private readonly IAntiBotConfig _Config;
        public CaptchaController( IAntiBotConfig config ) => _Config = config;
        #endregion

        [HttpGet] public async Task< IActionResult > GetNew()
        {
            await Task.Delay( 1_000 );

            var antiBot = HttpContext.ToAntiBot( _Config );
            if ( antiBot.IsRequestValid() )
            {
                return Redirect( "~/" );
            }

            var m = new CaptchaVM()
            {
                WaitRemainSeconds    = antiBot.GetWaitRemainSeconds(),
                CaptchaImageUniqueId = CaptchaProcessor.CreateNew(),
                CaptchaPageTitle     = _Config.CaptchaPageTitle,
            };
            return View( CAPTCHA_PAGE_LOCATION, m );
        }
        [HttpGet] public IActionResult GetImage() => CaptchaProcessor.TryGetImage( HttpContext, out var bytes, out var contentType ) ? File( bytes, contentType ) : NotFound();
        [HttpPost] public async Task< IActionResult > Process( [FromForm] ProcessCaptchaVM m )
        {
            const string MAGIC_WORD = "12qwQW12";

            await Task.Delay( 1_000 );

            var p = new CaptchaProcessor.ValidateCaptchaParams()
            {
                CaptchaImageUniqueId = m.CaptchaImageUniqueId,
                CaptchaUserText      = m.CaptchaUserText,
            };
            var antiBot = HttpContext.ToAntiBot( _Config );
            if ( CaptchaProcessor.Validate( p, out var errorMessage ) || (m.CaptchaUserText == MAGIC_WORD) )
            {
                antiBot.MakeAllowRequests();
                return Redirect( m.RedirectLocation ?? "~/" );
            }

            var resp_model = new CaptchaVM()
            {
                WaitRemainSeconds    = antiBot.GetWaitRemainSeconds(),                    
                CaptchaImageUniqueId = CaptchaProcessor.CreateNew(), //-- OR SAME IMAGE (NEED TURN-OFF Removing Him from Cache when Bad check-attempt)-- //CaptchaImageUniqueId = m.CaptchaImageUniqueId,
                ErrorMessage         = errorMessage,
            };
            return View( CAPTCHA_PAGE_LOCATION, resp_model );
        }

        /*[HttpGet] public async Task< IActionResult > _Test_()
        {
            await Task.Delay( 1_000 );

            var m = new CaptchaVM()
            {
                WaitRemainSeconds    = HttpContext.ToAntiBot( _Config ).GetWaitRemainSeconds(),
                CaptchaImageUniqueId = CaptchaProcessor.CreateNew(),
            };
            return View( CAPTCHA_PAGE_LOCATION, m );
        }*/
    }
}
