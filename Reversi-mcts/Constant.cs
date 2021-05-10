using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    class Constant
    {
        public static Random Random = new Random();

        // Game status
        public const byte Black = 0;
        public const byte White = 1;
        public const byte Draw = 2;
        public const byte GameNotCompleted = 3;

        // Node Score
        public const byte WinScore = 2;
        public const byte DrawScore = 1;
        public const byte LoseScore = 0;
    }
}