using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace lingvo.sentsplitting
{
    /// <summary>
    /// 
    /// </summary>
    internal readonly struct ngram_t< T >
    {
        public ngram_t( string[] _words, T _value )
        {
            words = _words;
            value = _value;
        }
        public string[] words { get; }
        public T        value { get; }

        public override string ToString() => ('\'' + string.Join( "' '", words ) + "' (" + words.Length + "), '" + value + "'");
    }

    /// <summary>
    /// 
    /// </summary>
    internal readonly struct SearchResult< T >
    {
        /// <summary>
        /// 
        /// </summary>
        public sealed class Comparer : IComparer< SearchResult< T > >
        {
            public static Comparer Inst { get; } = new Comparer();
            private Comparer() { }
            public int Compare( SearchResult< T > x, SearchResult< T > y )
            {
                var d = y.Length - x.Length;
                if ( d != 0 )
                    return (d);

                return (x.StartIndex - y.StartIndex);

                //d = x.StartIndex - y.StartIndex;
                //if ( d != 0 )
                    //return (d);

                //return (y.Value - x.Value);
            }
        }

        public SearchResult( int startIndex, int length, T value )
        {
            StartIndex = startIndex;
            Length     = length;
            v          = value;
        }

        public int StartIndex { get; }
        public int Length     { get; }
        public T   v          { get; }

        public override string ToString()
        {
            var s = v.ToString();
            if ( string.IsNullOrEmpty( s ) )
            {
                return ("[" + StartIndex + ":" + Length + "]");
            }
            return ("[" + StartIndex + ":" + Length + "], value: '" + s + "'");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal readonly struct SearchResultOfHead2Left< T >
    {
        /// <summary>
        /// 
        /// </summary>
        public sealed class Comparer : IComparer< SearchResultOfHead2Left< T > >
        {
            public static Comparer Inst { get; } = new Comparer();
            private Comparer() { }
            public int Compare( SearchResultOfHead2Left< T > x, SearchResultOfHead2Left< T > y ) => (y.Length - x.Length);
        }

        public SearchResultOfHead2Left( ss_word_t lastWord, int length, T value )
        {
            LastWord = lastWord;
            Length   = length;
            v        = value;
        }

        public ss_word_t LastWord { get; }
        public int       Length   { get; }
        public T         v        { get; }

        public override string ToString()
        {
            var s = v.ToString();
            if ( string.IsNullOrEmpty( s ) )
            {
                return ("[0:" + Length + "]");
            }
            return ("[0:" + Length + "], value: '" + s + "'");
        }
    }

    /// <summary>
    /// Class for searching string for one or multiple keywords using efficient Aho-Corasick search algorithm
    /// </summary>
    internal sealed class Searcher< T >
    {
        /// <summary>
        /// Tree node representing character and its transition and failure function
        /// </summary>
        private sealed class TreeNode
        {
            /// <summary>
            /// 
            /// </summary>
            private sealed class ngram_t_equalityComparer : IEqualityComparer< ngram_t< T > >
            {
                public static ngram_t_equalityComparer Inst { get; } = new ngram_t_equalityComparer();
                private ngram_t_equalityComparer() { }
                public bool Equals( ngram_t< T > x, ngram_t< T > y )
                {
                    var len = x.words.Length;
                    if ( len != y.words.Length )
                    {
                        return (false);
                    }

                    for ( int i = 0; i < len; i++ )
                    {
                        if ( !string.Equals( x.words[ i ], y.words[ i ] ) )
                        {
                            return (false);
                        }
                    }
                    return (true);
                }
                public int GetHashCode( ngram_t< T > obj ) => obj.words.Length;
            }

            /// <summary>
            /// Build tree from specified keywords
            /// </summary>
            public static TreeNode BuildTree( IEnumerable< ngram_t< T > > ngrams )
            {
                // Build keyword tree and transition function
                var root = new TreeNode( null, null );
                foreach ( var ngram in ngrams )
                {
                    // add pattern to tree
                    var node = root;
                    foreach ( var word in ngram.words )
                    {
                        var nodeNew = node.GetTransition( word );
                        if ( nodeNew == null )
                        {
                            nodeNew = new TreeNode( node, word );
                            node.AddTransition( nodeNew );
                        }
                        node = nodeNew;
                    }
                    node.AddNgram( ngram );
                }

                // Find failure functions
                var nodes = new List< TreeNode >();
                // level 1 nodes - fail to root node
                var transitions_root_nodes = root.Transitions;
                if ( transitions_root_nodes != null )
                {
                    nodes.Capacity = transitions_root_nodes.Count;

                    foreach ( var node in transitions_root_nodes )
                    {
                        node.Failure = root;
                        var transitions_nodes = node.Transitions;
                        if ( transitions_nodes != null )
                        {
                            foreach ( var trans in transitions_nodes )
                            {
                                nodes.Add( trans );
                            }
                        }
                    }
                }

                // other nodes - using BFS
                while ( nodes.Count != 0 )
                {
                    var newNodes = new List< TreeNode >( nodes.Count );
                    foreach ( var node in nodes )
                    {
                        var r = node.Parent.Failure;
                        var word = node.Word;

                        while ( (r != null) && !r.ContainsTransition( word ) )
                        {
                            r = r.Failure;
                        }
                        if ( r == null )
                        {
                            node.Failure = root;
                        }
                        else
                        {
                            node.Failure = r.GetTransition( word );
                            var failure_ngrams = node.Failure.Ngrams;
                            if ( failure_ngrams != null )
                            {
                                foreach ( var ng in failure_ngrams )
                                {
                                    node.AddNgram( ng );
                                }
                            }
                        }

                        // add child nodes to BFS list 
                        var transitions_nodes = node.Transitions;
                        if ( transitions_nodes != null )
                        {
                            foreach ( var child in transitions_nodes )
                            {
                                newNodes.Add( child );
                            }
                        }
                    }
                    nodes = newNodes;
                }
                root.Failure = root;

                return (root);
            }

            #region [.ctor() & methods.]
            /// <summary>
            /// Initialize tree node with specified character
            /// </summary>
            /// <param name="parent">Parent node</param>
            /// <param name="word">word</param>
            public TreeNode( TreeNode parent, string word )
            {
                Word   = word;
                Parent = parent;                
            }

            /// <summary>
            /// Adds pattern ending in this node
            /// </summary>
            /// <param name="ngram">Pattern</param>
            public void AddNgram( ngram_t< T > ngram )
            {
                if ( _Ngrams == null ) _Ngrams = new HashSet< ngram_t< T > >( ngram_t_equalityComparer.Inst );
                _Ngrams.Add( ngram );
            }

            /// <summary>
            /// Adds transition node
            /// </summary>
            /// <param name="node">Node</param>
            public void AddTransition( TreeNode node )
            {
                if ( _TransDict == null ) _TransDict = new Dictionary< string, TreeNode >();
                _TransDict.Add( node.Word, node );
            }

            /// <summary>
            /// Returns transition to specified character (if exists)
            /// </summary>
            /// <param name="word">word</param>
            /// <returns>Returns TreeNode or null</returns>
            public TreeNode GetTransition( string word ) => (_TransDict != null) && _TransDict.TryGetValue( word, out var node ) ? node : null;

            /// <summary>
            /// Returns true if node contains transition to specified character
            /// </summary>
            /// <param name="c">Character</param>
            /// <returns>True if transition exists</returns>
            public bool ContainsTransition( string word ) => ((_TransDict != null) && _TransDict.ContainsKey( word ));
            #endregion

            #region [.props.]
            private Dictionary< string, TreeNode > _TransDict;
            private HashSet< ngram_t< T > > _Ngrams;

            /// <summary>
            /// Character
            /// </summary>
            public string Word { [M(O.AggressiveInlining)] get; private set; }

            /// <summary>
            /// Parent tree node
            /// </summary>
            public TreeNode Parent { [M(O.AggressiveInlining)] get; private set; }

            /// <summary>
            /// Failure function - descendant node
            /// </summary>
            public TreeNode Failure { [M(O.AggressiveInlining)] get; internal set; }

            /// <summary>
            /// Transition function - list of descendant nodes
            /// </summary>
            public ICollection< TreeNode > Transitions { [M(O.AggressiveInlining)] get => ((_TransDict != null) ? _TransDict.Values : null); }

            /// <summary>
            /// Returns list of patterns ending by this letter
            /// </summary>
            public ICollection< ngram_t< T > > Ngrams { [M(O.AggressiveInlining)] get => _Ngrams; }
            public bool HasNgrams { [M(O.AggressiveInlining)] get => (_Ngrams != null); }
            #endregion

            public override string ToString() => ((Word != null) ? ('\'' + Word + '\'') : "ROOT") + ", transitions(descendants): " + ((_TransDict != null) ? _TransDict.Count : 0) + ", ngrams: " + ((_Ngrams != null) ? _Ngrams.Count : 0);
        }

        /// <summary>
        /// 
        /// </summary>
        private struct Finder
        {
            private TreeNode _Root;
            private TreeNode _Node;
            public static Finder Create( TreeNode root ) => new Finder() { _Root = root, _Node = root };

            public TreeNode Find( string word )
            {
                TreeNode transNode;
                do
                {
                    transNode = _Node.GetTransition( word );
                    if ( _Node == _Root )
                    {
                        break;
                    }
                    if ( transNode == null )
                    {
                        _Node = _Node.Failure;
                    }
                }
                while ( transNode == null );
                if ( transNode != null )
                {
                    _Node = transNode;
                }
                return (_Node);
            }
        }

        #region [.private field's.]
        private static SearchResult< T >[]            EMPTY_RESULT_1 = new SearchResult< T >[ 0 ];
        private static SearchResultOfHead2Left< T >[] EMPTY_RESULT_2 = new SearchResultOfHead2Left< T >[ 0 ];
        /// <summary>
        /// Root of keyword tree
        /// </summary>
        private TreeNode _Root;
        #endregion

        #region [.ctor().]
        /// <summary>
        /// Initialize search algorithm (Build keyword tree)
        /// </summary>
        /// <param name="keywords">Keywords to search for</param>
        internal Searcher( IList< ngram_t< T > > ngrams )
        {
            _Root = TreeNode.BuildTree( ngrams );
            NgramMaxLength = (0 < ngrams.Count) ? ngrams.Max( ngram => ngram.words.Length ) : 0;
        }
        #endregion

        #region [.public method's & properties.]
        internal int NgramMaxLength { get; }

        internal ICollection< SearchResult< T > > FindAll( IList< ss_word_t > words )
        {
            var ss = default(SortedSet< SearchResult< T > >);
            var finder = Finder.Create( _Root );

            for ( int index = 0, len = words.Count; index < len; index++ )
            {
                var node = finder.Find( words[ index ].valueOriginal );

                if ( node.HasNgrams )
                {
                    if ( ss == null ) ss = new SortedSet< SearchResult< T > >( SearchResult< T >.Comparer.Inst );
                    
                    foreach ( var ngram in node.Ngrams )
                    {
                        var r = ss.Add( new SearchResult< T >( index - ngram.words.Length + 1, ngram.words.Length, ngram.value ) );
                        Debug.Assert( r );
                    }
                }
            }
            if ( ss != null )
            {
                return (ss);
            }
            return (EMPTY_RESULT_1);
        }
        internal ICollection< SearchResultOfHead2Left< T > > FindOfHead2Left( ss_word_t headWord )
        {
            var ss = default(SortedSet< SearchResultOfHead2Left< T > >);
            var finder = Finder.Create( _Root );            
            int index = 0;

            for ( var word = headWord; word != null; word = word.next )
            {
                var node = finder.Find( word.valueOriginal );

                if ( node.HasNgrams )
                {
                    foreach ( var ngram in node.Ngrams )
                    {
                        var wordIndex = index - ngram.words.Length + 1;
                        if ( wordIndex == 0 )
                        {
                            if ( ss == null ) ss = new SortedSet< SearchResultOfHead2Left< T > >( SearchResultOfHead2Left< T >.Comparer.Inst );

                            var r = ss.Add( new SearchResultOfHead2Left< T >( word, ngram.words.Length, ngram.value ) );
                            Debug.Assert( r );
                        }
                    }
                }
                index++;
            }
            if ( ss != null )
            {
                return (ss);
            }
            return (EMPTY_RESULT_2);
        }
        #endregion

        public override string ToString() => ("[" + _Root + "]");
    }
}
