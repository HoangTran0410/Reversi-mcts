using System;
using Reversi_mcts.Core.Board;
using Reversi_mcts.Core.MonteCarlo;
using Reversi_mcts.PlayMode;
using Reversi_mcts.PlayMode.SocketIo;

namespace Reversi_mcts
{
    internal static class Program
    {
        private static void Main()
        {
            //SelfPlay.OneRound(1000, 1000);
            //SelfPlay.MultiRounds(200, 50, 50);
            //HumanVsAi.NewGame(Constant.White);

            var socketClient = new SocketClient(200);
        }
    }
}