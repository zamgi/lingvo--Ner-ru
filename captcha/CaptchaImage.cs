using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;

namespace captcha
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CaptchaImage
    {
        /// <summary>
        /// 
        /// </summary>
        public enum BackgroundNoiseLevel
        {
            None,
            Low,
            Medium,
            High,
            Extreme
        }
        /// <summary>
        /// 
        /// </summary>
        public enum FontWarpFactor
        {
            None,
            Low,
            Medium,
            High,
            Extreme
        }
        /// <summary>
        /// 
        /// </summary>
        public enum LineNoiseLevel
        {
            None,
            Low,
            Medium,
            High,
            Extreme
        }

        // Fields
        private Color                _BackColor = Color.White;
        private BackgroundNoiseLevel _BackgroundNoise = BackgroundNoiseLevel.Low;
        private Color                _FontColor = Color.Black;
        private string               _FontFamilyName;
        private FontWarpFactor       _FontWarp = FontWarpFactor.Low;
        private string               _FontWhitelist = "arial;arial black;comic sans ms;courier new;estrangelo edessa;franklin gothic medium;georgia;lucida console;lucida sans unicode;mangal;microsoft sans serif;palatino linotype;sylfaen;tahoma;times new roman;trebuchet ms;verdana";
        private string[]             _FontWhitelistArray;
        private DateTime             _GeneratedAt;
        private string               _Guid;
        private int                  _Height = 50;
        private Color                _LineColor = Color.Black;
        private LineNoiseLevel       _LineNoise = LineNoiseLevel.None;
        private Color                _NoiseColor = Color.Black;
        private Random               _Rand = new Random();
        private string               _RandomText;
        private string               _RandomTextChars  = "ACDEFGHJKLNPQRTUVXYZ2346789";
        private int                  _RandomTextLength = 5;        
        private int                  _Width = 180;

        private static bool IsLinux()
        {
            var p = (int) Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }

        // Methods
        public CaptchaImage()
        {
            this._RandomText  = this.GenerateRandomText();
            this._GeneratedAt = DateTime.Now;
            this._Guid        = Guid.NewGuid().ToString();

            /*if ( IsLinux() )
            {
                _FontFamilyName = FontFamily.GenericMonospace.Name;
            }*/
        }

        private void AddLine ( Graphics graphics, Rectangle rect )
        {
            int num = 0;
            float width = 1f;
            int num3 = 0;
            switch (this._LineNoise)
            {
                case LineNoiseLevel.None:
                    return;

                case LineNoiseLevel.Low:
                    num = 4;
                    width = Convert.ToSingle((double)(((double)this._Height) / 31.25));
                    num3 = 1;
                    break;

                case LineNoiseLevel.Medium:
                    num = 5;
                    width = Convert.ToSingle((double)(((double)this._Height) / 27.7777));
                    num3 = 1;
                    break;

                case LineNoiseLevel.High:
                    num = 3;
                    width = Convert.ToSingle((int)(this._Height / 0x19));
                    num3 = 2;
                    break;

                case LineNoiseLevel.Extreme:
                    num = 3;
                    width = Convert.ToSingle((double)(((double)this._Height) / 22.7272));
                    num3 = 3;
                    break;
            }
            PointF[] points = new PointF[num + 1];
            using ( var pen = new Pen(this._LineColor, width) )
            {
                for (int i = 1; i <= num3; i++)
                {
                    for (int j = 0; j <= num; j++)
                    {
                        points[ j ] = this.RandomPoint(rect);
                    }
                    graphics.DrawCurve(pen, points, 1.75f);
                }
            }
        }
        private void AddNoise( Graphics graphics, Rectangle rect )
        {
            int num = 0;
            int num2 = 0;
            switch (this._BackgroundNoise)
            {
                case BackgroundNoiseLevel.None:
                    return;

                case BackgroundNoiseLevel.Low:
                    num = 30;
                    num2 = 40;
                    break;

                case BackgroundNoiseLevel.Medium:
                    num = 0x12;
                    num2 = 40;
                    break;

                case BackgroundNoiseLevel.High:
                    num = 0x10;
                    num2 = 0x27;
                    break;

                case BackgroundNoiseLevel.Extreme:
                    num = 12;
                    num2 = 0x26;
                    break;
            }

            using ( var brush = new SolidBrush(this._NoiseColor))
            {
                int maxValue = Convert.ToInt32((int)(Math.Max(rect.Width, rect.Height) / num2));
                for (int i = 0; i <= Convert.ToInt32((int)((rect.Width * rect.Height) / num)); i++)
                {
                    graphics.FillEllipse(brush, this._Rand.Next(rect.Width), this._Rand.Next(rect.Height), this._Rand.Next(maxValue), this._Rand.Next(maxValue));
                }
            }
        }

        private Bitmap GenerateImagePrivate()
        {
            var image = new Bitmap( this._Width, this._Height, PixelFormat.Format32bppArgb );
            using ( var graphics = Graphics.FromImage( image ) )
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle( 0, 0, this._Width, this._Height );
                using ( var brush = new SolidBrush( this._BackColor ) )
                {
                    graphics.FillRectangle( brush, rect );
                }
                int num = 0;
                double num2 = this._Width / this._RandomTextLength;
                using ( var brush = new SolidBrush( this._FontColor ) )
                {
                    foreach ( char ch in this._RandomText )
                    {
                        using ( var f = this.GetFont() )
                        {
                            var rectf = new RectangleF( Convert.ToSingle( (double) (num * num2) ), 0, Convert.ToSingle( num2 ), this._Height );
                            using ( GraphicsPath path = this.TextPath( ch.ToString(), f, rectf ) )
                            {
                                /* linux mono 2.10.8 => WARNING **: NOT IMPLEMENTED: GdipWarpPath */
                                if ( !IsLinux() )
                                {
                                    this.WarpText( path, rectf );
                                }
                                graphics.FillPath( brush, path );
                            }
                        }
                        num++;
                    }
                }
                this.AddNoise( graphics, rect );
                this.AddLine ( graphics, rect );
            }
            return (image);
        }
        private string GenerateRandomText()
        {
            var builder = new StringBuilder( this._RandomTextLength );
            int length = this._RandomTextChars.Length;
            for ( int i = 0; i <= (this._RandomTextLength - 1); i++ )
            {
                builder.Append( this._RandomTextChars.Substring( this._Rand.Next( length ), 1 ) );
            }
            return builder.ToString();
        }

        private Font GetFont()
        {
            float emSize = 0f;
            string familyName = this._FontFamilyName;
            if ( string.IsNullOrEmpty( familyName ) )
            {
                familyName = this.RandomFontFamily();
            }
            switch (this.FontWarp)
            {
                case FontWarpFactor.None:
                    emSize = Convert.ToInt32((double)(this._Height * 0.7));
                    break;

                case FontWarpFactor.Low:
                    emSize = Convert.ToInt32((double)(this._Height * 0.8));
                    break;

                case FontWarpFactor.Medium:
                    emSize = Convert.ToInt32((double)(this._Height * 0.85));
                    break;

                case FontWarpFactor.High:
                    emSize = Convert.ToInt32((double)(this._Height * 0.9));
                    break;

                case FontWarpFactor.Extreme:
                    emSize = Convert.ToInt32((double)(this._Height * 0.95));
                    break;
            }
            return new Font(familyName, emSize, FontStyle.Bold);
        }
        private string RandomFontFamily()
        {
            if ( _FontWhitelistArray == null )
            {
                _FontWhitelistArray = _FontWhitelist.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries );
            }
            return _FontWhitelistArray[ this._Rand.Next( 0, _FontWhitelistArray.Length ) ];
        }

        private PointF RandomPoint( Rectangle rect )
        {
            return this.RandomPoint(rect.Left, rect.Width, rect.Top, rect.Bottom);
        }
        private PointF RandomPoint( int xmin, int xmax, int ymin, int ymax )
        {
            return new PointF((float)this._Rand.Next(xmin, xmax), (float)this._Rand.Next(ymin, ymax));
        }

        public Bitmap RenderImage()
        {
            return this.GenerateImagePrivate();
        }

        private GraphicsPath TextPath( string s, Font font, RectangleF rect )
        {
            var format = new StringFormat()
            {
                Alignment     = StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
                
            };
            var path = new GraphicsPath();
            path.AddString( s, font.FontFamily, (int) font.Style, font.Size, rect, format );
            return (path);
        }

        /* linux mono 2.10.8 => WARNING **: NOT IMPLEMENTED: GdipWarpPath */
        private void WarpText( GraphicsPath textPath, RectangleF rect )
        {
            float num = 1f;
            float num2 = 1f;
            switch ( this._FontWarp )
            {
                case FontWarpFactor.None:
                    return;

                case FontWarpFactor.Low:
                    num = 6f;
                    num2 = 1f;
                    break;

                case FontWarpFactor.Medium:
                    num = 5f;
                    num2 = 1.3f;
                    break;

                case FontWarpFactor.High:
                    num = 4.5f;
                    num2 = 1.4f;
                    break;

                case FontWarpFactor.Extreme:
                    num = 4f;
                    num2 = 1.5f;
                    break;
            }
            RectangleF srcRect = new RectangleF( Convert.ToSingle( rect.Left ), 0f, Convert.ToSingle( rect.Width ), (float) rect.Height );
            int num3 = Convert.ToInt32( (float) (((float) rect.Height) / num) );
            int num4 = Convert.ToInt32( (float) (((float) rect.Width) / num) );
            int xmin = (int) rect.Left - Convert.ToInt32( (float) (num4 * num2) );
            int ymin = (int) rect.Top  - Convert.ToInt32( (float) (num3 * num2) );
            int xmax = (int) (rect.Left + rect.Width) + Convert.ToInt32( (float) (num4 * num2) );
            int ymax = (int) (rect.Top + rect.Height) + Convert.ToInt32( (float) (num3 * num2) );
            if ( xmin < 0 )
            {
                xmin = 0;
            }
            if ( ymin < 0 )
            {
                ymin = 0;
            }
            if ( xmax > this.Width )
            {
                xmax = this.Width;
            }
            if ( ymax > this.Height )
            {
                ymax = this.Height;
            }
            PointF tf  = this.RandomPoint( xmin, xmin + num4, ymin, ymin + num3 );
            PointF tf2 = this.RandomPoint( xmax - num4, xmax, ymin, ymin + num3 );
            PointF tf3 = this.RandomPoint( xmin, xmin + num4, ymax - num3, ymax );
            PointF tf4 = this.RandomPoint( xmax - num4, xmax, ymax - num3, ymax );
            PointF[] destPoints = new PointF[] { tf, tf2, tf3, tf4 };
            Matrix matrix = new Matrix();
            matrix.Translate( 0f, 0f );
            textPath.Warp( destPoints, srcRect, matrix, WarpMode.Perspective, 0f );
        }

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
            }
        }
        public BackgroundNoiseLevel BackgroundNoise
        {
            get
            {
                return this._BackgroundNoise;
            }
            set
            {
                this._BackgroundNoise = value;
            }
        }
        public string Font
        {
            get
            {
                return this._FontFamilyName;
            }
            set
            {
                Font font = null;
                try
                {
                    font = new Font(value, 12f);
                    this._FontFamilyName = value;
                }
                catch (Exception)
                {
                    this._FontFamilyName = FontFamily.GenericSerif.Name;
                }
                finally
                {
                    font.Dispose();
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
            }
        }
        public FontWarpFactor FontWarp
        {
            get
            {
                return this._FontWarp;
            }
            set
            {
                this._FontWarp = value;
            }
        }
        public string FontWhitelist
        {
            get
            {
                return this._FontWhitelist;
            }
            set
            {
                this._FontWhitelist = value;
            }
        }
        public int Height
        {
            get
            {
                return this._Height;
            }
            set
            {
                if (value <= 30)
                {
                    throw new ArgumentOutOfRangeException("height", value, "height must be greater than 30.");
                }
                this._Height = value;
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
            }
        }
        public LineNoiseLevel LineNoise
        {
            get
            {
                return this._LineNoise;
            }
            set
            {
                this._LineNoise = value;
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
            }
        }
        public DateTime RenderedAt
        {
            get
            {
                return this._GeneratedAt;
            }
        }
        public string Text
        {
            get
            {
                return this._RandomText;
            }
        }
        public string TextChars
        {
            get
            {
                return this._RandomTextChars;
            }
            set
            {
                this._RandomTextChars = value;
                this._RandomText = this.GenerateRandomText();
            }
        }
        public int TextLength
        {
            get
            {
                return this._RandomTextLength;
            }
            set
            {
                this._RandomTextLength = value;
                this._RandomText = this.GenerateRandomText();
            }
        }
        public string UniqueId
        {
            get
            {
                return this._Guid;
            }
        }
        public int Width
        {
            get
            {
                return this._Width;
            }
            set
            {
                if (value <= 60)
                {
                    throw new ArgumentOutOfRangeException("width", value, "width must be greater than 60.");
                }
                this._Width = value;
            }
        }
    }
}
