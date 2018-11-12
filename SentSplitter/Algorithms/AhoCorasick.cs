using System.Collections.Generic;
using System.Linq;

namespace lingvo.sentsplitting
{
    /// <summary>
    /// 
    /// </summary>
    internal struct ngram_t< TValue >
    {
        public ngram_t( string[] _words, TValue _value )
        {
            words = _words;
            value = _value;
        }

        public string[] words { get; private set; }
        public TValue   value { get; private set; }

        public override string ToString()
        {
            return ('\'' + string.Join( "' '", words ) + "' (" + words.Length + "), '" + value + "'");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal struct SearchResult< TValue >
    {
        public SearchResult( int startIndex, int length, TValue value )
        {
            StartIndex = startIndex;
            Length     = length;
            v          = value;
        }

        public int    StartIndex { get; private set; }
        public int    Length     { get; private set; }
        public TValue v          { get; private set; }

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
    internal struct SearchResultOfHead2Left< TValue >
    {
        public SearchResultOfHead2Left( ss_word_t lastWord, int length, TValue value )
        {
            LastWord = lastWord;
            Length   = length;
            v        = value;
        }

        public ss_word_t LastWord { get; private set; }
        public int       Length   { get; private set; }
        public TValue    v        { get; private set; }

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
    internal sealed class AhoCorasick< TValue >
    {
        /// <summary>
        /// Tree node representing character and its transition and failure function
        /// </summary>
        private sealed class TreeNode
        {
            /// <summary>
            /// 
            /// </summary>
            private sealed class ngram_t_IEqualityComparer : IEqualityComparer< ngram_t< TValue > >
            {
                public static readonly ngram_t_IEqualityComparer Instance = new ngram_t_IEqualityComparer();
                private ngram_t_IEqualityComparer() { }

                #region [.IEqualityComparer< ngram_t >.]
                public bool Equals( ngram_t< TValue > x, ngram_t< TValue > y )
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

                public int GetHashCode( ngram_t< TValue > obj )
                {
                    return (obj.words.Length.GetHashCode());
                }
                #endregion
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
            public void AddNgram( ngram_t< TValue > ngram )
            {
                if ( _Ngrams == null )
                {
                    _Ngrams = new HashSet< ngram_t< TValue > >( ngram_t_IEqualityComparer.Instance );
                }
                _Ngrams.Add( ngram );
            }

            /// <summary>
            /// Adds trabsition node
            /// </summary>
            /// <param name="node">Node</param>
            public void AddTransition( TreeNode node )
            {
                if ( _TransDict == null )
                {
                    _TransDict = new Dictionary< string, TreeNode >();
                }
                _TransDict.Add( node.Word, node );
            }

            /// <summary>
            /// Returns transition to specified character (if exists)
            /// </summary>
            /// <param name="word">word</param>
            /// <returns>Returns TreeNode or null</returns>
            public TreeNode GetTransition( string word )
            {
                TreeNode node;
                if ( (_TransDict != null) && _TransDict.TryGetValue( word, out node ) )
                    return (node);
                return (null);
            }

            /// <summary>
            /// Returns true if node contains transition to specified character
            /// </summary>
            /// <param name="c">Character</param>
            /// <returns>True if transition exists</returns>
            public bool ContainsTransition( string word )
            {
                return ((_TransDict != null) && _TransDict.ContainsKey( word ));
            }
            #endregion

            #region [.properties.]
            private Dictionary< string, TreeNode > _TransDict;
            private HashSet< ngram_t< TValue > > _Ngrams;

            /// <summary>
            /// Character
            /// </summary>
            public string Word { get; private set; }

            /// <summary>
            /// Parent tree node
            /// </summary>
            public TreeNode Parent { get; private set; }

            /// <summary>
            /// Failure function - descendant node
            /// </summary>
            public TreeNode Failure { get; internal set; }

            /// <summary>
            /// Transition function - list of descendant nodes
            /// </summary>
            public IEnumerable< TreeNode > Transitions { get { return ((_TransDict != null) ? _TransDict.Values : Enumerable.Empty< TreeNode >()); } }

            /// <summary>
            /// Returns list of patterns ending by this letter
            /// </summary>
            public IEnumerable< ngram_t< TValue > > Ngrams { get { return (_Ngrams ?? Enumerable.Empty< ngram_t< TValue > >()); } }
            public bool HasNgrams { get { return (_Ngrams != null); } }
            #endregion

            public override string ToString()
            {
                return ( ((Word != null) ? ('\'' + Word + '\'') : "ROOT") +
                         ", transitions(descendants): " + ((_TransDict != null) ? _TransDict.Count : 0) + ", ngrams: " + ((_Ngrams != null) ? _Ngrams.Count : 0));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class SearchResultIComparer : IComparer< SearchResult< TValue > >
        {
            public static readonly SearchResultIComparer Instance = new SearchResultIComparer();
            private SearchResultIComparer() { }

            #region [.IComparer< SearchResult >.]
            public int Compare( SearchResult< TValue > x, SearchResult< TValue > y )
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
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class SearchResultOfHead2LeftIComparer : IComparer< SearchResultOfHead2Left< TValue > >
        {
            public static readonly SearchResultOfHead2LeftIComparer Instance = new SearchResultOfHead2LeftIComparer();
            private SearchResultOfHead2LeftIComparer() { }

            #region [.IComparer< SearchResultOfHead2Left >.]
            public int Compare( SearchResultOfHead2Left< TValue > x, SearchResultOfHead2Left< TValue > y )
            {
                return (y.Length - x.Length);
            }
            #endregion
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
        private readonly SearchResult< TValue >[]            EMPTY_RESULT_1 = new SearchResult< TValue >[ 0 ];
        private readonly SearchResultOfHead2Left< TValue >[] EMPTY_RESULT_2 = new SearchResultOfHead2Left< TValue >[ 0 ];
        /// <summary>
        /// Root of keyword tree
        /// </summary>
        private readonly TreeNode _Root;
        #endregion

        #region [.ctor().]
        /// <summary>
        /// Initialize search algorithm (Build keyword tree)
        /// </summary>
        /// <param name="keywords">Keywords to search for</param>
        internal AhoCorasick( IList< ngram_t< TValue > > ngrams )
        {
            _Root = new TreeNode( null, null );
            Count = ngrams.Count;
            if ( 0 < Count )
            {
                NgramMaxLength = ngrams.Max( ngram => ngram.words.Length );
                //ValuesMaxLength = ngrams.Max( ngram => ngram.words.Max( word => word.Length ) );
            }
            else
            {
                NgramMaxLength = 0;
                //ValuesMaxLength = 0;
            }            
            BuildTree( ngrams );
        }
        #endregion

        #region [.private method's - implementation.]
        /// <summary>
        /// Build tree from specified keywords
        /// </summary>
        private void BuildTree( IEnumerable< ngram_t< TValue > > ngrams )
        {
            // Build keyword tree and transition function
            //---_Root = new TreeNode( null, null );
            foreach ( ngram_t< TValue > ngram in ngrams )
            {
                // add pattern to tree
                TreeNode node = _Root;
                foreach ( string word in ngram.words )
                {
                    TreeNode nodeNew = null;
                    foreach ( TreeNode trans in node.Transitions )
                    {
                        if ( trans.Word == word )
                        {
                            nodeNew = trans;
                            break;
                        }
                    }

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
            foreach ( TreeNode node in _Root.Transitions )
            {
                node.Failure = _Root;
                foreach ( TreeNode trans in node.Transitions )
                {
                    nodes.Add( trans );
                }
            }
            // other nodes - using BFS
            while ( nodes.Count != 0 )
            {
                var newNodes = new List< TreeNode >();
                foreach ( TreeNode node in nodes )
                {
                    TreeNode r = node.Parent.Failure;
                    string word = node.Word;

                    while ( r != null && !r.ContainsTransition( word ) )
                    {
                        r = r.Failure;
                    }
                    if ( r == null )
                    {
                        node.Failure = _Root;
                    }
                    else
                    {
                        node.Failure = r.GetTransition( word );
                        foreach ( ngram_t< TValue > result in node.Failure.Ngrams )
                        {
                            node.AddNgram( result );
                        }
                    }

                    // add child nodes to BFS list 
                    foreach ( TreeNode child in node.Transitions )
                    {
                        newNodes.Add( child );
                    }
                }
                nodes = newNodes;
            }
            _Root.Failure = _Root;
        }
        #endregion

        #region [.public method's & properties.]
        internal int Count          { get; private set; }
        internal int NgramMaxLength { get; private set; }
        //internal int ValuesMaxLength { get; private set; }

        internal ICollection< SearchResult< TValue > > FindAll( DirectAccessList< ss_word_t > words )
        {
            var ss = default(SortedSet< SearchResult< TValue > >);
            var finder = Finder.Create( _Root );

            for ( int index = 0, len = words.Count; index < len; index++ )
            {
                var node = finder.Find( words._Items[ index ].valueOriginal );

                if ( node.HasNgrams )
                {
                    if ( ss == null ) ss = new SortedSet< SearchResult< TValue > >( SearchResultIComparer.Instance );
                    
                    foreach ( var ngram in node.Ngrams )
                    {
                        var r = ss.Add( new SearchResult< TValue >( index - ngram.words.Length + 1, ngram.words.Length, ngram.value ) );
                        System.Diagnostics.Debug.Assert( r );
                    }
                }
            }
            if ( ss != null )
            {
                return (ss);
            }
            return (EMPTY_RESULT_1);
        }
        internal ICollection< SearchResultOfHead2Left< TValue > > FindOfHead2Left( ss_word_t headWord )
        {
            var ss = default(SortedSet< SearchResultOfHead2Left< TValue > >);
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
                            if ( ss == null ) ss = new SortedSet< SearchResultOfHead2Left< TValue > >( SearchResultOfHead2LeftIComparer.Instance );

                            var r = ss.Add( new SearchResultOfHead2Left< TValue >( word, ngram.words.Length, ngram.value ) );
                            System.Diagnostics.Debug.Assert( r );
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

        public override string ToString()
        {
            return ("[" + _Root + "], count: " + Count);
        }
    }
}
