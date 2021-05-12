using System;
using Reversi_mcts.Core.Board;
using Reversi_mcts.Core.MonteCarlo;

namespace Reversi_mcts.PlayMode
{
    public static class SelfPlay
    {
        public static void MultiRounds(int totalRound = 10, int blackTimeout = 500, int whiteTimeout = 500)
        {
            var blackWin = 0;
            var whiteWin = 0;

            var round = 0;
            while (round++ < totalRound)
            {
                Console.Write("Round {0}: ", round);

                var winner = OneRound(blackTimeout, whiteTimeout, false);
                if (winner == Constant.Black) blackWin++;
                if (winner == Constant.White) whiteWin++;

                Console.WriteLine("{0}/{1}", blackWin, whiteWin);
            }

            Console.WriteLine("END. Black win: {0}, White win {1}", blackWin, whiteWin);
        }

        public static byte OneRound(int blackTimeout = 500, int whiteTimeout = 500, bool showLog = true)
        {
            var state = new State();
            var winner = Constant.GameNotCompleted;

            while (winner == Constant.GameNotCompleted)
            {
                var timeout = state.Player == Constant.Black ? blackTimeout : whiteTimeout;
                var move = Mcts.RunSearch(state, timeout);

                state = state.NextState(move);
                winner = state.Winner();

                if (showLog) ShowLog(move, state);
            }
            
            return winner;
        }

        private static void ShowLog(ulong move, State state)
        {
            Console.WriteLine("- Runtime: {0}ms, Playout: {1}, wins: {2}%",
                Mcts.LastRunTime,
                Mcts.LastPlayout,
                Mcts.LastWinPercentage);

            // call log after NextState => player-make-move is previous player = opponent of current player
            var playerMakeMove = Constant.Opponent(state.Player);

            if (move == 0)
            {
                Console.WriteLine("- Player " + playerMakeMove + " Pass move.");
            }
            else
            {
                var (row, col) = move.ToCoordinate();
                Console.WriteLine("- Player " + playerMakeMove + " move at " + (row, col).ToNotation());
            }
            
            state.Board.DrawWithLastMoveAndLegalMoves(move, playerMakeMove);
            var blackScore = state.Board.CountPieces(Constant.Black);
            var whiteScore = state.Board.CountPieces(Constant.White);
            Console.WriteLine("- Score(b/w): {0}/{1}\n", blackScore, whiteScore);
        }
    }
}