using System;

namespace Reversi_mcts.MonteCarlo
{
    public class MCTS
    {
        public static ulong RunSearch(State state, int timeout = 1000)
        {
            var winCount = 0;
            var playout = 0;

            var timeLimit = TimeSpan.FromMilliseconds(timeout);
            var start = DateTime.Now;

            // save root node ref, for find best-move later
            var root = new Node(state, null, 0);

            while (DateTime.Now - start < timeLimit)
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

                // Phase 4: Backpropagation
                node.Backpropagate(score);

                // Statistic
                playout++;
                if (score == Constant.WinScore) winCount++;
            }

            var winPercentage = winCount * 100f / playout;
            Console.WriteLine("- Runtime: {0}ms, Playout: {1}, wins: {2}%", timeout, playout, winPercentage);

            return BestMove(root);
        }

        private static ulong BestMove(Node node, string policy = Constant.RobustChild)
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