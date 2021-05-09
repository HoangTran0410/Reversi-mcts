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
        
        // Why use List<T>: https://stackoverflow.com/a/42753399/11898496
        public List<Node> ChildNodes { get; set; }
        public List<ulong> UntriedMoves { get; set; }

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
            UntriedMoves = state.BitLegalMoves.ToListUlong();
            ChildNodes = new List<Node>(UntriedMoves.Count);
        }
    }

    public static class NodeExtensions
    {
        // Phase 1: SELECTION
        public static Node SelectChild(this Node node)
        {
            Node bestChild = null;
            var bestUcb1 = Double.MinValue;
            
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
        
        // Phase 2: EXPANSION
        public static Node ExpandChild(this Node node)
        {
            var i = Constant.Random.Next(node.UntriedMoves.Count);
            var move = node.UntriedMoves[i];
            node.UntriedMoves.RemoveAt(i); // Untried -> Try

            var child = new Node(
                node.State.NextState(move), // code cũ NextState(node.Move) => phải dùng tới debug mới phát hiện sai
                node,
                move
            );
            node.ChildNodes.Add(child);

            return child;
        }
        
        // Phase 3: SIMULATION
        public static float Simulate(this Node node, byte player)
        {
            State state = new State(node.State);

            while (!state.IsTerminal())
            {
                ulong move = state.GetRandomMove();
                state = state.NextState(move);
            }

            return GetScore(state, player);
        }

        // Phase 3: BACKPROPAGATION
        public static void Backpropagate(this Node node, float result)
        {
            for (Node tempMode = node; tempMode != null; tempMode = tempMode.Parent)
            {
                tempMode.Update(result);
            }
        }

        public static void Update(this Node node, float result)
        {
            node.Wins += result;
            node.Visits++;
        }
        
        private static float GetScore(State state, byte player)
        {
            byte blackPieces = state.Board.CountPieces(Constant.Black);
            byte whitePieces = state.Board.CountPieces(Constant.White);
            bool isPlayerTurn = state.Player == player;

            // Draw
            if (blackPieces == whitePieces) return Constant.DrawScore;
            
            // Black Win
            if (blackPieces > whitePieces)
            {
                if (isPlayerTurn) return player == Constant.Black ? Constant.WinScore : Constant.LoseScore;
                return player == Constant.Black ? Constant.LoseScore : Constant.WinScore;
            }
            
            // White Win
            if (isPlayerTurn) return player == Constant.White ? Constant.WinScore : Constant.LoseScore;
            return player == Constant.White ? Constant.LoseScore : Constant.WinScore;
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
        
        private static double GetUcb1(this Node node, double biasParam = 2)
        {
            return node.Wins / node.Visits + Math.Sqrt(biasParam * Math.Log(node.Parent.Visits) / node.Visits);
        }
    }
}
