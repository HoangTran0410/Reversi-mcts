using System;
using System.Collections.Generic;

namespace Reversi_mcts
{
    class Program
    {
        static void Main(string[] args)
        {
            var state = new State(new BitBoard(), Constant.Black);
            var winner = Constant.GameNotCompleted;

            while (winner == Constant.GameNotCompleted)
            {
                var move = MCTS.RunSearch(state, 2000);

                if (move == 0)
                {
                    Console.WriteLine("- Player " + state.Player + " Pass move.");
                }
                else
                {
                    var coor = move.ToCoordinate();
                    Console.WriteLine("- Player " + state.Player + " move at " + coor.row + ", " + coor.col);
                }

                state = state.NextState(move);
                winner = state.Winner();

                state.Board.DrawWithLastMoveAndLegalMoves(move, state.Player);
                int blackScore = state.Board.CountPieces(Constant.Black);
                int whiteScore = state.Board.CountPieces(Constant.White);
                Console.WriteLine("- Score(b/w): {0}/{1}\n", blackScore, whiteScore);
            }

            Console.WriteLine("End game. Winner is player {0}", winner);
        }
    }
}