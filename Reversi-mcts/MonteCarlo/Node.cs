using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    public class Node
    {
        public State State { get; set; }
        public Node Parent { get; set; }
        public Move Move { get; set; }

        // Why use List<T>: https://stackoverflow.com/a/42753399/11898496
        // Why use Dictionary instead of List: https://stackoverflow.com/a/16977769/11898496
        public Dictionary<int, Tuple<Move, Node>> Children { get; set; }

        public int Visits { get; set; }
        public int Wins { get; set; }

        public Node(Node parent, Move play, State state, List<Move> unexpandedPlays)
        {
            Move = play;
            State = state;

            // Monte Carlo stuff
            Visits = 0;
            Wins = 0;

            // Tree stuff
            Parent = parent;
            Children = new Dictionary<int, Tuple<Move, Node>>();
            foreach (Move _move in unexpandedPlays)
            {
                Children.Add(_move.Hash(), new Tuple<Move, Node>(_move, null));
            }
        }
    }

    public static class NodeExtensions
    {

        public static Node ChildNode(this Node node, Move play)
        {
            Tuple<Move, Node> child = node.Children.GetValueOrDefault(play.Hash());
            return child.Item2;
        }

        public static Node Expand(this Node node, Move move, State childState, List<Move> unexpandedPlays)
        {
            if (!node.Children.ContainsKey(move.Hash())) return null;
            Node childNode = new Node(node, move, childState, unexpandedPlays);
            node.Children[move.Hash()] = new Tuple<Move, Node>(move, childNode);
            return childNode;
        }

        public static List<Move> AllMoves(this Node node)
        {
            List<Move> ret = new List<Move>();

            foreach (Tuple<Move, Node> child in node.Children.Values)
            {
                ret.Add(child.Item1);
            }
            return ret;
        }

        public static List<Move> UnexpandedMoves(this Node node)
        {
            List<Move> ret = new List<Move>();
            foreach (Tuple<Move, Node> child in node.Children.Values)
            {
                if (child.Item2 == null) ret.Add(child.Item1);
            }
            return ret;
        }

        public static bool IsFullyExpanded(this Node node)
        {
            foreach (Tuple<Move, Node> child in node.Children.Values)
            {
                if (child.Item2 == null) return false;
            }
            return true;
        }

        public static bool IsLeaf(this Node node)
        {
            return node.Children.Count == 0;
        }

        public static double GetUCB1(this Node node, double biasParam)
        {
            return (node.Wins / node.Visits) + Math.Sqrt(biasParam * Math.Log(node.Parent.Visits) / node.Visits);
        }




        //static readonly Random RandomGenerator = new Random();

        //public static Node GetRandomChildNode(this Node node)
        //{
        //    int index = RandomGenerator.Next(0, node.Children.Count);
        //    return node.Children[index];
        //}

        //public static Node GetChildWithMaxScore(this Node node)
        //{
        //    Node n = node.Children[0];
        //    int maxVisit = n.Visits;
        //    foreach (Node child in node.Children)
        //    {
        //        if (child.Visits > maxVisit)
        //        {
        //            n = child;
        //            maxVisit = child.Visits;
        //        }
        //    }
        //    return n;
        //}
    }
}
