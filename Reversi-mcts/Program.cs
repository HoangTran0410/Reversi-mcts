using System;
using System.Collections.Generic;

namespace Reversi_mcts
{
    class Program
    {
        static void Main(string[] args)
        {
            var board = new ReversiBitboard();
            //Console.WriteLine(board.ToDisplayString());
            board.Draw();

            board.MakeMove(0, 2, 3);
            board.Draw();

            board.MakeMove(1, 4, 2);
            board.Draw();

            ulong l = BitboardExtensions.CoordinateToULong(5, 3);
            l.Draw();
            Console.WriteLine(l.ToCoordinate());

            Console.WriteLine(board.GetScore(0));
        }
    }
}
