using System;
using System.Collections.Generic;

namespace Reversi_mcts
{
    class Program
    {
        static void Main(string[] args)
        {
            var state = new State(new BitBoard(), Constant.Black);
            var winner = state.Winner();

            while (winner == Constant.GameNotCompleted)
            {
                var move = MCTS.RunSearch(state, 2000);

                var coor = move.ToCoordinate();
                Console.WriteLine("Player " + state.Player + " move at " + coor.row + ", " + coor.col);
                
                state = state.NextState(move);
                winner = state.Winner();
            
                state.Board.Draw();
            }
        }
    }
}
