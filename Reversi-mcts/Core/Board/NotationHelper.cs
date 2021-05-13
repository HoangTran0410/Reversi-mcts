namespace Reversi_mcts.Core.Board
{
    public static class NotationHelper
    {
        private const string ColumnName = "abcdefgh";
        private const string RowName = "12345678";

        // ------------------------------------ BitMove ------------------------------------
        public static (int row, int col) ToCoordinate(this ulong bitMove)
        {
            if (bitMove == 0) return (-1, -1);
            var index = bitMove.BitScanReverse();
            return (index / 8, index % 8);
        }

        public static string ToNotation(this ulong bitMove)
        {
            return bitMove.ToCoordinate().ToNotation();
        }

        // ------------------------------------ Coordinate ------------------------------------
        public static ulong ToBitMove(this (int row, int col) coordinate)
        {
            return 0UL.SetBitAtCoordinate(coordinate.row, coordinate.col);
        }

        public static string ToNotation(this (int row, int col) coordinate)
        {
            if (coordinate.row == -1 || coordinate.col == -1) return "PASSED";
            return "" + ColumnName[coordinate.col] + RowName[coordinate.row];
        }

        // ------------------------------------ Notation ------------------------------------
        public static ulong ToBitMove(this string notation)
        {
            var (row, col) = notation.ToCoordinate();
            if (row == -1 || col == -1) return 0UL;
            return 0UL.SetBitAtCoordinate(row, col);
        }

        public static (int row, int col) ToCoordinate(this string notation)
        {
            var notationChar = notation.ToLower().ToCharArray();
            var col = ColumnName.IndexOf(notationChar[0]);
            var row = RowName.IndexOf(notationChar[1]);

            return (row, col);
        }
    }
}