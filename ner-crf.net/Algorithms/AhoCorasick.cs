/* Aho-Corasick text search algorithm for string's implementation
 * 
 * For more information visit
 *		- http://www.cs.uku.fi/~kilpelai/BSA05/lectures/slides04.pdf
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using lingvo.tokenizing;

namespace lingvo.ner
{
    /// <summary>
    /// 
    /// </summary>
    internal struct ngram_t
    {
        public ngram_t( NerOutputType[] nerOutputTypes, NerOutputType resultNerOutputType ) : this()
        {
            NerOutputTypes      = nerOutputTypes;
            ResultNerOutputType = resultNerOutputType;
        }

        public NerOutputType[] NerOutputTypes
        {
            get;
            private set;
        }
        public NerOutputType   ResultNerOutputType
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return ("nerOutputTypes: " + NerOutputTypes.Length + " => '" + ResultNerOutputType + "'");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal struct SearchResult
    {
        public SearchResult( int startIndex, int length, NerOutputType nerOutputType ) : this()
        {
            StartIndex = startIndex;
            Length     = length;
            NerOutputType        = nerOutputType;
        }

        public int           StartIndex
        {
            get;
            private set;
        }
        public int           Length
        {
            get;
            private set;
        }
        public NerOutputType NerOutputType
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return ("[" + StartIndex + ":" + Length + "], NerOutputType: '" + NerOutputType + "'");
        }
    }

    /// <summary>
    /// Class for searching string for one or multiple 
    /// keywords using efficient Aho-Corasick search algorithm
    /// </summary>
    internal sealed class AhoCorasick
    {
        internal const byte DONT_MERGE_WITH_NAME_ANOTHER = 0xFF;

        /// <summary>
        /// Tree node representing character and its 
        /// transition and failure function
        /// </summary>
        private class TreeNode
        {
            /// <summary>
            /// 
            /// </summary>
            private class ngram_t_IEqualityComparer : IEqualityComparer< ngram_t >
            {
                public static readonly ngram_t_IEqualityComparer Instance = new ngram_t_IEqualityComparer();
                private ngram_t_IEqualityComparer() { }

                #region [.IEqualityComparer< ngram_t >.]
                public bool Equals( ngram_t x, ngram_t y )
                {
                    var len = x.NerOutputTypes.Length;
                    if ( len != y.NerOutputTypes.Length )
                    {
                        return (false);
                    }

                    for ( int i = 0; i < len; i++ )
                    {
                        if ( x.NerOutputTypes[ i ] != y.NerOutputTypes[ i ] )
                        {
                            return (false);
                        }
                    }
                    return (true);
                }

                public int GetHashCode( ngram_t obj )
                {
                    return (obj.NerOutputTypes.Length.GetHashCode());
                }
                #endregion
            }

            #region [.ctor() & methods.]
            /// <summary>
            /// Initialize tree node with specified value
            /// </summary>
            public TreeNode() : this( null, NerOutputType.O )
            {
            }
            public TreeNode( TreeNode parent, NerOutputType nerOutputType )
            {
                NerOutputType = nerOutputType;
                Parent        = parent;
                Ngrams = new HashSet< ngram_t >( ngram_t_IEqualityComparer.Instance );

                Transitions      = new TreeNode[ 0 ];
                _TransDictionary = new Dictionary< NerOutputType, TreeNode >();
            }

            /// <summary>
            /// Adds pattern ending in this node
            /// </summary>
            /// <param name="ngram">Pattern</param>
            public void AddNgram( ngram_t ngram )
            {
                Ngrams.Add( ngram );
            }

            /// <summary>
            /// Adds trabsition node
            /// </summary>
            /// <param name="node">Node</param>
            public void AddTransition( TreeNode node )
            {
                _TransDictionary.Add( node.NerOutputType, node );
                var nodes = new TreeNode[ _TransDictionary.Values.Count ];
                _TransDictionary.Values.CopyTo( nodes, 0 );
                Transitions = nodes;
            }

            /// <summary>
            /// Returns transition to specified character (if exists)
            /// </summary>
            /// <param name="c">Character</param>
            /// <returns>Returns TreeNode or null</returns>
            public TreeNode GetTransition( NerOutputType nerOutputType )
            {
                var node = default(TreeNode);
                if ( _TransDictionary.TryGetValue( nerOutputType, out node ) )
                    return (node);
                return (null);
            }

            /// <summary>
            /// Returns true if node contains transition to specified character
            /// </summary>
            /// <param name="c">Character</param>
            /// <returns>True if transition exists</returns>
            public bool ContainsTransition( NerOutputType nerOutputType )
            {
                return (_TransDictionary.ContainsKey( nerOutputType ));
            }
            #endregion

            #region [.properties.]
            private Dictionary< NerOutputType, TreeNode > _TransDictionary;

            /// <summary>
            /// Character
            /// </summary>
            public NerOutputType NerOutputType
            {
                get;
                private set;
            }

            /// <summary>
            /// Parent tree node
            /// </summary>
            public TreeNode Parent
            {
                get;
                private set;
            }

            /// <summary>
            /// Failure function - descendant node
            /// </summary>
            public TreeNode Failure
            {
                get;
                internal set;
            }

            /// <summary>
            /// Transition function - list of descendant nodes
            /// </summary>
            public TreeNode[] Transitions
            {
                get;
                private set;
            }

            /// <summary>
            /// Returns list of patterns ending by this letter
            /// </summary>
            public HashSet< ngram_t > Ngrams
            {
                get;
                private set;
            }
            #endregion

            public override string ToString()
            {
                return (
                    ((Parent != null) ? ('\'' + NerOutputType.ToString() + '\'') : "ROOT") +
                    ", transitions(descendants): " + Transitions.Length + ", ngrams: " + Ngrams.Count
                    );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class SearchResultIComparer : IComparer< SearchResult >
        {
            public static readonly SearchResultIComparer Instance = new SearchResultIComparer();
            private SearchResultIComparer() { }

            #region [.IComparer< SearchResult >.]
            public int Compare( SearchResult x, SearchResult y )
            {
                var d = y.Length - x.Length;
                if ( d != 0 )
                    return (d);

                d = x.StartIndex - y.StartIndex;
                if ( d != 0 )
                    return (d);

                return (y.NerOutputType - x.NerOutputType);
            }
            #endregion
        }

        #region [.private field's.]
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
        public AhoCorasick( IList< ngram_t > ngrams )
        {
            _Root = new TreeNode();
            Count = ngrams.Count;
            BuildTree( ngrams );
        }
        public AhoCorasick( ngram_t ngram )
        {
            _Root = new TreeNode();
            Count = 1;
            BuildTree( ngram );
        }
        #endregion

        #region [.private method's - implementation.]
        /// <summary>
        /// Build tree from specified keywords
        /// </summary>
        private void BuildTree( IList< ngram_t > ngrams )
        {
            // Build keyword tree and transition function
            //---_Root = new TreeNode( null, null );
            foreach ( ngram_t ngram in ngrams )
            {
                // add pattern to tree
                TreeNode node = _Root;
                foreach ( var nerOutputType in ngram.NerOutputTypes )
                {
                    TreeNode nodeNew = null;
                    foreach ( TreeNode trans in node.Transitions )
                    {
                        if ( trans.NerOutputType == nerOutputType )
                        {
                            nodeNew = trans;
                            break;
                        }
                    }

                    if ( nodeNew == null )
                    {
                        nodeNew = new TreeNode( node, nerOutputType );
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
                    var nerOutputType = node.NerOutputType;

                    while ( r != null && !r.ContainsTransition( nerOutputType ) )
                    {
                        r = r.Failure;
                    }
                    if ( r == null )
                    {
                        node.Failure = _Root;
                    }
                    else
                    {
                        node.Failure = r.GetTransition( nerOutputType );
                        foreach ( ngram_t result in node.Failure.Ngrams )
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

        private void BuildTree( ngram_t ngram )
        {
            // Build keyword tree and transition function
            {
                // add pattern to tree
                TreeNode node = _Root;
                foreach ( var nerOutputType in ngram.NerOutputTypes )
                {
                    TreeNode nodeNew = null;
                    foreach ( TreeNode trans in node.Transitions )
                    {
                        if ( trans.NerOutputType == nerOutputType )
                        {
                            nodeNew = trans;
                            break;
                        }
                    }

                    if ( nodeNew == null )
                    {
                        nodeNew = new TreeNode( node, nerOutputType );
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
                    var nerOutputType = node.NerOutputType;

                    while ( r != null && !r.ContainsTransition( nerOutputType ) )
                    {
                        r = r.Failure;
                    }
                    if ( r == null )
                    {
                        node.Failure = _Root;
                    }
                    else
                    {
                        node.Failure = r.GetTransition( nerOutputType );
                        foreach ( ngram_t result in node.Failure.Ngrams )
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
        public int Count
        {
            get;
            private set;
        }

        public SearchResult? /*ICollection< SearchResult >*/ FindFirst( List< word_t > words )
        {            
            var searchResults = default(SortedSet< SearchResult >);

            TreeNode node = _Root;

            for ( int index = 0, len = words.Count; index < len; index++ )
            {
                TreeNode trans = null;
                while ( trans == null )
                {
                    var word = words[ index ];
                    if ( word.IsWordInNerChain ) //---if ( word.Tag == DONT_MERGE_WITH_NAME_ANOTHER )
                        goto SKIP_WORD;
                    trans = node.GetTransition( word.nerOutputType );
                    if ( node == _Root ) 
                        break;
                    if ( trans == null ) 
                        node = node.Failure;
                }
                if ( trans != null ) 
                    node = trans;

                if ( 0 < node.Ngrams.Count )
                {
                    if ( searchResults == null )
                    {
                        searchResults = new SortedSet< SearchResult >( SearchResultIComparer.Instance );
                    }

                    foreach ( var ngram in node.Ngrams )
                    {
                        searchResults.Add( new SearchResult( index - ngram.NerOutputTypes.Length + 1, ngram.NerOutputTypes.Length, ngram.ResultNerOutputType ) );
                    }
                }
            SKIP_WORD:
                ;
            }
            if ( searchResults != null )
            {
                return (searchResults.Min);
            }
            return (null);
        }
        public ICollection< SearchResult > FindAll( List< word_t > words )
        {            
            var searchResults = default(SortedSet< SearchResult >);

            TreeNode node = _Root;

            for ( int index = 0, len = words.Count; index < len; index++ )
            {
                TreeNode trans = null;
                while ( trans == null )
                {
                    var word = words[ index ];
                    if ( word.IsWordInNerChain ) //---if ( word.Tag == DONT_MERGE_WITH_NAME_ANOTHER )
                        goto SKIP_WORD;
                    trans = node.GetTransition( word.nerOutputType );
                    if ( node == _Root ) 
                        break;
                    if ( trans == null ) 
                        node = node.Failure;
                }
                if ( trans != null ) 
                    node = trans;

                if ( 0 < node.Ngrams.Count )
                {
                    if ( searchResults == null )
                    {
                        searchResults = new SortedSet< SearchResult >( SearchResultIComparer.Instance );
                    }

                    foreach ( var ngram in node.Ngrams )
                    {
                        searchResults.Add( new SearchResult( index - ngram.NerOutputTypes.Length + 1, ngram.NerOutputTypes.Length, ngram.ResultNerOutputType ) );
                    }
                }
            SKIP_WORD:
                ;
            }
            return (searchResults);
        }
        #endregion

        public override string ToString()
        {
            return ("[" + _Root + "], count: " + Count);
        }
    }
}
