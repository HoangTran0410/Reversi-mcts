using System;
using System.Collections.Generic;

namespace Reversi_mcts.Core.MonteCarlo
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
        public float[] Wins { get; set; }

        public Node(State state, Node parentNode, ulong parentMove)
        {
            State = state;
            ParentMove = parentMove;

            // Monte Carlo stuff
            Visits = 0;
            Wins = new[] {0f, 0f};

            // Tree stuff
            ParentNode = parentNode;
            UntriedMoves = state.GetListLegalMoves();
            ChildNodes = new List<Node>(UntriedMoves.Count);
        }
    }

    public static class NodeExtensions
    {
        // Phase 1: SELECTION
        public static Node SelectChild(this Node node)
        {
            Node bestChild = null;
            var maxUcb1 = double.MinValue;
            foreach (var childNode in node.ChildNodes)
            {
                var childUcb1 = childNode.GetUcb1();
                if (childUcb1 > maxUcb1)
                {
                    bestChild = childNode;
                    maxUcb1 = childUcb1;
                }
            }

            return bestChild;
        }

        private static double GetUcb1(this Node node)
        {
            var wins = node.Wins[node.ParentNode.State.Player];
            var visits = node.Visits;
            var parentVisits = node.ParentNode.Visits;
            
            return wins / visits  + Constant.C * Math.Sqrt(Math.Log(parentVisits) / visits);
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
        // Returns a score vector.
        // * (1.0, 0.0) indicates a win for player black.
        // * (0.0, 1.0) indicates a win for player white.
        // * (0.5, 0.5) indicates a draw
        public static (float blackReward, float whiteReward) Simulate(this Node node)
        {
            var state = node.State; // TODO old code: state = new State(node.State)

            while (!state.IsTerminal())
            {
                var move = state.GetRandomMove();
                state = state.NextState(move);
            }

            var winner = state.Winner();
            return winner switch
            {
                Constant.Black => (Constant.WinScore, Constant.LoseScore),
                Constant.White => (Constant.LoseScore, Constant.WinScore),
                _ => (Constant.DrawScore, Constant.DrawScore)
            };
        }

        // Phase 4: BACKPROPAGATION
        public static void BackPropagate(this Node node, (float blackReward, float whiteReward) reward)
        {
            while (node != null)
            {
                node.Visits++;
                node.Wins[Constant.Black] += reward.blackReward;
                node.Wins[Constant.White] += reward.whiteReward;
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