using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace lingvo.sentsplitting
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        public static HashSet< string > ToHashset( this IEnumerable< string > seq, bool toUpperInvariant ) 
            => new HashSet< string >( seq.Select( d => d.TrimEx( toUpperInvariant ) ).Where( d => !string.IsNullOrEmpty( d ) ) );
        public static HashSet< string > ToHashsetWithReverseValues( this IEnumerable< string > seq, bool toUpperInvariant )
            => new HashSet< string >( seq.Select( d => (d != null) ? new string( d.Trim().Reverse().ToArray() ).ToUpperInvariantEx( toUpperInvariant ) : null ).Where( d => !string.IsNullOrEmpty( d ) ) );
        public static int GetItemMaxLength( this HashSet< string > hs ) => ((hs.Count != 0) ? hs.Max( d => d.Length ) : 0);

        public static Dictionary< string, T > ToDictionary< T >( this IEnumerable< KeyValuePair< string, T > > seq, bool toUpperInvariant )
        {
            var dict = new Dictionary< string, T >();
            foreach ( var pair in seq )
            {
                var key = pair.Key.TrimEx( toUpperInvariant );
                if ( string.IsNullOrEmpty( key ) )
                    continue;

                if ( dict.ContainsKey( key ) )
                    continue;

                dict.Add( key, pair.Value );
            }
            return (dict);
        }
        public static int GetItemMaxKeyLength< T >( this Dictionary< string, T > dict ) => ((dict.Count != 0) ? dict.Max( p => p.Key.Length ) : 0);
        public static int GetItemMinKeyLength< T >( this Dictionary< string, T > dict ) => ((dict.Count != 0) ? dict.Min( p => p.Key.Length ) : 0);

        public static string TrimStartDot( this string value ) => value.TrimStart( '.' );
        public static string TrimEndDot( this string value ) => value.TrimEnd( '.' );

        public static bool AttrValueIsTrue( this XElement xe, string attrName )
        {
            var xa = xe.Attribute( attrName );
            if ( xa != null )
            {
                if ( bool.TryParse( xa.Value, out var r ) )
                    return (r);
            }
            return (false);
        }

        private static string TrimEx( this string value, bool toUpperInvarian )
        {
            if ( value == null )
                return (null);
            return (toUpperInvarian ? value.ToUpperInvariant() : value);
        }
        private static string ToUpperInvariantEx( this string value, bool toUpperInvarian ) => (toUpperInvarian ? value.ToUpperInvariant() : value);

        public static IEnumerable< T > SelectMany< T >( this IEnumerable< IEnumerable< T > > t ) => t.SelectMany( _ => _ );
    }

    internal struct before_proper_loader_t
    {
        private const char DOT = '.';
        private char[] SPLIT_BY_DOT;
        private char[] SPLIT_BY_SPACES;
        public static before_proper_loader_t Create() => new before_proper_loader_t()
        {
            SPLIT_BY_DOT    = new[] { DOT },
            SPLIT_BY_SPACES = new[] { ' ', '\t', '\r', '\n' }
        };

        public IEnumerable< ngram_t< before_no_proper_t > > ToBeforeNoProper_ngrams( IEnumerable< XElement > xelements )
        {
            foreach ( var xe in xelements )
            {
                yield return (ToBeforeNoProper_ngrams( xe ));
            }
        }
        public ngram_t< before_no_proper_t > ToBeforeNoProper_ngrams( XElement xe )
        {
            var words = GetWordsArray( xe );
            var unstick_from_digits = xe.AttrValueIsTrue( "unstick-from-digits" );

            var ngram = new ngram_t< before_no_proper_t >( words, new before_no_proper_t( unstick_from_digits ) );
            return (ngram);
        }

        public IEnumerable< ngram_t< before_proper_or_number_t > > ToBeforeProperOrNumber_ngrams( IEnumerable< XElement > xelements )
        {
            foreach ( var xe in xelements )
            {
                yield return (ToBeforeProperOrNumber_ngrams( xe ));
            }
        }
        public ngram_t< before_proper_or_number_t > ToBeforeProperOrNumber_ngrams( XElement xe )
        {
            var words = GetWordsArray( xe );
            var digits_before       = xe.AttrValueIsTrue( "digits-before" );
            var slash_before        = xe.AttrValueIsTrue( "slash-before" );
            var unstick_from_digits = xe.AttrValueIsTrue( "unstick-from-digits" );

            var ngram = new ngram_t< before_proper_or_number_t >( words, new before_proper_or_number_t( digits_before, slash_before, unstick_from_digits ) );
            return (ngram);
        }

        private string[] GetWordsArray( XElement xe )
        {
            var words = xe.Value.Split( SPLIT_BY_DOT, StringSplitOptions.RemoveEmptyEntries );
            var word_list = new List< string >( words.Length );
            for ( int i = 0, len = words.Length - 1; i <= len; i++ )
            {
                var word = words[ i ].Trim();
                var words_by_space = word.Split( SPLIT_BY_SPACES, StringSplitOptions.RemoveEmptyEntries );
                if ( words_by_space.Length == 1 )
                {
                    //if ( i == len )
                    //{
                    //    word_list.Add( word );
                    //}
                    //else
                    //{
                        word_list.Add( word + DOT );
                    //}                    
                }
                else
                {
                    for ( int j = 0, len_by_space = words_by_space.Length - 1; j <= len_by_space; j++ )
                    {
                        word = words_by_space[ j ];
                        if ( j == len_by_space )
                        {
                            //if ( i == len )
                            //{
                            //    word_list.Add( word );
                            //}
                            //else
                            //{
                                word_list.Add( word + DOT );
                            //} 
                        }
                        else
                        {
                            word_list.Add( word );
                        }  
                    }
                }
            }
            return (word_list.ToArray());
        }
    }
}
