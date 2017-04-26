using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace lingvo.ner
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        public static bool Try2Bool( this string value, bool defaultValue )
        {
            if ( value != null )
            {
                var result = default(bool);
                if ( bool.TryParse( value, out result ) )
                {
                    return (result);
                }                    
            }
            return (defaultValue);
        }

        public static T ToEnum< T >( this string value ) where T : struct
        {
            var result = (T) Enum.Parse( typeof(T), value, true );
            return (result);
        }

        public static string GetRequestStringParam( this HttpContext context, string paramName, int maxLength )
        {
            var value = context.Request[ paramName ];
            if ( (value != null) && (maxLength < value.Length) && (0 < maxLength) )
            {
                return (value.Substring( 0, maxLength ));
            }
            return (value);
        }
    }
}