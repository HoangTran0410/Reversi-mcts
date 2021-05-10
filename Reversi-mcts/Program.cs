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
            var totalRound = 10;
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

            //TestPerformance();
        }

        private static byte SelfPlay(int blackTimeout = 500, int whiteTimeout = 500)
        {
            var state = new State(new BitBoard(), Constant.Black);
            var winner = Constant.GameNotCompleted;

            while (winner == Constant.GameNotCompleted)
            {
                var timeout = state.Player == Constant.Black ? blackTimeout : whiteTimeout;
                var move = MCTS.RunSearch(state, timeout);

                // if (move == 0)
                // {
                //     Console.WriteLine("- Player " + state.Player + " Pass move.");
                // }
                // else
                // {
                //     var (row, col) = move.ToCoordinate();
                //     Console.WriteLine("- Player " + state.Player + " move at " + row + ", " + col);
                // }

                state = state.NextState(move);
                winner = state.Winner();

                // state.Board.DrawWithLastMoveAndLegalMoves(move, state.Player);
                // int blackScore = state.Board.CountPieces(Constant.Black);
                // int whiteScore = state.Board.CountPieces(Constant.White);
                // Console.WriteLine("- Score(b/w): {0}/{1}\n", blackScore, whiteScore);
            }

            //Console.WriteLine("End game. Winner is player {0}", winner);
            return winner;
        }

        private static void TestPerformance()
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            stopwatch.Stop();
            Console.WriteLine("Took " + stopwatch.Elapsed);
        }
    }
}