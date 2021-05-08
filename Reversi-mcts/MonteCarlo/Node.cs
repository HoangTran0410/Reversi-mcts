using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    public class Node
    {
        public ulong Move { get; set; }
        public State State { get; set; }
        public Node Parent { get; set; }
        public Dictionary<ulong, Node> Children { get; set; }
        // Why use List<T>: https://stackoverflow.com/a/42753399/11898496
        // Why use Dictionary instead of List: https://stackoverflow.com/a/16977769/11898496

        public int Visits { get; set; }
        public int Wins { get; set; }

        public Node(Node parent, ulong move, State state, List<ulong> unexpandedPlays)
        {
            Move = move;
            State = state;

            // Monte Carlo stuff
            Visits = 0;
            Wins = 0;

            // Tree stuff
            Parent = parent;
            Children = new Dictionary<ulong, Node>();
            foreach (ulong _move in unexpandedPlays)
            {
                Children.Add(_move, null); // _move is the key of dictionary
            }
        }
    }

    public static class NodeExtensions
    {

        /// <summary>
        /// Get the Node corresponding to the given play.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="move">The move leading to the child node.</param>
        /// <returns></returns>
        public static Node ChildNode(this Node node, ulong move)
        {
            return node.Children.GetValueOrDefault(move); // move is key of dictionary
        }

        /// <summary>
        /// <para>Expand the specified child play and return the new child node.</para>
        /// <para>Add the node to the array of children nodes.</para>
        /// <para>Remove the play from the array of unexpanded plays.</para>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="play">The play to expand.</param>
        /// <param name="childState">The child state corresponding to the given play.</param>
        /// <param name="unexpandedPlays">The given child's unexpanded child plays; typically all of them.</param>
        /// <returns></returns>
        public static Node Expand(this Node node, ulong play, State childState, List<ulong> unexpandedPlays)
        {
            if (!node.Children.ContainsKey(play)) return null;

            Node childNode = new Node(node, play, childState, unexpandedPlays);
            node.Children[play] = childNode;
            return childNode;
        }

        /// <summary>
        /// Get all legal plays from this node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static List<ulong> AllMoves(this Node node)
        {
            // https://stackoverflow.com/a/1276792/11898496
            return new List<ulong>(node.Children.Keys);
        }

        /// <summary>
        /// Get all unexpanded legal moves from this node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static List<ulong> UnexpandedMoves(this Node node)
        {
            List<ulong> ret = new List<ulong>();
            foreach (ulong key in node.Children.Keys)
            {
                if (node.Children[key] == null) ret.Add(key);
            }
            return ret;
        }

        /// <summary>
        /// Whether this node is fully expanded.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool IsFullyExpanded(this Node node)
        {
            foreach (Node child in node.Children.Values)
            {
                if (child == null) return false;
            }
            return true;
        }

        /// <summary>
        /// Whether this node is terminal in the game tree, NOT INCLUSIVE of termination due to winning.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool IsLeaf(this Node node)
        {
            return node.Children.Count == 0;
        }

        /// <summary>
        /// Get the UCB1 value for this node.
        /// </summary>
        /// <param name="node">The square of the bias parameter in the UCB1 algorithm, default tp sqrt(2)</param>
        /// <param name="biasParam"></param>
        /// <returns></returns>
        public static double GetUCB1(this Node node, double biasParam = 1.41)
        {
            return (node.Wins / node.Visits) + Math.Sqrt(biasParam * Math.Log(node.Parent.Visits) / node.Visits);
        }
    }
}
