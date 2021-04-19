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

            var board = new ReversiBitBoard();
            Console.WriteLine(board.ToDisplayString());

            var neibor = board.EmptyNeighbours();
            Console.WriteLine(neibor.ToBinaryString().Pretty());

            var lineCapMovesBlack = board.LineCapMoves(0);
            Console.WriteLine(lineCapMovesBlack.ToBinaryString().Pretty());

            var lineCapMovesWhite = board.LineCapMoves(1);
            Console.WriteLine(lineCapMovesWhite.ToBinaryString().Pretty());
        }
    }
}
