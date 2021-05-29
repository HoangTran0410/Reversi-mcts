using System;
using System.Collections.Generic;
using Reversi_mcts.Board;
using Reversi_mcts.MachineLearning;

namespace Reversi_mcts.MonteCarlo
{
    public static class Node1Extensions
    {
        // Phase 1: SELECTION Kết hợp với dữ liệu BTMM trained
        public static Node SelectChildBTMM(this Node node, byte rootPlayer)
        {
            var isRootPlayer = node.State.Player == rootPlayer;

            Node bestChild = null;
            var maxUcb1 = double.MinValue;
            foreach (var childNode in node.ChildNodes)
            {
                var childUcb1 = childNode.GetUcbBias(isRootPlayer);
                if (childUcb1 > maxUcb1)
                {
                    bestChild = childNode;
                    maxUcb1 = childUcb1;
                }
            }

            return bestChild;
        }

        private static double GetUcbBias(this Node node, bool isRootPlayer)
        {
            var prob = node.Strength / node.ParentNode.AllChildStrength;
            var wins = isRootPlayer ? node.Wins : node.Visits - node.Wins;
            return wins / node.Visits
                   + Constant.C * Math.Sqrt(Math.Log(node.ParentNode.Visits) / node.Visits)
                   + Constant.Cbt * prob * Math.Sqrt(Constant.K / (node.ParentNode.Visits + Constant.K));
        }

        // Phase 2: EXPANSION kết hợp dữ liệu BTMM trained
        public static Node ExpandChildBTMM(this Node node)
        {
            // Tính & Lưu lại child Strength, để ko cần tính lại khi expand
            // Cộng child strength để tính allChildStrength luôn
            if (node.ChildStrengths == null)
            {
                node.ChildStrengths = new Dictionary<ulong, double>(node.UntriedMoves.Count);
                double allChildStrength = 0;
                foreach (var bitMove in node.UntriedMoves)
                {
                    double strength = BTMMAlgorithm.StrongOfAction(node.State, bitMove);
                    node.ChildStrengths.Add(bitMove, strength);
                    allChildStrength += strength;
                }

                node.AllChildStrength = allChildStrength;
            }

            // Expand random child
            var i = Constant.Random.Next(node.UntriedMoves.Count);
            var move = node.UntriedMoves[i];
            node.UntriedMoves.RemoveAt(i); // Untried -> Try

            var newState = node.State.Clone().NextState(move);
            var child = new Node(newState, node, move);
            child.Strength = node.ChildStrengths[move]; // dùng lại child strength đã được tính trước đó
            node.ChildNodes.Add(child);

            return child;
        }
        
        // Phase 3: SIMULATION Kết hợp với dữ liệu BTMM trained
        public static float SimulateBTMM(this Node node, byte rootPlayer)
        {
            var state = node.State.Clone();

            while (!state.IsTerminal())
            {
                var move = state.RouletteWheelSelection();
                state.NextState(move);
            }

            var winner = state.Winner();
            if (winner == Constant.Draw) return Constant.DrawScore;
            return winner == rootPlayer ? Constant.WinScore : Constant.LoseScore;
        }

        // Distribute probability based on Roulette wheel method
        private static ulong RouletteWheelSelection(this State state)
        {
            var listLegalMoves = state.GetArrayLegalMoves();
            var moveCount = listLegalMoves.Length;
            var wheel = new int[moveCount];
            var maxWheel = 0;

            if (moveCount == 1) return listLegalMoves[0];

            for (var i = 0; i < moveCount; i++)
            {
                // StrongOfAction nằm trong khoảng [0.01, 100], nên nhân 1000 để cast int chính xác hơn
                var temp = (int) (1000 * BTMMAlgorithm.StrongOfAction(state, listLegalMoves[i]));
                maxWheel += temp;
                wheel[i] = maxWheel;
            }

            var selectPos = Constant.Random.Next(0, maxWheel);
            for (var i = 0; i < moveCount; i++)
                if (selectPos <= wheel[i])
                    return listLegalMoves[i];

            return 0UL;
        }
    }
}