using Reversi_mcts.MCTS_medium;
using System;

namespace Reversi_mcts
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new Game();
            var mcts = new MCTS_medium.MCTS(game);

            var state = game.Start();
            var winner = game.Winner(state);

            // From initial state, take turns to play game until someone wins
            while (winner == -2)
            {
                mcts.RunSearch(state, 1);
                Play play = mcts.BestPlay(state);
                state = game.NextState(state, play);
                winner = game.Winner(state);
            }

            Console.WriteLine(winner);
        }
    }
}
