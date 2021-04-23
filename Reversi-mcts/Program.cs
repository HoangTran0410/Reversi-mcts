using Reversi_mcts.MCTS_medium;
using System;
using System.Collections.Generic;

namespace Reversi_mcts
{
    class Program
    {
        static void Main(string[] args)
        {
            //var game = new ReversiGame();
            //var mcts = new MCTS_medium.MCTS(game);

            //var state = game.Start();
            //var winner = game.Winner(state);

            //// From initial state, take turns to play game until someone wins
            //while (winner == -2)
            //{
            //    mcts.RunSearch(state, 1);
            //    Move move = mcts.BestMove(state);
            //    state = game.NextState(state, move);
            //    winner = game.Winner(state);
            //}

            //Console.WriteLine(winner);

            var board = new ReversiBitboard();
            Console.WriteLine(board.ToDisplayString());

            board.MakeMove(0, 2, 3);
            Console.WriteLine(board.ToDisplayString());

            board.MakeMove(1, 4, 2);
            Console.WriteLine(board.ToDisplayString());

            ulong l = BitboardExtensions.CoordinateToULong(5, 3);
            Console.WriteLine(l.ToPrettyString());
            Console.WriteLine(l.ToCoordinate());

            Console.WriteLine(board.GetScore(0));
        }
    }
}
