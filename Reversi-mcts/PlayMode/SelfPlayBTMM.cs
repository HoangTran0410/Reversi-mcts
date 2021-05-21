using System;
using Reversi_mcts.Board;
using Reversi_mcts.MonteCarlo;

namespace Reversi_mcts.PlayMode
{
    public class SelfPlayBTMM
    {
        // Black sẽ dùng BTMM trong phase SIMULATION, White thì dùng MCTS bình thường
        public static byte OneRound(int blackTimeout = 500, int whiteTimeout = 500, bool showLog = true)
        {
            var state = new State();
            var winner = Constant.Draw;

            while (!state.IsTerminal())
            {
                var isBlack = state.Player == Constant.Black;
                var timeout = isBlack ? blackTimeout : whiteTimeout;
                var move = isBlack ? Mcts.RunSearch1(state, timeout) : Mcts.RunSearch(state, timeout);

                state.NextState(move);
                winner = state.Winner();

                if (showLog) SelfPlay.ShowLog(move, state);
            }

            return winner;
        }

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
    }
}