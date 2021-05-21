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
            BTMMAlgorithm.Run();

            // SelfPlay.OneRound(1000, 1000);
            // SelfPlay.MultiRounds(200, 50, 50);
            // HumanVsAi.NewGame(Constant.White);
            // ContinueFromRecord.NewGame("f5f4c3f6f3d6e6c5c4d3e3d2c7c6c2c8d1b1e2f2b3a3e1f1b4c1b5a4g5h5g2h1");

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