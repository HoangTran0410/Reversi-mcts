using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    class Constant
    {
        // Game status
        public static byte Black = 0;
        public static byte White = 1;
        public static byte Draw = 2;
        public static byte GameNotCompleted = 3;

        // Node Score
        public static byte WinScore = 2;
        public static byte DrawScore = 1;
        public static byte LoseScore = 0;
    }
}
