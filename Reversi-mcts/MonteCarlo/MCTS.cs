using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    public class MCTS
    {
        public static ulong RunSearch(State state, int timeout = 1000)
        {
            int winCount = 0;
            int loseCount = 0;
            int playout = 0;

            TimeSpan timeLimit = TimeSpan.FromMilliseconds(timeout);
            DateTime start = DateTime.Now;
            
            // save root node ref, for find best-move later
            Node root = new Node(state, null, 0);

            while (DateTime.Now - start < timeLimit)
            {
                // delare inner scope: https://stackoverflow.com/a/13922788/11898496
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
                if (score == 1f) winCount++;
                if (score == 0) loseCount++; 
            }

            Console.WriteLine("- Runtime: {0}ms, Playout: {1}, wins/loses: {2}/{3}" , timeout, playout, winCount, loseCount);
            return BestMove(root);
        }

        public static ulong BestMove(Node node, string policy = "robust")
        {
            // If not all children are expanded, not enough information
            if (node.IsFullyExpanded() == false)
                throw new Exception("Not enough information!");
            
            var bestMove = 0UL;
        
            // Most visits (robust child)
            if (policy.Equals("robust"))
            {
                var max = Double.MinValue;
                foreach (Node childNode in node.ChildNodes)
                {
                    if (childNode.Visits > max)
                    {
                        bestMove = childNode.Move;
                        max = childNode.Visits;
                    }
                }
            }
        
            // Highest winrate (max child)
            else if (policy.Equals("max"))
            {
                var max = Double.MinValue;
                foreach (Node childNode in node.ChildNodes)
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
