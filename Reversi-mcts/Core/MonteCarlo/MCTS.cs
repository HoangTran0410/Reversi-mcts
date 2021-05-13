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
                    node = node.SelectChild(root.State.Player);
                }

                // Phase 2: Expansion
                if (!node.IsFullyExpanded())
                {
                    node = node.ExpandChild();
                }

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