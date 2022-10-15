using System;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif

using captcha;

namespace lingvo.ner.webService.Controllers
{
    public sealed class ProcessController : Controller
    {
        #region [.ctor().]
        private readonly ConcurrentFactory _ConcurrentFactory;
#if DEBUG
        private readonly ILogger< ProcessController > _Logger;
#endif
#if DEBUG
        public ProcessController( ConcurrentFactory concurrentFactory, ILogger< ProcessController > logger )
        {
            _ConcurrentFactory = concurrentFactory;
            _Logger            = logger;
        }
#else
        public ProcessController( ConcurrentFactory concurrentFactory ) => _ConcurrentFactory = concurrentFactory;
#endif
        #endregion

        [HttpPost] public async Task< IActionResult > Run( [FromBody] InitParamsVM m )
        {
            try
            {
                #region [.anti-bot.]
                var antiBot = HttpContext.ToAntiBot( _ConcurrentFactory.Config );
                if ( antiBot.IsNeedRedirectOnCaptchaIfRequestNotValid() )
                {
                    return Json( AntiBot.CreateGotoOnCaptchaResponseObj() );
                }
                #endregion

                #region [.anti-bot.]
                antiBot.MarkRequestEx( m.Text );
                #endregion
#if DEBUG
                _Logger.LogInformation( $"start process: '{m.Text}'..." );
#endif
                //var sents = await _ConcurrentFactory.Run_Details( m.Text, m.SplitBySmiles );
                //var result = new ResultVM( m, sents );
                var words = await _ConcurrentFactory.Run( m.Text, m.SplitBySmiles );
                var result = new ResultVM( m, words );
#if DEBUG
                _Logger.LogInformation( $"end process: '{m.Text}'." );
#endif
                return Ok( result );
            }
            catch ( Exception ex )
            {
#if DEBUG
                _Logger.LogError( $"Error while process: '{m.Text}' => {ex}" );
#endif
                return Ok( new ResultVM( m, ex ) );
            }
        }
    }
}
