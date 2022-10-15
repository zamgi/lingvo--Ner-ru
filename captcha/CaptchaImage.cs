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

        private string   _FontFamilyName;
        private string[] _FontWhitelistArray;
        private DateTime _GeneratedAt;
        private string   _Guid;
        private int      _Height = 50;
        private Random   _Rand = new Random();
        private string   _RandomText;
        private string   _RandomTextChars  = "ACDEFGHJKLNPQRTUVXYZ2346789";
        private int      _RandomTextLength = 5;        
        private int      _Width = 180;

        private static bool IsLinux()
        {
            var p = (int) Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }

        public CaptchaImage()
        {
            _RandomText  = GenerateRandomText();
            _GeneratedAt = DateTime.Now;
            _Guid        = Guid.NewGuid().ToString();

            //if ( IsLinux() ) _FontFamilyName = FontFamily.GenericMonospace.Name;
        }

        private void AddLine ( Graphics graphics, Rectangle rect )
        {
            float width = 1f;
            int   n1    = 0;
            int   n2    = 0;
            switch ( LineNoise )
            {
                case LineNoiseLevel.None:
                    return;

                case LineNoiseLevel.Low:
                    n1 = 4;
                    n2 = 1;
                    width = Convert.ToSingle( (double) (((double) _Height) / 31.25) );                    
                    break;

                case LineNoiseLevel.Medium:
                    n1 = 5;
                    n2 = 1;
                    width = Convert.ToSingle( (double) (((double) _Height) / 27.7777) );                    
                    break;

                case LineNoiseLevel.High:
                    n1 = 3;
                    n2 = 2;
                    width = Convert.ToSingle( (int) (_Height / 0x19) );                    
                    break;

                case LineNoiseLevel.Extreme:
                    n1 = 3;
                    n2 = 3;
                    width = Convert.ToSingle( (double) (((double) _Height) / 22.7272) );
                    break;
            }
            var points = new PointF[ n1 + 1 ];
            using ( var pen = new Pen( LineColor, width ) )
            {
                for ( int i = 1; i <= n2; i++ )
                {
                    for ( int j = 0; j <= n1; j++ )
                    {
                        points[ j ] = RandomPoint( rect );
                    }
                    graphics.DrawCurve( pen, points, 1.75f );
                }
            }
        }
        private void AddNoise( Graphics graphics, Rectangle rect )
        {
            int n1 = 0;
            int n2 = 0;
            switch ( BackgroundNoise )
            {
                case BackgroundNoiseLevel.None:
                    return;

                case BackgroundNoiseLevel.Low:
                    n1 = 30;
                    n2 = 40;
                    break;

                case BackgroundNoiseLevel.Medium:
                    n1 = 0x12;
                    n2 = 40;
                    break;

                case BackgroundNoiseLevel.High:
                    n1 = 0x10;
                    n2 = 0x27;
                    break;

                case BackgroundNoiseLevel.Extreme:
                    n1 = 12;
                    n2 = 0x26;
                    break;
            }

            using ( var brush = new SolidBrush( NoiseColor ) )
            {
                int maxValue = Convert.ToInt32( (int) (Math.Max( rect.Width, rect.Height ) / n2) );
                for ( int i = 0; i <= Convert.ToInt32( (int) ((rect.Width * rect.Height) / n1) ); i++ )
                {
                    graphics.FillEllipse( brush, _Rand.Next( rect.Width ), _Rand.Next( rect.Height ), _Rand.Next( maxValue ), _Rand.Next( maxValue ) );
                }
            }
        }

        private Bitmap GenerateImagePrivate()
        {
            var image = new Bitmap( _Width, _Height, PixelFormat.Format32bppArgb );
            using ( var graphics = Graphics.FromImage( image ) )
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle( 0, 0, _Width, _Height );
                using ( var brush = new SolidBrush( BackColor ) )
                {
                    graphics.FillRectangle( brush, rect );
                }
                int n1 = 0;
                double n2 = _Width / _RandomTextLength;
                using ( var brush = new SolidBrush( FontColor ) )
                {
                    foreach ( char ch in _RandomText )
                    {
                        using ( var f = CreateFont() )
                        {
                            var rectf = new RectangleF( Convert.ToSingle( (double) (n1 * n2) ), 0, Convert.ToSingle( n2 ), _Height );
                            using ( GraphicsPath path = TextPath( ch.ToString(), f, rectf ) )
                            {
                                /* linux mono 2.10.8 => WARNING **: NOT IMPLEMENTED: GdipWarpPath */
                                if ( !IsLinux() )
                                {
                                    WarpText( path, rectf );
                                }
                                graphics.FillPath( brush, path );
                            }
                        }
                        n1++;
                    }
                }
                AddNoise( graphics, rect );
                AddLine( graphics, rect );
            }
            return (image);
        }
        private string GenerateRandomText()
        {
            var buf = new StringBuilder( _RandomTextLength );
            int length = _RandomTextChars.Length;
            for ( int i = 0, len = (_RandomTextLength - 1); i <= len; i++ )
            {
                buf.Append( _RandomTextChars.Substring( _Rand.Next( length ), 1 ) );
            }
            return (buf.ToString());
        }

        private Font CreateFont()
        {
            float  emSize     = 0f;
            string familyName = _FontFamilyName;
            if ( string.IsNullOrEmpty( familyName ) )
            {
                familyName = RandomFontFamily();
            }
            switch ( FontWarp )
            {
                case FontWarpFactor.None:
                    emSize = Convert.ToInt32( (double) (_Height * 0.7) );
                    break;

                case FontWarpFactor.Low:
                    emSize = Convert.ToInt32( (double) (_Height * 0.8) );
                    break;

                case FontWarpFactor.Medium:
                    emSize = Convert.ToInt32( (double) (_Height * 0.85) );
                    break;

                case FontWarpFactor.High:
                    emSize = Convert.ToInt32( (double) (_Height * 0.9) );
                    break;

                case FontWarpFactor.Extreme:
                    emSize = Convert.ToInt32( (double) (_Height * 0.95) );
                    break;
            }
            return (new Font( familyName, emSize, FontStyle.Bold ));
        }
        private string RandomFontFamily()
        {
            if ( _FontWhitelistArray == null )
            {
                _FontWhitelistArray = FontWhitelist.Split( new[] { ';' }, StringSplitOptions.RemoveEmptyEntries );
            }
            return _FontWhitelistArray[ _Rand.Next( 0, _FontWhitelistArray.Length ) ];
        }

        private PointF RandomPoint( Rectangle rect ) => RandomPoint( rect.Left, rect.Width, rect.Top, rect.Bottom );
        private PointF RandomPoint( int xmin, int xmax, int ymin, int ymax ) => new PointF( (float) _Rand.Next( xmin, xmax ), (float) _Rand.Next( ymin, ymax ) );

        public Bitmap RenderImage() => GenerateImagePrivate();

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
            float n1 = 1f;
            float n2 = 1f;
            switch ( FontWarp )
            {
                case FontWarpFactor.None:
                    return;

                case FontWarpFactor.Low:
                    n1 = 6f;
                    n2 = 1f;
                    break;

                case FontWarpFactor.Medium:
                    n1 = 5f;
                    n2 = 1.3f;
                    break;

                case FontWarpFactor.High:
                    n1 = 4.5f;
                    n2 = 1.4f;
                    break;

                case FontWarpFactor.Extreme:
                    n1 = 4f;
                    n2 = 1.5f;
                    break;
            }
            var srcRect = new RectangleF( Convert.ToSingle( rect.Left ), 0f, Convert.ToSingle( rect.Width ), (float) rect.Height );
            int n3 = Convert.ToInt32( (float) (((float) rect.Height) / n1) );
            int n4 = Convert.ToInt32( (float) (((float) rect.Width) / n1) );
            int xmin = (int) rect.Left - Convert.ToInt32( (float) (n4 * n2) );
            int ymin = (int) rect.Top  - Convert.ToInt32( (float) (n3 * n2) );
            int xmax = (int) (rect.Left + rect.Width) + Convert.ToInt32( (float) (n4 * n2) );
            int ymax = (int) (rect.Top + rect.Height) + Convert.ToInt32( (float) (n3 * n2) );
            if ( xmin < 0 )
            {
                xmin = 0;
            }
            if ( ymin < 0 )
            {
                ymin = 0;
            }
            if ( xmax > Width )
            {
                xmax = Width;
            }
            if ( ymax > Height )
            {
                ymax = Height;
            }
            PointF tf  = RandomPoint( xmin, xmin + n4, ymin, ymin + n3 );
            PointF tf2 = RandomPoint( xmax - n4, xmax, ymin, ymin + n3 );
            PointF tf3 = RandomPoint( xmin, xmin + n4, ymax - n3, ymax );
            PointF tf4 = RandomPoint( xmax - n4, xmax, ymax - n3, ymax );
            var destPoints = new[] { tf, tf2, tf3, tf4 };
            var matrix = new Matrix();
            matrix.Translate( 0f, 0f );
            textPath.Warp( destPoints, srcRect, matrix, WarpMode.Perspective, 0f );
        }

        public Color BackColor { get; set; } = Color.White;
        public BackgroundNoiseLevel BackgroundNoise { get; set; } = BackgroundNoiseLevel.Low;
        public string Font
        {
            get => _FontFamilyName;
            set
            {
                Font font = null;
                try
                {
                    font = new Font( value, 12f );
                    _FontFamilyName = value;
                }
                catch ( Exception )
                {
                    _FontFamilyName = FontFamily.GenericSerif.Name;
                }
                finally
                {
                    font.Dispose();
                }
            }
        }
        public Color FontColor { get; set; } = Color.Black;
        public FontWarpFactor FontWarp { get; set; } = FontWarpFactor.Low;
        public string FontWhitelist { get; set; } = "arial;arial black;comic sans ms;courier new;estrangelo edessa;franklin gothic medium;georgia;lucida console;lucida sans unicode;mangal;microsoft sans serif;palatino linotype;sylfaen;tahoma;times new roman;trebuchet ms;verdana";
        public int Height
        {
            get => _Height;
            set
            {
                if ( value <= 30 ) throw (new ArgumentOutOfRangeException( "height", value, "height must be greater than 30." ));
                _Height = value;
            }
        }
        public Color LineColor { get; set; } = Color.Black;
        public LineNoiseLevel LineNoise { get; set; } = LineNoiseLevel.None;
        public Color NoiseColor { get; set; } = Color.Black;
        public DateTime RenderedAt => _GeneratedAt;
        public string Text => _RandomText;            
        public string TextChars
        {
            get => _RandomTextChars;
            set
            {
                _RandomTextChars = value;
                _RandomText      = GenerateRandomText();
            }
        }
        public int TextLength
        {
            get => _RandomTextLength;
            set
            {
                _RandomTextLength = value;
                _RandomText       = GenerateRandomText();
            }
        }
        public string UniqueId => _Guid;            
        public int Width
        {
            get => _Width;
            set
            {
                if ( value <= 60 ) throw (new ArgumentOutOfRangeException( "width", value, "width must be greater than 60." ));
                _Width = value;
            }
        }
    }
}
