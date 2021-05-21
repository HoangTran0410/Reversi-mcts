using Reversi_mcts.Board;
using Reversi_mcts.MachineLearning;

namespace Reversi_mcts.MonteCarlo
{
    public static class Node1Extensions
    {
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
            {
                if (selectPos <= wheel[i])
                {
                    return listLegalMoves[i];
                }
            }

            return 0UL;
        }
    }
}