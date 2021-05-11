using System;

namespace Reversi_mcts
{
    // why use internal static class? => Rider recommend, idk :))
    internal static class Constant
    {
        public static readonly Random Random = new Random();

        public static byte Opponent(byte player)
        {
            // The 3 ways below have the same performance

            return player == Black ? White : Black;
            // return (byte) (Black + White - player);
            // return (byte)(1 ^ player);
        }

        // Game status
        public const byte Black = 0;
        public const byte White = 1;
        public const byte Draw = 2;
        public const byte GameNotCompleted = 3;

        // Node Score
        public const byte WinScore = 2;
        public const byte DrawScore = 1;
        public const byte LoseScore = 0;

        // Policy
        public const byte RobustChild = 0; // Most visits
        public const byte MaxChild = 1; // Highest win-rate

        // Draw
        public const string WrongMove = "X ";
        public const string LastBlackMove = "B ";
        public const string LastWhiteMove = "W ";
        public const string LegalMove = "_ ";
        public const string BlackPiece = "b ";
        public const string WhitePiece = "w ";
        public const string EmptyCell = ". ";
    }
}