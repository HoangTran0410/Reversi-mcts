using System;
using System.Collections.Generic;

namespace Reversi_mcts
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new Game();
            var mcts = new MCTS(game);

            var state = game.Start();
            var winner = game.Winner(state);

            while (winner == Constant.GameNotCompleted)
            {
                mcts.RunSearch(state, 2);

                var move = mcts.BestMove(state, "robust");
                state = game.NextState(state, move);
                winner = game.Winner(state);

                state.Board.Draw();
            }
        }
    }
}
