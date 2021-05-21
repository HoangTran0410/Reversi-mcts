using System;
using Reversi_mcts.MachineLearning;
using Reversi_mcts.PlayMode;
using Reversi_mcts.PlayMode.SocketIo;
using Reversi_mcts.Utils;

namespace Reversi_mcts
{
    internal static class Program
    {
        private static void Main()
        {
            BTMMAlgorithm.TrainGameRecord(@"E:\game-record.txt", @"E:\trained.txt");
            BTMMAlgorithm.LoadTrainedData(@"E:\trained.txt");
            // SelfPlayBTMM.OneRound(50, 50);
            // SelfPlayBTMM.MultiRounds(20, 200, 200);
            
            // SelfPlay.OneRound(1000, 1000);
            // SelfPlay.MultiRounds(200, 500, 500);
            // HumanVsAi.NewGame(Constant.White);
            // ContinueFromRecord.NewGame("f5f4c3f6f3d6e6c5c4d3e3d2c7c6c2c8d1b1e2f2b3a3e1f1b4c1b5a4g5h5g2h1");

            // var socketClient = new SocketClient(4000);
        }
    }
}