using System;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace lingvo.core
{
    /// <summary>
    /// 
    /// </summary>
    unsafe public static class StringsHelper
    {
        private static char* _UPPER_INVARIANT_MAP;
        static StringsHelper() => _UPPER_INVARIANT_MAP = xlat_Unsafe.Inst._UPPER_INVARIANT_MAP;

        /// <summary>
        /// 
        /// </summary>
        [M(O.AggressiveInlining)] public static string ToUpperInvariant( string value )
        {
            var len = value.Length;
            if ( 0 < len )
            {
                string valueUpper;

                const int THRESHOLD = 1024;
                if ( len <= THRESHOLD )
                {
                    var chars = stackalloc char[ len ];
                    fixed ( char* value_ptr = value )
                    {
                        for ( var i = 0; i < len; i++ )
                        {
                            chars[ i ] = _UPPER_INVARIANT_MAP[ value_ptr[ i ] ];
                        }
                    }
                    valueUpper = new string( chars, 0, len );
                }
                else
                {
                    valueUpper = new string( '\0', len ); // string.Copy( value ); // => [Obsolete( "This API should not be used to create mutable strings. See https://go.microsoft.com/fwlink/?linkid=2084035 for alternatives." )]
                    fixed ( char* value_ptr = value )
                    fixed ( char* valueUpper_ptr = valueUpper )
                    {
                        for ( var i = 0; i < len; i++ )
                        {
                            valueUpper_ptr[ i ] = _UPPER_INVARIANT_MAP[ value_ptr[ i ] ];
                        }
                    }                    
                }

                return (valueUpper);
            }
            return (string.Empty);
        }
        //[M(O.AggressiveInlining)] public static string ToUpperInvariant( string value )
        //{
        //    var len = value.Length;
        //    if ( 0 < len )
        //    {
        //        var valueUpper = string.Copy( value );
        //        fixed ( char* valueUpper_ptr = valueUpper )
        //        {
        //            for ( int i = 0; i < len; i++ )
        //            {
        //                var ptr = valueUpper_ptr + i;
        //                *ptr = *(_UPPER_INVARIANT_MAP + *ptr);
        //            }
        //        }
        //        return (valueUpper);
        //    }
        //    return (string.Empty);
        //}
        [M(O.AggressiveInlining)] public static void   ToUpperInvariant( char* wordFrom, char* bufferTo )
        {
            for ( ; ; wordFrom++, bufferTo++ )
            {
                var ch = *wordFrom;
                *bufferTo = *(_UPPER_INVARIANT_MAP + ch);
                if ( ch == '\0' )
                    return;
            }            
        }
        [M(O.AggressiveInlining)] public static void   ToUpperInvariantInPlace( string value )
        {
            fixed ( char* value_ptr = value )
            {
                ToUpperInvariantInPlace( value_ptr );
            }
        }
        [M(O.AggressiveInlining)] public static void   ToUpperInvariantInPlace( char* word )
        {
            for ( ; ; word++ )
            {
                var ch = *word;
                if ( ch == '\0' )
                    return;
                *word = *(_UPPER_INVARIANT_MAP + ch);
            }
        }
        [M(O.AggressiveInlining)] public static void   ToUpperInvariantInPlace( char* word, int length )
        {
            for ( length--; 0 <= length; length-- )
            {
                word[ length ] = _UPPER_INVARIANT_MAP[ word[ length ] ];
            }
        }

        [M(O.AggressiveInlining)] public static string ToLowerInvariant( string value ) => value.ToLowerInvariant();

        /// проверка эквивалентности строк
        [M(O.AggressiveInlining)] public static bool IsEqual( string first, string second )
        {
            int length = first.Length;
            if ( length != second.Length )
            {
                return (false);
            }
            if ( length == 0 )
            {
                return (true);
            }

            fixed ( char* first_ptr  = first )
            fixed ( char* second_ptr = second )
            {
                for ( int i = 0; i < length; i++ )
                {
                    if ( *(first_ptr + i) != *(second_ptr + i) ) //if ( GetLetter( first, i ) != GetLetter( second, i ) )
                    {
                        return (false);
                    }
                }
            }
            return (true);
        }
        [M(O.AggressiveInlining)] public static bool IsEqual( string first, char* second_ptr, int secondLength )
        {
            /*
            if ( first.Length != secondLength )
            {
                return (false);
            }
            if ( secondLength == 0 )
            {
                return (true);
            }
            */

            fixed ( char* first_ptr  = first )
            {
                for ( int i = 0; i < secondLength; i++ )
                {
                    if ( *(first_ptr + i) != *(second_ptr + i) )
                    {
                        return (false);
                    }
                }
            }
            return (true);
        }

        [M(O.AggressiveInlining)] public static bool IsEqual( string first, int firstIndex, string second )
        {
            int length = first.Length - firstIndex;
            if ( length != second.Length )
            {
                return (false);
            }
            if ( length == 0 )
            {
                return (true);
            }

            fixed ( char* first_base = first  )
            fixed ( char* second_ptr = second )
            {
                char* first_ptr = first_base + firstIndex;
                for ( int i = 0; i < length; i++ )
                {
                    if ( *(first_ptr + i) != *(second_ptr + i) ) //if ( GetLetter( first, i ) != GetLetter( second, i ) )
                    {
                        return (false);
                    }
                }
            }
            return (true);
        }
        [M(O.AggressiveInlining)] public static bool IsEqual( string first, int firstIndex, char* second_ptr, int secondLength )
        {
            int length = first.Length - firstIndex;
            if ( length != secondLength )
            {
                return (false);
            }
            if ( secondLength == 0 )
            {
                return (true);
            }

            fixed ( char* first_base = first  )
            {
                char* first_ptr = first_base + firstIndex;
                for ( int i = 0; i < secondLength; i++ )
                {
                    if ( *(first_ptr + i) != *(second_ptr + i) ) //if ( GetLetter( first, i ) != GetLetter( second, i ) )
                    {
                        return (false);
                    }
                }
            }
            return (true);
        }

        [M(O.AggressiveInlining)] public static bool IsEqual( IntPtr x, IntPtr y )
        {
            if ( x == y )
                return (true);

            for ( char* x_ptr = (char*) x.ToPointer(),
                        y_ptr = (char*) y.ToPointer(); ; x_ptr++, y_ptr++ )
            {
                var x_ch = *x_ptr;

                if ( x_ch != *y_ptr )
                    return (false);
                if ( x_ch == '\0' )
                    return (true);
            }
        }
        [M(O.AggressiveInlining)] public static bool IsEqual( char* x, char* y )
        {
            if ( x == y )
                return (true);

            for ( ; ; x++, y++)
            {
                var x_ch = *x;

                if ( x_ch != *y )
                    return (false);
                if ( x_ch == '\0' )
                    return (true);
            }
        }

        [M(O.AggressiveInlining)] public static int GetLength( char* _base )
        {
            for ( var ptr = _base; ; ptr++ )
            {
                if ( *ptr == '\0' )
                {
                    return ((int)(ptr - _base));
                }
            }
        }
        [M(O.AggressiveInlining)] public static int GetLength( IntPtr _base ) => GetLength( (char*) _base );

        [M(O.AggressiveInlining)] public static string CreateWordForm( string _base, char* morphoFormEnding )
        {
            var endingLength = GetLength( morphoFormEnding );
            if ( endingLength == 0 )
                return (_base);

            var wordForm = new string( '\0', _base.Length + endingLength );
            fixed ( char* wordForm_ptr = wordForm )
            fixed ( char* _base_ptr    = _base )
            {
                var wf_ptr = wordForm_ptr;
                var ptr    = _base_ptr;
                for ( ; ; )
                {
                    var ch = *(ptr++);
                    if ( ch == '\0' )
                        break;
                    *(wf_ptr++) = ch;
                }

                for ( ; ; )
                {
                    var ch = *(morphoFormEnding++);
                    if ( ch == '\0' )
                        break;
                    *(wf_ptr++) = ch;
                }
            }
            return (wordForm);
        }
        [M(O.AggressiveInlining)] public static string CreateWordForm( char*  _base, char* morphoFormEnding )
        {            
            var endingLength = GetLength( morphoFormEnding );
            if ( endingLength == 0 )
            {
                return (new string( _base ));
            }

            var baseLength = GetLength( _base );

            var wordForm = new string( '\0', baseLength + endingLength );
            fixed ( char* wordFormBase = wordForm )
            {
                var wf_ptr = wordFormBase;
                for ( ; ; )
                {
                    var ch = *(_base++);
                    if ( ch == '\0' )
                        break;
                    *(wf_ptr++) = ch;
                }

                for ( ; ; )
                {
                    var ch = *(morphoFormEnding++);
                    if ( ch == '\0' )
                        break;
                    *(wf_ptr++) = ch;
                }
            }
            return (wordForm);
        }

        [M(O.AggressiveInlining)] public static string ToString( char* value )
        {
            if ( value == null )
            {
                return (null);
            }

            var length = GetLength( value );
            if ( length == 0 )
            {
                return (string.Empty);
            }

            var str = new string( '\0', length );
            fixed ( char* str_ptr = str )
            {                
                for ( var wf_ptr = str_ptr; ; )
                {
                    var ch = *(value++);
                    if ( ch == '\0' )
                        break;
                    *(wf_ptr++) = ch;
                }
            }
            return (str);
        }
        [M(O.AggressiveInlining)] public static string ToString( char* value, int length )
        {
            if ( value == null )
            {
                return (null);
            }

            if ( length == 0 )
            {
                return (string.Empty);
            }

            var str = new string( '\0', length );
            fixed ( char* str_ptr = str )
            {
                for ( var wf_ptr = str_ptr; 0 < length; length-- )
                {
                    var ch = *(value++);
                    if ( ch == '\0' )
                        break;
                    *(wf_ptr++) = ch;
                }
            }
            return (str);
        }
        [M(O.AggressiveInlining)] public static string ToString( IntPtr value ) => ToString( (char*) value );
    }
}
