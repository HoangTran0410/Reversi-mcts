using System;
using System.Collections.Generic;
using Reversi_mcts.Core.Board;

namespace Reversi_mcts.Core.MonteCarlo
{
    public class Node
    {
        public ulong Move { get; }
        public State State { get; }
        public Node Parent { get; }

        // Why use List<T>: https://stackoverflow.com/a/42753399/11898496
        public List<Node> ChildNodes { get; }
        public List<ulong> UntriedMoves { get; }

        public int Visits { get; set; }
        public float Wins { get; set; }

        public Node(State state, Node parent, ulong move)
        {
            Move = move;
            State = state;

            // Monte Carlo stuff
            Visits = 0;
            Wins = 0;

            // Tree stuff
            Parent = parent;
            UntriedMoves = state.BitLegalMoves.ToListBitMove();
            ChildNodes = new List<Node>(UntriedMoves.Count);
        }
    }

    public static class NodeExtensions
    {
        // Phase 1: SELECTION
        public static Node SelectChild(this Node node)
        {
            Node bestChild = null;
            var bestUcb1 = double.MinValue;

            foreach (var childNode in node.ChildNodes)
            {
                // is continue/break bad: https://softwareengineering.stackexchange.com/a/58253
                var childUcb1 = childNode.GetUcb1();
                if (childUcb1 > bestUcb1)
                {
                    bestChild = childNode;
                    bestUcb1 = childUcb1;
                }
            }

            return bestChild;
        }

        private static double GetUcb1(this Node node)
        {
            return node.Wins / node.Visits + Math.Sqrt(2 * Math.Log(node.Parent.Visits) / node.Visits);
        }

        // Phase 2: EXPANSION
        public static Node ExpandChild(this Node node)
        {
            var i = Constant.Random.Next(node.UntriedMoves.Count);
            var move = node.UntriedMoves[i];
            node.UntriedMoves.RemoveAt(i); // Untried -> Try

            var newState = node.State.NextState(move);
            var child = new Node(newState, node, move);
            node.ChildNodes.Add(child);

            return child;
        }

        // Phase 3: SIMULATION
        public static float Simulate(this Node node, byte rootPlayer)
        {
            var state = node.State; // TODO old code: state = new State(node.State)

            while (!state.IsTerminal())
            {
                var move = state.GetRandomMove();
                state = state.NextState(move);
            }

            return GetScore(state, rootPlayer);
        }

        private static float GetScore(State state, byte rootPlayer)
        {
            var blackCount = state.Board.CountPieces(Constant.Black);
            var whiteCount = state.Board.CountPieces(Constant.White);

            // Draw
            if (blackCount == whiteCount) return Constant.DrawScore;

            // Not Draw
            var winner = blackCount > whiteCount ? Constant.Black : Constant.White;
            var score = rootPlayer == winner ? Constant.WinScore : Constant.LoseScore;

            return score;
        }

        // Phase 4: BACKPROPAGATION
        public static void BackPropagate(this Node node, float result)
        {
            for (var tempMode = node; tempMode != null; tempMode = tempMode.Parent)
            {
                tempMode.Wins += result;
                tempMode.Visits++;
            }
        }

        public static bool IsFullyExpanded(this Node node)
        {
            return node.UntriedMoves.Count == 0;
        }

        public static bool HasChildNode(this Node node)
        {
            return node.ChildNodes.Count != 0;
        }

        public static bool IsLeaf(this Node node)
        {
            return node.IsFullyExpanded() && !node.HasChildNode();
        }
    }
}