using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Mime;
using System.Runtime.Caching;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using BackgroundNoiseLevel = captcha.CaptchaImage.BackgroundNoiseLevel;
using FontWarpFactor = captcha.CaptchaImage.FontWarpFactor;
using LineNoiseLevel = captcha.CaptchaImage.LineNoiseLevel;

namespace captcha
{
    /// <summary>
    /// 
    /// </summary>
    public static class CaptchaProcessor
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly struct CreateNewCaptchaParams
        {
            [DefaultValue(90), Description("Maximum number of seconds CAPTCHA will be cached and valid. If you're too slow, you may be a CAPTCHA hack attempt. Set to zero to disable.")]
            public int TimeoutSecondsMax { get; init; }


            [DefaultValue(3), Description("Minimum number of seconds CAPTCHA must be displayed before it is valid. If you're too fast, you must be a robot. Set to zero to disable.")]
            public int TimeoutSecondsMin { get; init; }


            [DefaultValue(typeof(BackgroundNoiseLevel), "Low"), Description("Amount of background noise to generate in the CAPTCHA image")]
            public BackgroundNoiseLevel CaptchaBackgroundNoise { get; init; }


            [DefaultValue(typeof(FontWarpFactor), "Low"), Description("Amount of random font warping used on the CAPTCHA text")]
            public FontWarpFactor FontWarp { get; init; }


            [DefaultValue(typeof(LineNoiseLevel), "None"), Description("Add line noise to the CAPTCHA image")]
            public LineNoiseLevel LineNoise { get; init; }


            [DefaultValue(100), Description("Height of generated CAPTCHA image.")]
            public int Height { get; init; }


            [DefaultValue(300), Description("Width of generated CAPTCHA image.")]
            public int Width { get; init; }


            [DefaultValue(""), Description("Font used to render CAPTCHA text. If font name is blank, a random font will be chosen.")]
            public string Font { get; init; }


            [DefaultValue("ABCDEFGHJKLMNPQRSTUVWXYZ23456789"), Description("Characters used to render CAPTCHA text. A character will be picked randomly from the string.")]
            public string TextChars { get; init; }

            [DefaultValue(5), Description("Number of CaptchaChars used in the CAPTCHA text")]
            public int TextLength { get; init; }

            [DefaultValue(false), Description("Ignore case when compare CAPTCHA image text.")]
            public bool CheckIgnoreCase { get; init; }

            public Color BackColor { get; init; }
            public Color FontColor { get; init; }
            public Color LineColor { get; init; }
            public Color NoiseColor { get; init; }

            public static CreateNewCaptchaParams CreateDefault() => new CreateNewCaptchaParams()
            {
                TimeoutSecondsMax = 90,
                TimeoutSecondsMin = 3,
                BackColor         = Color.White,
                Font       = string.Empty,
                FontColor         = Color.Black,
                LineColor         = Color.Black,
                NoiseColor        = Color.Black,

                CheckIgnoreCase = false,
                Width           = 300, //180,
                Height          = 100, //60,
                LineNoise       = LineNoiseLevel.None,
                FontWarp        = FontWarpFactor.Low,
                TextChars       = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789",
                TextLength      = 5,
            };                
        }
        public static string CreateNew( in CreateNewCaptchaParams? pp = null )
        {
            var p = (pp.HasValue ? pp.Value : CreateNewCaptchaParams.CreateDefault());

            var captchaImage = new CaptchaImage()
            {
                BackColor   = p.BackColor,
                Font        = p.Font,
                FontColor   = p.FontColor,
                LineColor   = p.LineColor,
                NoiseColor  = p.NoiseColor,

                Width       = p.Width,
                Height      = p.Height,
                LineNoise   = p.LineNoise,
                FontWarp    = p.FontWarp,
                TextChars   = p.TextChars,
                TextLength  = p.TextLength,
            };

            MemoryCache.Default.Add( captchaImage.UniqueId, captchaImage, new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds( p.TimeoutSecondsMax ),
                SlidingExpiration  = TimeSpan.Zero,
                Priority           = CacheItemPriority.NotRemovable,
                //RemovedCallback    = e => { System.Diagnostics.Debug.WriteLine( e ); },
                //UpdateCallback     = e => { System.Diagnostics.Debug.WriteLine( e ); },
            });

            return (captchaImage.UniqueId);
        }

        /// <summary>
        /// 
        /// </summary>
        public readonly struct ValidateCaptchaParams
        {
            public string CaptchaImageUniqueId { get; init; }
            public string CaptchaUserText      { get; init; }

            public bool CaptchaIgnoreCase { get; init; }
            public int? TimeoutSecondsMin { get; init; }
        }
        public static bool Validate( in ValidateCaptchaParams p, out string errorMessage )
        {
            if ( p.CaptchaUserText.IsNullOrWhiteSpace() )
            {
                errorMessage = "Empty user input text";
                return (false);
            }
            if ( p.CaptchaImageUniqueId.IsNullOrWhiteSpace() )
            {
                errorMessage = "Bad input params";
                return (false);
            }

            if ( !(MemoryCache.Default.Get( p.CaptchaImageUniqueId ) is CaptchaImage captchaImage) )
            {
                errorMessage = "The code was entered too long, its period has expired.";
                               //$"The code you typed has expired after {CaptchaMaxTimeout} seconds.";
                return (false);
            }

            MemoryCache.Default.Remove( p.CaptchaImageUniqueId );

            var timeoutSecondsMin = p.TimeoutSecondsMin.GetValueOrDefault( 3 );
            if ( (0 < timeoutSecondsMin) && (DateTime.Now < captchaImage.RenderedAt.AddSeconds( timeoutSecondsMin )) )
            {
                errorMessage = $"Code was typed too quickly. Wait at least {timeoutSecondsMin} seconds.";
                return (false);
            }
            if ( string.Compare( p.CaptchaUserText, captchaImage.Text, p.CaptchaIgnoreCase ) != 0 )
            {
                errorMessage = "The code you typed does not match the code in the image.";
                return (false);
            }

            errorMessage = default;
            return (true);
        }

        public static bool TryGetImage( HttpContext context, out byte[] bytes, out string contentType )
        {
            var key = (string) context.Request.Query[ "guid" ];
            if ( !key.IsNullOrWhiteSpace() && (MemoryCache.Default.Get( key ) is CaptchaImage captchaImage) )
            {
                using ( var ms = new MemoryStream() )
                {
                    using ( var bitmap = captchaImage.RenderImage() )
                    {
                        bitmap.Save( ms, ImageFormat.Jpeg );
                    }

                    contentType = MediaTypeNames.Image.Jpeg; // "image/jpeg";
                    bytes = ms.ToArray();
                    return (true);
                }
            }

            contentType = default;
            bytes = default;
            return (false);
        }


        public static IMvcBuilder AddCaptchaController( this IMvcBuilder mvcBuilder ) => mvcBuilder.AddApplicationPart( typeof(CaptchaController).Assembly ).AddControllersAsServices();
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        public static bool IsNullOrWhiteSpace( this string s ) => string.IsNullOrWhiteSpace( s );
    }
}
