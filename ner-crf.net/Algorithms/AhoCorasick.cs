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
        public ngram_t( NerOutputType[] nerOutputTypes, NerOutputType resultNerOutputType )
        {
            NerOutputTypes      = nerOutputTypes;
            ResultNerOutputType = resultNerOutputType;
        }

        public NerOutputType[] NerOutputTypes      { get; private set; }
        public NerOutputType   ResultNerOutputType { get; private set; }

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
        /// <summary>
        /// 
        /// </summary>
        public sealed class Comparer : IComparer< SearchResult >
        {
            public static readonly Comparer Instance = new Comparer();
            private Comparer() { }

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

        public SearchResult( int startIndex, int length, NerOutputType nerOutputType )
        {
            StartIndex    = startIndex;
            Length        = length;
            NerOutputType = nerOutputType;
        }

        public int           StartIndex    { get; private set; }
        public int           Length        { get; private set; }
        public NerOutputType NerOutputType { get; private set; }

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
        /// <summary>
        /// Tree node representing character and its transition and failure function
        /// </summary>
        private sealed class TreeNode
        {
            /// <summary>
            /// 
            /// </summary>
            private sealed class ngram_t_IEqualityComparer : IEqualityComparer< ngram_t >
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

            /// <summary>
            /// Build tree from specified keywords
            /// </summary>
            public static TreeNode BuildTree( IList< ngram_t > ngrams )
            {
                // Build keyword tree and transition function
                var root = new TreeNode();
                foreach ( var ngram in ngrams )
                {
                    // add pattern to tree
                    var node = root;
                    foreach ( var nerOutputType in ngram.NerOutputTypes )
                    {
                        var nodeNew = node.GetTransition( nerOutputType );
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
                        var nerOutputType = node.NerOutputType;

                        while ( (r != null) && !r.ContainsTransition( nerOutputType ) )
                        {
                            r = r.Failure;
                        }
                        if ( r == null )
                        {
                            node.Failure = root;
                        }
                        else
                        {
                            node.Failure = r.GetTransition( nerOutputType );
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
            public static TreeNode BuildTree( ngram_t ngram )
            {
                var root = new TreeNode();
                // Build keyword tree and transition function
                {
                    // add pattern to tree
                    var node = root;
                    foreach ( var nerOutputType in ngram.NerOutputTypes )
                    {
                        var nodeNew = node.GetTransition( nerOutputType );
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
                        var nerOutputType = node.NerOutputType;

                        while ( (r != null) && !r.ContainsTransition( nerOutputType ) )
                        {
                            r = r.Failure;
                        }
                        if ( r == null )
                        {
                            node.Failure = root;
                        }
                        else
                        {
                            node.Failure = r.GetTransition( nerOutputType );
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
            /// Initialize tree node with specified value
            /// </summary>
            public TreeNode() : this( null, NerOutputType.O )
            {
            }
            public TreeNode( TreeNode parent, NerOutputType nerOutputType )
            {
                NerOutputType = nerOutputType;
                Parent        = parent;                                
            }

            /// <summary>
            /// Adds pattern ending in this node
            /// </summary>
            /// <param name="ngram">Pattern</param>
            public void AddNgram( ngram_t ngram )
            {
                if ( _Ngrams == null )
                {
                    _Ngrams = new HashSet< ngram_t >( ngram_t_IEqualityComparer.Instance );
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
                    _TransDict = new Dictionary< NerOutputType, TreeNode >();
                }
                _TransDict.Add( node.NerOutputType, node );
            }

            /// <summary>
            /// Returns transition to specified character (if exists)
            /// </summary>
            /// <param name="nerOutputType">NerOutputType</param>
            /// <returns>Returns TreeNode or null</returns>
            public TreeNode GetTransition( NerOutputType nerOutputType )
            {
                TreeNode node;
                if ( (_TransDict != null) && _TransDict.TryGetValue( nerOutputType, out node ) )
                    return (node);
                return (null);
            }

            /// <summary>
            /// Returns true if node contains transition to specified character
            /// </summary>
            /// <param name="nerOutputType">NerOutputType</param>
            /// <returns>True if transition exists</returns>
            public bool ContainsTransition( NerOutputType nerOutputType )
            {
                return ((_TransDict != null) && _TransDict.ContainsKey( nerOutputType ));
            }
            #endregion

            #region [.properties.]
            private Dictionary< NerOutputType, TreeNode > _TransDict;
            private HashSet< ngram_t > _Ngrams;

            /// <summary>
            /// NerOutputType
            /// </summary>
            public NerOutputType NerOutputType { get; private set; }

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
            public ICollection< TreeNode > Transitions { get { return ((_TransDict != null) ? _TransDict.Values : null); } }

            /// <summary>
            /// Returns list of patterns ending by this letter
            /// </summary>
            public ICollection< ngram_t > Ngrams { get { return (_Ngrams); } }
            public bool HasNgrams { get { return (_Ngrams != null); } }
            #endregion

            public override string ToString()
            {
                return ( ((Parent != null) ? ('\'' + NerOutputType.ToString() + '\'') : "ROOT") +
                         ", transitions(descendants): " + ((_TransDict != null) ? _TransDict.Count : 0) + ", ngrams: " + ((_Ngrams != null) ? _Ngrams.Count : 0)
                       );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private struct Finder
        {
            private TreeNode _Root;
            private TreeNode _Node;
            public static Finder Create( TreeNode root ) => new Finder() { _Root = root, _Node = root };

            public bool Find( word_t word, out TreeNode node )
            {
                TreeNode transNode;
                do
                {
                    if ( word.IsWordInNerChain ) //---if ( word.Tag == DONT_MERGE_WITH_NAME_ANOTHER )
                    {
                        node = null;
                        return (false); //goto SKIP_WORD;
                    }
                    transNode = _Node.GetTransition( word.nerOutputType );
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
                node = _Node;
                return (true);
            }
        }

        #region [.private field's.]
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
        public AhoCorasick( IList< ngram_t > ngrams )
        {
            _Root = TreeNode.BuildTree( ngrams );
        }
        public AhoCorasick( ngram_t ngram )
        {
            _Root = TreeNode.BuildTree( ngram );
        }
        #endregion

        #region [.public method's & properties.]
        public SearchResult? FindFirst( List< word_t > words )
        {
            var ss = FindAllInternal( words );
            if ( ss != null )
            {
                return (ss.Min);
            }
            return (null);
        }
        public ICollection< SearchResult > FindAll( List< word_t > words )
        {
            return (FindAllInternal( words ));
        }
        private SortedSet< SearchResult > FindAllInternal( List< word_t > words )
        {            
            var ss = default(SortedSet< SearchResult >);
            var finder = Finder.Create( _Root );

            for ( int index = 0, len = words.Count; index < len; index++ )
            {
                if ( finder.Find( words[ index ], out var node ) && node.HasNgrams )
                {
                    if ( ss == null ) ss = new SortedSet< SearchResult >( SearchResult.Comparer.Instance );

                    foreach ( var ngram in node.Ngrams )
                    {
                        ss.Add( new SearchResult( index - ngram.NerOutputTypes.Length + 1, ngram.NerOutputTypes.Length, ngram.ResultNerOutputType ) );
                    }
                }
            }
            return (ss);
        }
        #endregion

        public override string ToString()
        {
            return ("[" + _Root + "]");
        }
    }
}
