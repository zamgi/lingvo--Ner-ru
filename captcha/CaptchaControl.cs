using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;

using CaptchaImage         = captcha.CaptchaImage;
using BackgroundNoiseLevel = captcha.CaptchaImage.BackgroundNoiseLevel;
using FontWarpFactor       = captcha.CaptchaImage.FontWarpFactor;
using LineNoiseLevel       = captcha.CaptchaImage.LineNoiseLevel;

namespace captcha
{
    /// <summary>
    /// 
    /// </summary>
    [DefaultProperty("Text")]
    public class CaptchaControl : WebControl, INamingContainer, IPostBackDataHandler, IValidator
    {
        /// <summary>
        /// 
        /// </summary>
        public enum CacheType
        {
            HttpRuntime,
            Session
        }

        public const string CAPTCHA_IMAGE_HANDLER_URL = "CaptchaImage.axd";

        // Fields
        private Color        _BackColor = Color.White;
        private CacheType    _CacheStrategy;
        private CaptchaImage _CaptchaImage = new CaptchaImage();
        private string       _ErrorMessage = string.Empty;
        private string       _Font = string.Empty;
        private Color        _FontColor = Color.Black;
        private Color        _LineColor = Color.Black;
        private Color        _NoiseColor = Color.Black;
        private string       _PrevGuid;        
        private int          _TimeoutSecondsMax = 90;
        private int          _TimeoutSecondsMin = 3;
        private bool         _UserValidated = true;
        private string       _CustomValidatorErrorMessage;
        private string       _ValidationGroup;
        //private string       _Text = "Enter the code shown:";

        public CaptchaControl()
        {
            CaptchaImageHandlerUrl = CAPTCHA_IMAGE_HANDLER_URL;
        }

        // Methods
        private string CssStyle()
        {
            var builder = new StringBuilder();
            builder.Append( " style='" );
            if ( this.BorderWidth.ToString().Length > 0 )
            {
                builder.Append( "border-width:" );
                builder.Append( this.BorderWidth.ToString() );
                builder.Append( ";" );
            }
            if ( this.BorderStyle != BorderStyle.NotSet )
            {
                builder.Append( "border-style:" );
                builder.Append( this.BorderStyle.ToString() );
                builder.Append( ";" );
            }
            string str = this.HtmlColor( this.BorderColor );
            if ( str.Length > 0 )
            {
                builder.Append( "border-color:" );
                builder.Append( str );
                builder.Append( ";" );
            }
            str = this.HtmlColor( this.BackColor );
            if ( str.Length > 0 )
            {
                builder.Append( "background-color:" + str + ";" );
            }
            str = this.HtmlColor( this.ForeColor );
            if ( str.Length > 0 )
            {
                builder.Append( "color:" + str + ";" );
            }
            if ( this.Font.Bold )
            {
                builder.Append( "font-weight:bold;" );
            }
            if ( this.Font.Italic )
            {
                builder.Append( "font-style:italic;" );
            }
            if ( this.Font.Underline )
            {
                builder.Append( "text-decoration:underline;" );
            }
            if ( this.Font.Strikeout )
            {
                builder.Append( "text-decoration:line-through;" );
            }
            if ( this.Font.Overline )
            {
                builder.Append( "text-decoration:overline;" );
            }
            if ( this.Font.Size.ToString().Length > 0 )
            {
                builder.Append( "font-size:" + this.Font.Size.ToString() + ";" );
            }
            if ( this.Font.Names.Length > 0 )
            {
                builder.Append( "font-family:" );
                foreach ( string str2 in this.Font.Names )
                {
                    builder.Append( str2 );
                    builder.Append( "," );
                }
                builder.Length--;
                builder.Append( ";" );
            }
            if ( this.Height.ToString() != string.Empty )
            {
                builder.Append( "height:" + this.Height.ToString() + ";" );
            }
            if ( this.Width.ToString() != string.Empty )
            {
                builder.Append( "width:" + this.Width.ToString() + ";" );
            }
            builder.Append( "'" );
            if ( builder.ToString() == " style=''" )
            {
                return string.Empty;
            }
            return builder.ToString();
        }

        private void GenerateNewCaptcha()
        {
            if ( !this.IsDesignMode )
            {
                if ( this._CacheStrategy == CacheType.HttpRuntime )
                {
                    HttpRuntime.Cache.Add( this._CaptchaImage.UniqueId, this._CaptchaImage, null, DateTime.Now.AddSeconds( Convert.ToDouble( (this.CaptchaMaxTimeout == 0) ? 90 : this.CaptchaMaxTimeout ) ), TimeSpan.Zero, CacheItemPriority.NotRemovable, null );
                }
                else
                {
                    HttpContext.Current.Session.Add( this._CaptchaImage.UniqueId, this._CaptchaImage );
                }
            }
        }

