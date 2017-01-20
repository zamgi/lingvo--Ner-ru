using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lingvo.core
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        public static void ThrowIfNull( this object obj, string paramName )
        {
            if ( obj == null )
                throw (new ArgumentNullException( paramName ));
        }
        public static void ThrowIfNullOrWhiteSpace( this string text, string paramName )
        {
            if ( string.IsNullOrWhiteSpace( text ) )
                throw (new ArgumentNullException( paramName ));
        }
        /*public static void ThrowIfNullAnyElement< T >( this T[] array, string paramName )
        {
            if ( array == null )
                throw (new ArgumentNullException( paramName ));
            foreach ( var a in array )
            {
                if ( a == null )
                    throw (new ArgumentNullException( paramName + " => some array element is NULL" ));
            }
        }
        public static void ThrowIfNullAnyElement< T >( this ICollection< T > collection, string paramName )
        {
            if ( collection == null )
                throw (new ArgumentNullException( paramName ));
            foreach ( var c in collection )
            {
                if ( c == null )
                    throw (new ArgumentNullException( paramName + " => some collection element is NULL" ));
            }
        }
        */
        public static void ThrowIfNullOrWhiteSpaceAnyElement( this ICollection< string > collection, string paramName )
        {
            if ( collection == null )
                throw (new ArgumentNullException( paramName ));
            foreach ( var c in collection )
            {
                if ( string.IsNullOrWhiteSpace( c ) )
                    throw (new ArgumentNullException( paramName + " => some collection element is NULL-or-WhiteSpace" ));
            }
        }

        public static bool IsNullOrWhiteSpace( this string text )
        {
            return (string.IsNullOrWhiteSpace( text ));
        }
        public static bool IsNullOrEmpty( this string text )
        {
            return (string.IsNullOrEmpty( text ));
        }
    }
}
