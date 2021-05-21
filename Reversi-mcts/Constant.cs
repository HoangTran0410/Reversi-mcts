using System;

namespace Reversi_mcts
{
    // why use internal static class? => Rider recommend, idk :))
    internal static class Constant
    {
        // --------------------------------------------------------------
        // ----------------------- Machine Learning ---------------------
        // --------------------------------------------------------------
        public const string GameRecordFilePath = @"E:\game-record100.txt";
        public const string TrainedDataFilePath = @"E:\trained.txt";

        // --------------------------------------------------------------
        // ------------------- Monte carlo tree search ------------------
        // --------------------------------------------------------------
        // https://stackoverflow.com/a/768001/11898496
        public static readonly Random Random = new Random();

        public static byte Opponent(byte player)
        {
            // The 3 ways below have the same performance

            return player == Black ? White : Black;
            // return (byte) (Black + White - player);
            // return (byte)(1 ^ player);
        }

        public const double C = 0.85; // exploration constant

        // Cell value
        public const byte EmptyCell = 0;
        public const byte BlackCell = 1;
        public const byte WhiteCell = 2;

        // Game status, Color
        public const byte Black = 0;
        public const byte White = 1;
        public const byte Draw = 2;

        // Node Score
        public const float WinScore = 1;
        public const float DrawScore = 0.5f;
        public const float LoseScore = 0;

        // Policy
        public const byte RobustChild = 0; // Most visits
        public const byte MaxChild = 1; // Highest win-rate

        // Display
        public const string WrongCellStr = "X ";
        public const string LastBlackMoveStr = "B ";
        public const string LastWhiteMoveStr = "W ";
        public const string LegalMoveStr = "_ ";
        public const string BlackPieceStr = "b ";
        public const string WhitePieceStr = "w ";
        public const string EmptyCellStr = ". ";
    }
}