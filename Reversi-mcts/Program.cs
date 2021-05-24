using Reversi_mcts.MachineLearning;

namespace Reversi_mcts
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // ---------------------------------
            // ----------- Menu GUI ------------
            // ---------------------------------
            Menu.MainMenu();
            
            // ---------------------------------
            // --------- All functions ---------
            // ---------------------------------
            // BTMMAlgorithm.TrainGameRecord(@"E:\z-Reversi\game-records\ALL.txt", @"E:\z-Reversi\trained-all");
            // BTMMAlgorithm.LoadTrainedData(@"E:\z-Reversi\trained71k.bin");
            // SelfPlay.OneRound(blackTimeout, blackAlgorithm, whiteTimeout, whiteAlgorithm);
            // SelfPlay.MultiRounds(totalRounds, blackTimeout, blackAlgorithm, whiteTimeout, whiteAlgorithm);
            // HumanVsAi.NewGame(Constant.White, aiTimeout, aiAlgorithm);
            // new SocketClient(serverIp, clientName, algorithm, timeout).Connect();
            
            // ContinueFromRecord.NewGame("f5f4c3f6f3d6e6c5c4d3e3d2c7c6c2c8d1b1e2f2b3a3e1f1b4c1b5a4g5h5g2h1");

            // ---------------------------------
            // -------- Convert ggf file -------
            // ---------------------------------
            // var parser = new GameRecordParser();
            // parser.Parse(@"E:\z-Reversi\Dataset\Othello.01e4.ggf\Othello.01e4.ggf", true);
            // parser.SaveParsedGame(@"E:\z-Reversi\game-records\ggf10k.txt");
        }
    }
}