        private CaptchaImage GetCachedCaptcha( string guid )
        {
            if ( this._CacheStrategy == CacheType.HttpRuntime )
            {
                return (CaptchaImage) HttpRuntime.Cache.Get( guid );
            }
            return (CaptchaImage) HttpContext.Current.Session[ guid ];
        }

        private string HtmlColor( Color color )
        {
            if ( color.IsEmpty )
            {
                return string.Empty;
            }
            if ( color.IsNamedColor )
            {
                return color.ToKnownColor().ToString();
            }
            if ( color.IsSystemColor )
            {
                return color.ToString();
            }
            return ("#" + color.ToArgb().ToString( "x" ).Substring( 2 ));
        }

        protected override void LoadControlState( object state )
        {
            if ( state != null )
            {
                this._PrevGuid = (string) state;
            }
        }
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.Page.RegisterRequiresControlState( this );
            this.Page.Validators.Add( this );
        }
        protected override void OnPreRender( EventArgs e )
        {
            if ( this.Visible )
            {
                this.GenerateNewCaptcha();
            }
            base.OnPreRender( e );
        }
        protected override void OnUnload( EventArgs e )
        {
            if ( this.Page != null )
            {
                this.Page.Validators.Remove( this );
            }
            base.OnUnload( e );
        }

        private void RemoveCachedCaptcha( string guid )
        {
            if ( this._CacheStrategy == CacheType.HttpRuntime )
            {
                HttpRuntime.Cache.Remove( guid );
            }
            else
            {
                HttpContext.Current.Session.Remove( guid );
            }
        }

        protected override void Render( HtmlTextWriter Output )
        {
            Output.Write( "<div" );
            if ( this.CssClass != string.Empty )
            {
                Output.Write( " class='" + this.CssClass + "'" );
            }
            Output.Write( this.CssStyle() );
            Output.Write( ">" );
            Output.Write( "<img src=\"" + this.CaptchaImageHandlerUrl );
            if ( !this.IsDesignMode )
            {
                Output.Write( "?guid=" + Convert.ToString( this._CaptchaImage.UniqueId ) );
            }
            if ( this.CacheStrategy == CacheType.Session )
            {
                Output.Write( "&s=1" );
            }
            Output.Write( "\" border='0'" );
            if ( this.ToolTip.Length > 0 )
            {
                Output.Write( " alt='" + this.ToolTip + "'" );
            }
            Output.Write( " width="  + this._CaptchaImage.Width );
            Output.Write( " height=" + this._CaptchaImage.Height );
            Output.Write( ">" );
            Output.Write( "</div>" );
        }
        protected override object SaveControlState()
        {
            return this._CaptchaImage.UniqueId;
        }

