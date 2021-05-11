using System;

namespace Reversi_mcts.Core.MonteCarlo
{
    public static class Mcts
    {
        public static int LastPlayout { get; private set; }
        public static float LastWinPercentage { get; private set; }
        public static int LastRunTime { get; private set; }
        
        public static ulong RunSearch(State state, int timeout = 1000)
        {
            var winCount = 0;
            var totalSimulations = 0;

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
                {
                    node = node.SelectChild();
                }

                // Phase 2: Expansion
                if (!node.IsFullyExpanded())
                {
                    node = node.ExpandChild();
                }

                // Phase 3: Simulation
                var score = node.Simulate(state.Player);

                // Phase 4: BackPropagation
                node.BackPropagate(score);

                // Statistic
                totalSimulations++;
                winCount += score == Constant.WinScore ? 1 : 0; // https://stackoverflow.com/a/4192249/11898496
            }

            // save statistic
            LastWinPercentage = winCount * 100f / totalSimulations;
            LastPlayout = totalSimulations;
            LastRunTime = timeout;

            // calculate best move
            return BestMove(root);
        }

        private static ulong BestMove(Node node, byte policy = Constant.RobustChild)
        {
            // If not all children are expanded, not enough information
            if (node.IsFullyExpanded() == false)
                throw new Exception("Not enough information!");

            var bestMove = 0UL;

            if (policy == Constant.RobustChild)
            {
                var max = double.MinValue;
                foreach (var childNode in node.ChildNodes)
                {
                    if (childNode.Visits > max)
                    {
                        bestMove = childNode.Move;
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
                        bestMove = childNode.Move;
                        max = childNode.Visits;
                    }
                }
            }

            return bestMove;
        }
    }
}