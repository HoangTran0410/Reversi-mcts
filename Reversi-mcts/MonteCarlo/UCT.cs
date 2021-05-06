using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    class UCT
    {
        public static double UctValue(int totalVisit, double nodeWinScore, int nodeVisit)
        {
            if (nodeVisit == 0)
            {
                return Int32.MaxValue;
            }
            return (nodeWinScore / (double)nodeVisit) + 1.41 * Math.Sqrt(Math.Log(totalVisit) / (double)nodeVisit);
        }

        public static Node FindBestNodeWithUCT(Node node)
        {
            int parentVisit = node.Visits;

            Node selectedNode = node.Children[0];
            double maxUctValue = -1;
            foreach (Node n in node.Children)
            {
                double uctValue = UctValue(parentVisit, n.Wins, n.Visits);
                if (uctValue > maxUctValue)
                {
                    maxUctValue = uctValue;
                    selectedNode = n;
                }
            }
            return selectedNode;
        }
    }
}
