using System;
using Reversi_mcts.Board;
using Reversi_mcts.MonteCarlo;
using System.Diagnostics;

namespace Reversi_mcts
{
    internal static class Program
    {
        private static void Main()
        {
            SelfPlay();
            //SelfPlayRounds(50);
            //CheckGetOpponentPerformance();
        }

        private static void SelfPlayRounds(int totalRound = 10)
        {
            var blackWin = 0;
            var whiteWin = 0;

            var round = 0;
            while (round++ < totalRound)
            {
                Console.Write("Round {0}: ", round);

                var winner = SelfPlay();
                if (winner == Constant.Black) blackWin++;
                if (winner == Constant.White) whiteWin++;

                Console.WriteLine("{0}/{1}", blackWin, whiteWin);
            }

            Console.WriteLine("END. Black win: {0}, White win {1}", blackWin, whiteWin);
        }

        private static byte SelfPlay(int blackTimeout = 500, int whiteTimeout = 500)
        {
            var state = new State(new BitBoard(), Constant.Black);
            var winner = Constant.GameNotCompleted;

            while (winner == Constant.GameNotCompleted)
            {
                var timeout = state.Player == Constant.Black ? blackTimeout : whiteTimeout;
                var move = Mcts.RunSearch(state, timeout);

                if (move == 0)
                {
                    Console.WriteLine("- Player " + state.Player + " Pass move.");
                }
                else
                {
                    var (row, col) = move.ToCoordinate();
                    Console.WriteLine("- Player " + state.Player + " move at " + row + ", " + col);
                }

                state = state.NextState(move);
                winner = state.Winner();

                state.Board.DrawWithLastMoveAndLegalMoves(move, state.Player);
                int blackScore = state.Board.CountPieces(Constant.Black);
                int whiteScore = state.Board.CountPieces(Constant.White);
                Console.WriteLine("- Score(b/w): {0}/{1}\n", blackScore, whiteScore);
            }

            Console.WriteLine("End game. Winner is player {0}", winner);
            return winner;
        }

        private static void CheckGetOpponentPerformance()
        {
            const double loop = 1E9;
            var stopwatch = new Stopwatch();

            // ---------------- using bit operator ----------------
            stopwatch.Start();
            byte player = 0;
            for (var i = 0; i < loop; i++)
            {
                player = (byte) (1 ^ player);
            }
            stopwatch.Stop();
            Console.WriteLine("bit took " + stopwatch.Elapsed);

            // ---------------- using Conditional (ternary) operator ----------------
            stopwatch.Restart();
            byte player2 = 0;
            for (var i = 0; i < loop; i++)
            {
                player2 = player2 == Constant.Black ? Constant.White : Constant.Black;
            }

            stopwatch.Stop();
            Console.WriteLine("conditional took " + stopwatch.Elapsed);

            // ---------------- using sub operator ----------------
            stopwatch.Restart();
            byte player3 = 0;
            for (var i = 0; i < loop; i++)
            {
                player3 = (byte) (Constant.Black + Constant.White - player3);
            }

            stopwatch.Stop();
            Console.WriteLine("sub took " + stopwatch.Elapsed);
        }
    }
}