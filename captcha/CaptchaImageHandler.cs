using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Web;

namespace captcha
{
    public class CaptchaImageHandler : IHttpHandler
    {
        // Methods
        public void ProcessRequest( HttpContext context )
        {
            HttpApplication applicationInstance = context.ApplicationInstance;
            string key = applicationInstance.Request.QueryString[ "guid" ];
            CaptchaImage image = null;
            if ( !string.IsNullOrEmpty( key ) )
            {
                if ( string.IsNullOrEmpty( applicationInstance.Request.QueryString[ "s" ] ) )
                {
                    image = (CaptchaImage) HttpRuntime.Cache.Get( key );
                }
                else
                {
                    image = (CaptchaImage) HttpContext.Current.Session[ key ];
                }
            }
            if ( image == null )
            {
                if ( key == "xz" )
                {
                    //var rm = new ResourceManager( , Assembly.GetExecutingAssembly() );
                    //var x = rm.GetObject( "captcha.CaptchaControl.bmp" );
                    applicationInstance.Response.ContentType = "application/json";
                    applicationInstance.Context.Response.Write( "{ key: \"xz\" }" );
                    applicationInstance.Response.StatusCode = 200;
                    context.ApplicationInstance.CompleteRequest();
                }
                
                applicationInstance.Response.StatusCode = 404;
                context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                using ( Bitmap bitmap = image.RenderImage() )
                {
                    bitmap.Save( applicationInstance.Context.Response.OutputStream, ImageFormat.Jpeg );
                }
                applicationInstance.Response.ContentType = "image/jpeg";
                applicationInstance.Response.StatusCode = 200;
                context.ApplicationInstance.CompleteRequest();
            }
        }

        // Properties
        public bool IsReusable
        {
            get { return (true); }
        }
    }
}
