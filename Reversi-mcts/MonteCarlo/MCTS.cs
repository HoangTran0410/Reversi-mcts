using System;
using Reversi_mcts.Board;
using Reversi_mcts.MachineLearning;
using Reversi_mcts.Utils;

namespace Reversi_mcts.MonteCarlo
{
    public static class Mcts
    {
        public static int LastPlayout { get; private set; }
        public static float LastWinPercentage { get; private set; }
        public static int LastRunTime { get; private set; }

        public static ulong RunSearch(Algorithm algoName, State state, int timeout)
        {
            // Nếu không có legal move => trả về pass move
            if (state.BitLegalMoves == 0) return 0UL;

            // Nếu chỉ có 1 legal move => trả về legal move đó luôn
            if (state.BitLegalMoves.PopCount() == 1) return state.BitLegalMoves;

            // Nếu có >2 legal moves => MCTS
            var move = algoName switch
            {
                Algorithm.Mcts => RunSearch(state, timeout),
                Algorithm.Mcts1 => RunSearch1(state, timeout),
                Algorithm.Mcts2 => throw new NotImplementedException(),
                _ => 0UL
            };

            return move;
        }

        // Search bằng MCTS thông thường
        public static ulong RunSearch(State state, int timeout = 1000)
        {
            // save root node ref, for find best-move later
            var root = new Node(state, null, 0);

            // https://stackoverflow.com/q/4075525/11898496
            var doneTick = Environment.TickCount + timeout;
            while (Environment.TickCount <= doneTick)
            {
                // declare inner scope: https://stackoverflow.com/a/13922788/11898496
                var node = root;

                // Phase 1: Selection
                while (node.IsFullyExpanded() && node.HasChildNode())
                    node = node.SelectChild(root.State.Player);

                // Phase 2: Expansion
                if (!node.IsFullyExpanded())
                    node = node.ExpandChild();

                // Phase 3: Simulation
                var reward = node.Simulate(root.State.Player);

                // Phase 4: BackPropagation
                node.BackPropagate(reward);
            }

            // save statistic
            LastWinPercentage = (int) (root.Wins * 100f / root.Visits);
            LastPlayout = root.Visits;
            LastRunTime = timeout;

            // calculate best move
            return BestMove(root);
        }

        // Search bằng MCTS kêt hợp BTMM vào giai đoạn SIMULATION
        public static ulong RunSearch1(State state, int timeout = 1000)
        {
            var root = new Node(state, null, 0);
            var doneTick = Environment.TickCount + timeout;
            while (Environment.TickCount <= doneTick)
            {
                var node = root;

                while (node.IsFullyExpanded() && node.HasChildNode())
                    node = node.SelectChild(root.State.Player);

                if (!node.IsFullyExpanded())
                    node = node.ExpandChild();

                var reward = node.SimulateBTMM(root.State.Player); // SIMULATE BTMM
                node.BackPropagate(reward);
            }

            LastWinPercentage = (int) (root.Wins * 100f / root.Visits);
            LastPlayout = root.Visits;
            LastRunTime = timeout;

            return BestMove(root);
        }

        // Searh bằng MCTS kết hợp BTMM vào 2 giai đoạn SIMULATION + SELECTION
        // ...

        private static ulong BestMove(Node node, byte policy = Constant.RobustChild)
        {
            // If not all children are expanded, not enough information
            // if (node.IsFullyExpanded() == false)
            //     throw new Exception("Not enough information!");

            var bestMove = 0UL;

            if (policy == Constant.RobustChild)
            {
                var max = double.MinValue;
                foreach (var childNode in node.ChildNodes)
                {
                    if (childNode.Visits > max)
                    {
                        bestMove = childNode.ParentMove;
                        max = childNode.Visits;
                    }
                }
            }
            else if (policy == Constant.MaxChild)
            {
                var max = double.MinValue;
                foreach (var childNode in node.ChildNodes)
                {
                    double ratio = childNode.Wins / childNode.Visits;
                    if (ratio > max)
                    {
                        bestMove = childNode.ParentMove;
                        max = childNode.Visits;
                    }
                }
            }

            return bestMove;
        }
    }
}