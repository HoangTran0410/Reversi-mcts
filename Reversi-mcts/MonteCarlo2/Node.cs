using System;
using System.Collections.Generic;
using Reversi_mcts.Board;

namespace Reversi_mcts.MonteCarlo2
{
    public class Node
    {
        public State State { get; }
        public ulong ParentMove { get; }
        public Node ParentNode { get; }

        // Why use List<T>: https://stackoverflow.com/a/42753399/11898496
        public List<Node> ChildNodes { get; }
        public List<ulong> UntriedMoves { get; }

        public int Visits { get; set; }
        public float Wins { get; set; }

        public Node(State state, Node parentNode, ulong parentMove)
        {
            State = state;
            ParentMove = parentMove;

            // Monte Carlo stuff
            Visits = 0;
            Wins = 0;

            // Tree stuff
            ParentNode = parentNode;
            UntriedMoves = state.GetListLegalMoves();
            ChildNodes = new List<Node>(UntriedMoves.Count);
        }
    }

    public static class NodeExtensions
    {
        // Phase 1: SELECTION
        public static Node SelectChild(this Node node, byte rootPlayer)
        {
            var isRootPlayer = node.State.Player == rootPlayer;

            Node bestChild = null;
            var maxUcb1 = double.MinValue;
            foreach (var childNode in node.ChildNodes)
            {
                var childUcb1 = childNode.GetUcb1(isRootPlayer);
                if (childUcb1 > maxUcb1)
                {
                    bestChild = childNode;
                    maxUcb1 = childUcb1;
                }
            }

            return bestChild;
        }

        private static double GetUcb1(this Node node, bool isRootPlayer)
        {
            var wins = isRootPlayer ? node.Wins : node.Visits - node.Wins;
            return wins / node.Visits + Constant.C * Math.Sqrt(Math.Log(node.ParentNode.Visits) / node.Visits);
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

            var winner = state.Winner();
            if (winner == Constant.Draw) return Constant.DrawScore;
            return winner == rootPlayer ? Constant.WinScore : Constant.LoseScore;
        }

        // Phase 4: BACKPROPAGATION
        public static void BackPropagate(this Node node, float reward)
        {
            while (node != null)
            {
                node.Wins += reward;
                node.Visits++;

                node = node.ParentNode;
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