using System;

namespace Reversi_mcts
{
    public enum Algorithm
    {
        Mcts, // monte carlo tree search + bitboard
        Mcts1, // Mcts + btmm in simulation phase
        Mcts2 // Mcts1 + btmm in selection phase
    }

    // why use internal static class? => Rider recommend, idk :))
    public static class Constant
    {
        public static readonly Random Random = new Random(); // https://stackoverflow.com/a/768001/118984969

        public static byte Opponent(byte player)
        {
            // The 3 ways below have the same performance

            return player == Black ? White : Black;
            // return (byte) (Black + White - player);
            // return (byte)(1 ^ player);
        }
        
        // BTMM
        public const double Cbt = 0.5; // hệ số điều chỉnh ảnh hưởng độ lệch
        public const double K = 500.0; // tham số điều chỉnh tỷ lệ khi nước đi có xu hướng giảm số lần viếng thăm
        
        // MCTS
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