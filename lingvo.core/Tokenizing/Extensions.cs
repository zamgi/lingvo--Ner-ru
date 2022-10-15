using System;
using System.Collections.Generic;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace lingvo.core
{
    /// <summary>
    /// 
    /// </summary>
    public static class Extensions
    {
        public static void ThrowIfNull( this object obj, string paramName )
        {
            if ( obj == null ) throw (new ArgumentNullException( paramName ));
        }
        public static void ThrowIfNullOrWhiteSpace( this string text, string paramName )
        {
            if ( text.IsNullOrWhiteSpace() ) throw (new ArgumentNullException( paramName ));
        }
        public static void ThrowIfNullOrWhiteSpaceAnyElement( this IEnumerable< string > seq, string paramName )
        {
            if ( seq == null ) throw (new ArgumentNullException( paramName ));

            foreach ( var s in seq )
            {
                if ( s.IsNullOrWhiteSpace() ) throw (new ArgumentNullException( $"'{paramName}' => some collection element is NULL-or-WhiteSpace." ));
            }
        }

        [M(O.AggressiveInlining)] public static bool IsNullOrWhiteSpace( this string text ) => string.IsNullOrWhiteSpace( text );
        [M(O.AggressiveInlining)] public static bool IsNullOrEmpty( this string text ) => string.IsNullOrEmpty( text );
    }
}
