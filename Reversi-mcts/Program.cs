using System;
using Reversi_mcts.Utils;

namespace Reversi_mcts
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            ConsoleUtil.WriteAndWaitKey("This project (Reversi-mcts) doesn't have any GUI. Run Reversi-game instead!");
            // ---------------------------------
            // --------- BTMM functions ---------
            // ---------------------------------
            // BTMMAlgorithm.TrainGameRecord(@"E:\z-Reversi\Dataset\Othello.latest.294420.ggf", @"E:\ggf-lastest");
            // BTMMAlgorithm.LoadTrainedData(@"E:\z-Reversi\trained71k.bin");

            // ---------------------------------
            // ---------- Local Fight ----------
            // ---------------------------------
            // SelfPlay.OneRound(50, Algorithm.Mcts, 50, Algorithm.Mcts);
            // SelfPlay.MultiRounds(totalRounds, blackTimeout, blackAlgorithm, whiteTimeout, whiteAlgorithm);
            // HumanVsAi.NewGame(Constant.White, aiTimeout, aiAlgorithm);
            // ContinueFromRecord.NewGame("f5f4c3f6f3d6e6c5c4d3e3d2c7c6c2c8d1b1e2f2b3a3e1f1b4c1b5a4g5h5g2h1");

            // ---------------------------------
            // -------- Socket io Fight --------
            // ---------------------------------
            // new SocketClient("http://localhost:3000", "mcts", Algorithm.Mcts, 200).Connect();

            // ---------------------------------
            // -------- Convert ggf file -------
            // ---------------------------------
            // var parser = new GameRecordParser();
            // parser.Parse(@"E:\z-Reversi\Dataset\Othello.latest.294420.ggf");
            // parser.SaveParsedGame(@"E:\b.txt");

            // ---------------------------------
            // -------- Visualize Board --------
            // ---------------------------------
            // (new BitBoard(2310351075842195456, 2278360699897856)).Display();
        }
    }
}