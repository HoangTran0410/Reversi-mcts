using System;
using System.Linq;

namespace Reversi_mcts.MCTS
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
            int parentVisit = node.state.visitCount;

            Node selectedNode = node.childArray[0];
            double maxUctValue = -1;
            foreach (Node n in node.childArray)
            {
                double uctValue = UCT.UctValue(parentVisit, n.state.winScore, n.state.visitCount);
                if(uctValue > maxUctValue)
                {
                    maxUctValue = uctValue;
                    selectedNode = n;
                }
            }
            return selectedNode;
        }
    }
}
