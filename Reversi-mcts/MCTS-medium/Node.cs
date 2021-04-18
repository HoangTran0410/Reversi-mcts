using System;
using System.Collections.Generic;

namespace Reversi_mcts.MCTS_medium
{
    class Node
    {
        public Node parent { get; }
        public ReversiState state { get; }

        public Dictionary<string, Tuple<Move, Node>> children { get; }
        Move move; // TODO có cần dùng tới cái này ko ?

        public int n_plays { get; set; }
        public int n_wins { get; set; }

        /// <summary>
        ///     Create a new MonteCarloNode in the search tree.
        /// </summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="play">Last play played to get to this state.</param>
        /// <param name="state">The corresponding state.</param>
        /// <param name="unexpandedPlays">The node's unexpanded child plays.</param>
        public Node(Node parent, Move play, ReversiState state, List<Move> unexpandedPlays)
        {
            this.move = play;
            this.state = state;

            // Monte Carlo stuff
            this.n_plays = 0;
            this.n_wins = 0;

            // Tree stuff
            this.parent = parent;
            this.children = new Dictionary<string, Tuple<Move, Node>>();
            foreach (Move _move in unexpandedPlays)
            {
                this.children.Add(_move.hash(), new Tuple<Move, Node>(_move, null));
            }
        }

        /// <summary>
        ///     Get the MonteCarloNode corresponding to the given play.
        /// </summary>
        /// <param name="play">The play leading to the child node.</param>
        /// <returns>The child node corresponding to the play given.</returns>
        public Node ChildNode(Move play)
        {
            Tuple<Move, Node> child = this.children.GetValueOrDefault(play.hash());
            return child.Item2;
        }

        /// <summary>
        /// Expand the specified child play and return the new child node.
        /// Add the node to the array of children nodes.
        /// Remove the play from the array of unexpanded plays.
        /// </summary>
        /// <param name="move">The play to expand.</param>
        /// <param name="childState">The child state corresponding to the given play.</param>
        /// <param name="unexpandedPlays">The given child's unexpanded child plays; typically all of them.</param>
        /// <returns>The new child node.</returns>
        public Node Expand(Move move, ReversiState childState, List<Move> unexpandedPlays)
        {
            if (!this.children.ContainsKey(move.hash())) return null;
            Node childNode = new Node(this, move, childState, unexpandedPlays);
            this.children[move.hash()] = new Tuple<Move, Node>(move, childNode);
            return childNode;
        }

        /// <summary>
        /// Get all legal plays from this node.
        /// </summary>
        /// <returns>All plays.</returns>
        public List<Move> AllMoves()
        {
            List<Move> ret = new List<Move>();

            foreach (Tuple<Move, Node> child in this.children.Values)
            {
                ret.Add(child.Item1);
            }
            return ret;
        }

        /// <summary>
        /// Get all unexpanded legal plays from this node.
        /// </summary>
        /// <returns>All unexpanded plays.</returns>
        public List<Move> UnexpandedMoves()
        {
            List<Move> ret = new List<Move>();
            foreach (Tuple<Move, Node> child in this.children.Values)
            {
                if (child.Item2 == null) ret.Add(child.Item1);
            }
            return ret;
        }

        /// <summary>
        /// Whether this node is fully expanded.
        /// </summary>
        /// <returns>Whether this node is fully expanded.</returns>
        public bool IsFullyExpanded()
        {
            foreach (Tuple<Move, Node> child in this.children.Values)
            {
                if (child.Item2 == null) return false;
            }
            return true;
        }

        /// <summary>
        /// Whether this node is terminal in the game tree, NOT INCLUSIVE of termination due to winning.
        /// </summary>
        /// <returns>Whether this node is a leaf in the tree.</returns>
        public bool IsLeaf()
        {
            return this.children.Count == 0;
        }

        /// <summary>
        /// Get the UCB1 value for this node.
        /// </summary>
        /// <param name="biasParam">The square of the bias parameter in the UCB1 algorithm, defaults to 2.</param>
        /// <returns>The UCB1 value of this node.</returns>
        public double GetUCB1(double biasParam)
        {
            return (this.n_wins / this.n_plays) + Math.Sqrt(biasParam * Math.Log(this.parent.n_plays) / this.n_plays);
        }
    }
}