using System;
using Reversi_mcts.Core.Board;
using Reversi_mcts.Core.MonteCarlo;
using Reversi_mcts.GamePattern;
using Reversi_mcts.PlayMode;
using Reversi_mcts.PlayMode.SocketIo;
using Reversi_mcts.Utils;

namespace Reversi_mcts
{
    internal static class Program
    {
        private static void Main()
        {
            BTMMAlgorithm.Run();

            // SelfPlay.OneRound(1000, 1000);
            // SelfPlay.MultiRounds(200, 50, 50);
            // HumanVsAi.NewGame(Constant.White);
            // ContinueFromRecord.NewGame("d3c3b3c5f6f5g6f3b6b5b4a5f4f7g4e3c4a3f8b7c6d6a8g7d2f2f1h4c2c1a6g3d7e1d1g1h3h2e6e7h8d8g8e2c8h5b2a4e8b8a2g5g2b1a1h1c7a7h6");

            // var socketClient = new SocketClient(4000);

            // 0x22120A0E1222221E
            // var x = new BitBoard();
            // Console.WriteLine("Normal");
            // x.Display();
            //
            // Console.WriteLine("FlipVertical");
            // x.FlipVertical().Display();
            //
            // Console.WriteLine("MirrorHorizontal {0}", x);
            // x.MirrorHorizontal().Display();
            //
            // Console.WriteLine("FlipDiagA1H8 {0}", x);
            // x.FlipDiagA1H8().Display();
            //
            // Console.WriteLine("FlipDiagA8H1 {0}", x);
            // x.FlipDiagA8H1().Display();
            //
            // Console.WriteLine("Rotate180 {0}", x);
            // x.Rotate180().Display();
            //
            // Console.WriteLine("Rotate90Clockwise {0}", x);
            // x.Rotate90Clockwise().Display();
            //
            // Console.WriteLine("Rotate90AntiClockwise {0}", x);
            // x.Rotate90AntiClockwise().Display();

            // ConsoleUtils.RunTest();
            // ProgressBar.RunTest();
        }
    }
}