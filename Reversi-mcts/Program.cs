using System;
using System.Collections.Generic;

namespace Reversi_mcts
{
    class Program
    {
        static void Main(string[] args)
        {
            var board = new BitBoard();
            board.DrawWithLegalMoves(0);

            var nextMove = MonteCarlo.MCTS.FindNextMove(board, Constant.Black);
            nextMove.Draw();

            //ulong legalMoves = board.GetLegalMoves(0);
            //legalMoves.Draw();
            //foreach (ulong m in legalMoves.ToListUlong())
            //{
            //    m.Draw();
            //}

            //ulong l = 1UL;
            //l.Draw();

            //board.MakeMove(0, 2, 3);
            //board.Draw();
            //board.MakeMove(1, 4, 2);
            //board.Draw();
            //Console.WriteLine(board.GetScore(0));

            //ulong l = BitBoardHelper.CoordinateToULong(5, 3);
            //l.Draw();
            //Console.WriteLine(l.ToCoordinate
        }
    }
}
