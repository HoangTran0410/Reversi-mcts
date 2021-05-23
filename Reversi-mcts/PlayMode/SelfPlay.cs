using System;
using Reversi_mcts.Board;
using Reversi_mcts.MonteCarlo;
using Reversi_mcts.Utils;

namespace Reversi_mcts.PlayMode
{
    public static class SelfPlay
    {
        public static void MultiRounds(
            int totalRounds = 10,
            int blackTimeout = 500,
            Algorithm blackAlgorithm = Algorithm.Mcts,
            int whiteTimeout = 500,
            Algorithm whiteAlgorithm = Algorithm.Mcts)
        {
            var blackWin = 0;
            var whiteWin = 0;

            var round = 0;
            while (round++ < totalRounds)
            {
                Console.Write($"Round {round}: Playing...");

                var winner = OneRound(blackTimeout, blackAlgorithm, whiteTimeout, whiteAlgorithm, false);
                if (winner == Constant.Black) blackWin++;
                if (winner == Constant.White) whiteWin++;

                ConsoleUtil.ClearCurrentConsoleLine();
                Console.WriteLine($"Round {round}: {blackWin}/{whiteWin}");
            }

            Console.WriteLine("END. Black win: {0}, White win {1}", blackWin, whiteWin);
        }

        public static byte OneRound(
            int blackTimeout = 500,
            Algorithm blackAlgorithm = Algorithm.Mcts,
            int whiteTimeout = 500,
            Algorithm whiteAlgorithm = Algorithm.Mcts,
            bool showLog = true)
        {
            var state = new State();
            var winner = Constant.Draw;

            while (!state.IsTerminal())
            {
                var isBlack = state.Player == Constant.Black;
                var timeout = isBlack ? blackTimeout : whiteTimeout;
                var algoName = isBlack ? blackAlgorithm : whiteAlgorithm;

                var move = Mcts.RunSearch(algoName, state, timeout);

                state.NextState(move);
                winner = state.Winner();

                if (showLog) ShowLog(move, state);
            }

            return winner;
        }

        public static void ShowLog(ulong move, State state)
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

            state.Board.DisplayWithLastMoveAndLegalMoves(move, playerMakeMove);
            var blackScore = state.Board.CountPieces(Constant.Black);
            var whiteScore = state.Board.CountPieces(Constant.White);
            Console.WriteLine("- Score(b/w): {0}/{1}\n", blackScore, whiteScore);
        }
    }
}