        bool IPostBackDataHandler.LoadPostData( string PostDataKey, NameValueCollection Values )
        {
            this.ValidateCaptcha( Convert.ToString( Values[ this.UniqueID ] ) );
            return false;
        }
        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
        }
        void IValidator.Validate()
        {
        }

        public bool ValidateCaptcha( string userEntry )
        {
            if ( !this.Visible | !this.Enabled )
            {
                this._UserValidated = true;
            }
            else
            {
                CaptchaImage cachedCaptcha = this.GetCachedCaptcha( this._PrevGuid );
                if ( cachedCaptcha == null )
                {
                    ((IValidator) this).ErrorMessage = "Код вводился слишком долго, его срок истек"; // после " + this.CaptchaMaxTimeout + " секунд.";
                    //"The code you typed has expired after " + this.CaptchaMaxTimeout + " seconds.";
                    this._UserValidated = false;
                }
                else if ( (this.CaptchaMinTimeout > 0) && (cachedCaptcha.RenderedAt.AddSeconds( (double) this.CaptchaMinTimeout ) > DateTime.Now) )
                {
                    this._UserValidated = false;
                    ((IValidator) this).ErrorMessage = "Код был введен слишком быстро. Ожидайте по крайней мере " + this.CaptchaMinTimeout + " секунд.";
                    //"Code was typed too quickly. Wait at least " + this.CaptchaMinTimeout + " seconds.";
                    this.RemoveCachedCaptcha( this._PrevGuid );
                }
                else if ( string.Compare( userEntry, cachedCaptcha.Text, this.CaptchaIgnoreCase ) != 0 )
                {
                    ((IValidator) this).ErrorMessage = "Код, который Вы ввели, не соответствует коду на изображении.";
                    //"The code you typed does not match the code in the image.";
                    this._UserValidated = false;
                    this.RemoveCachedCaptcha( this._PrevGuid );
                }
                else
                {
                    this._UserValidated = true;
                    this.RemoveCachedCaptcha( this._PrevGuid );
                }
            }

            if ( !this._UserValidated && string.IsNullOrEmpty( ((IValidator) this).ErrorMessage ) )
            {
                ((IValidator) this).ErrorMessage = "Введен неверный код.";
            }
            return (this._UserValidated);
        }

        /*private static CaptchaImage GetCachedCaptchaII( string captchaImageUniqueId )
        {
            return ((CaptchaImage) HttpRuntime.Cache.Get( captchaImageUniqueId ) ?? (CaptchaImage) HttpContext.Current.Session[ captchaImageUniqueId ]);
        }
        private static void RemoveCachedCaptchaII( string captchaImageUniqueId )
        {
            var obj = HttpRuntime.Cache.Remove( captchaImageUniqueId );
            if ( obj == null )
                HttpContext.Current.Session.Remove( captchaImageUniqueId );
        }
        public static Tuple< bool, string > ValidateCaptcha( string captchaImageUniqueId, string userEntry, int captchaMaxTimeout, int captchaMinTimeout )
        {
            var userValidated = false;
            var errorMessage = default( string );

            CaptchaImage cachedCaptcha = GetCachedCaptchaII( captchaImageUniqueId );
            if ( cachedCaptcha == null )
            {
                errorMessage = "Код, который Вы ввели, истек после " + captchaMaxTimeout + " секунд.";
                //"The code you typed has expired after " + captchaMaxTimeout + " seconds.";
                userValidated = false;
            }
            else if ( (captchaMinTimeout > 0) && (cachedCaptcha.RenderedAt.AddSeconds( (double) captchaMinTimeout ) > DateTime.Now) )
            {
                userValidated = false;
                errorMessage = "Код был введен слишком быстро. Ожидайте по крайней мере " + captchaMinTimeout + " секунд.";
                //"Code was typed too quickly. Wait at least " + captchaMinTimeout + " seconds.";
                RemoveCachedCaptchaII( captchaImageUniqueId );
            }
            else if ( string.Compare( userEntry, cachedCaptcha.Text, true ) != 0 )
            {
                errorMessage = "Код, который Вы ввели, не соответствует коду на изображении.";
                //"The code you typed does not match the code in the image.";
                userValidated = false;
                RemoveCachedCaptchaII( captchaImageUniqueId );
            }
            else
            {
                userValidated = true;
                RemoveCachedCaptchaII( captchaImageUniqueId );
            }
            return (new Tuple<bool, string>( userValidated, errorMessage ));
        }
        */

        // Properties
        public Color BackColor
        {
            get
            {
                return this._BackColor;
            }
            set
            {
                this._BackColor = value;
                this._CaptchaImage.BackColor = this._BackColor;
            }
        }

        [Description( "Determines if CAPTCHA codes are stored in HttpRuntime (fast, but local to current server) or Session (more portable across web farms)." ), Category( "Captcha" ), DefaultValue( typeof( CacheType ), "HttpRuntime" )]
        public CacheType CacheStrategy
        {
            get
            {
                return this._CacheStrategy;
            }
            set
            {
                this._CacheStrategy = value;
            }
        }

        [Category( "Captcha" ), DefaultValue( typeof(BackgroundNoiseLevel), "Low" ), Description( "Amount of background noise to generate in the CAPTCHA image" )]
        public BackgroundNoiseLevel CaptchaBackgroundNoise
        {
            get
            {
                return this._CaptchaImage.BackgroundNoise;
            }
            set
            {
                this._CaptchaImage.BackgroundNoise = value;
            }
        }

        [DefaultValue( "ABCDEFGHJKLMNPQRSTUVWXYZ23456789" ), Category( "Captcha" ), Description( "Characters used to render CAPTCHA text. A character will be picked randomly from the string." )]
        public string CaptchaChars
        {
            get
            {
                return this._CaptchaImage.TextChars;
            }
            set
            {
                this._CaptchaImage.TextChars = value;
            }
        }

        [Category( "Captcha" ), DefaultValue( "" ), Description( "Font used to render CAPTCHA text. If font name is blank, a random font will be chosen." )]
        public string CaptchaFont
        {
            get
            {
                return this._Font;
            }
            set
            {
                this._Font = value;
                this._CaptchaImage.Font = this._Font;
            }
        }

        [DefaultValue( typeof(FontWarpFactor), "Low" ), Description( "Amount of random font warping used on the CAPTCHA text" ), Category( "Captcha" )]
        public FontWarpFactor CaptchaFontWarping
        {
            get
            {
                return this._CaptchaImage.FontWarp;
            }
            set
            {
                this._CaptchaImage.FontWarp = value;
            }
        }

        [Category( "Captcha" ), DefaultValue( 50 ), Description( "Height of generated CAPTCHA image." )]
        public int CaptchaHeight
        {
            get
            {
                return this._CaptchaImage.Height;
            }
            set
            {
                this._CaptchaImage.Height = value;
            }
        }

        [DefaultValue( 5 ), Category( "Captcha" ), Description( "Number of CaptchaChars used in the CAPTCHA text" )]
        public int CaptchaLength
        {
            get
            {
                return this._CaptchaImage.TextLength;
            }
            set
            {
                this._CaptchaImage.TextLength = value;
            }
        }

        [Description( "Add line noise to the CAPTCHA image" ), Category( "Captcha" ), DefaultValue( typeof(LineNoiseLevel), "None" )]
        public LineNoiseLevel CaptchaLineNoise
        {
            get
            {
                return this._CaptchaImage.LineNoise;
            }
            set
            {
                this._CaptchaImage.LineNoise = value;
            }
        }

        [DefaultValue( 90 ), Category( "Captcha" ), Description( "Maximum number of seconds CAPTCHA will be cached and valid. If you're too slow, you may be a CAPTCHA hack attempt. Set to zero to disable." )]
        public int CaptchaMaxTimeout
        {
            get
            {
                return this._TimeoutSecondsMax;
            }
            set
            {
                if ( (value < 15) & (value != 0) )
                {
                    throw new ArgumentOutOfRangeException( "CaptchaTimeout", "Timeout must be greater than 15 seconds. Humans can't type that fast!" );
                }
                this._TimeoutSecondsMax = value;
            }
        }

        [Category( "Captcha" ), DefaultValue( 2 ), Description( "Minimum number of seconds CAPTCHA must be displayed before it is valid. If you're too fast, you must be a robot. Set to zero to disable." )]
        public int CaptchaMinTimeout
        {
            get
            {
                return this._TimeoutSecondsMin;
            }
            set
            {
                if ( value > 15 )
                {
                    throw new ArgumentOutOfRangeException( "CaptchaTimeout", "Timeout must be less than 15 seconds. Humans aren't that slow!" );
                }
                this._TimeoutSecondsMin = value;
            }
        }

        [DefaultValue( 180 ), Category( "Captcha" ), Description( "Width of generated CAPTCHA image." )]
        public int CaptchaWidth
        {
            get
            {
                return this._CaptchaImage.Width;
            }
            set
            {
                this._CaptchaImage.Width = value;
            }
        }

        public string CustomValidatorErrorMessage
        {
            get
            {
                return this._CustomValidatorErrorMessage;
            }
            set
            {
                this._CustomValidatorErrorMessage = value;
            }
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
                if ( !value )
                {
                    this._UserValidated = true;
                }
            }
        }

        public Color FontColor
        {
            get
            {
                return this._FontColor;
            }
            set
            {
                this._FontColor = value;
                this._CaptchaImage.FontColor = this._FontColor;
            }
        }

        private bool IsDesignMode
        {
            get
            {
                return (HttpContext.Current == null);
            }
        }

        public Color LineColor
        {
            get
            {
                return this._LineColor;
            }
            set
            {
                this._LineColor = value;
                this._CaptchaImage.LineColor = this._LineColor;
            }
        }

        public Color NoiseColor
        {
            get
            {
                return this._NoiseColor;
            }
            set
            {
                this._NoiseColor = value;
                this._CaptchaImage.NoiseColor = this._NoiseColor;
            }
        }

        [Category( "Appearance" ), Description( "Message to display in a Validation Summary when the CAPTCHA fails to validate." ), Browsable( false ), Bindable( true ), DefaultValue( "The text you typed does not match the text in the image." )]
        string IValidator.ErrorMessage
        {
            get
            {
                if ( !this._UserValidated )
                {
                    return this._ErrorMessage;
                }
                return string.Empty;
            }
            set
            {
                this._ErrorMessage = value;
            }
        }

        bool IValidator.IsValid
        {
            get
            {
                return this._UserValidated;
            }
            set
            {
            }
        }

        [Category( "Captcha" ), Description( "Returns True if the user was CAPTCHA validated after a postback." )]
        public bool UserValidated
        {
            get
            {
                return this._UserValidated;
            }
        }

        public string ValidationGroup
        {
            get
            {
                return this._ValidationGroup;
            }
            set
            {
                this._ValidationGroup = value;
            }
        }

        public string CaptchaImageUniqueId
        {
            get { return ((this._CaptchaImage != null) ? this._CaptchaImage.UniqueId : null); }
        }

        [Category( "Captcha" ), DefaultValue( false ), Description( "Ignore case when compare CAPTCHA image text." )]
        public bool CaptchaIgnoreCase
        {
            get;
            set;
        }

        [Category( "Captcha" ), DefaultValue( CAPTCHA_IMAGE_HANDLER_URL ), Description( "CAPTCHA-IMAGE-HANDLER-URL." )]
        public string CaptchaImageHandlerUrl
         {
             get;
             set;
         }
    }
}
