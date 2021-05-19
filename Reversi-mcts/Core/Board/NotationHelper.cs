namespace Reversi_mcts.Core.Board
{
    public static class NotationHelper
    {
        private const string ColumnName = "abcdefgh";
        private const string RowName = "12345678";

        // ------------------------------------ BitMove ------------------------------------
        public static bool IsValid(this ulong bitMove)
        {
            return bitMove != 0UL;
        }

        public static (int row, int col) ToCoordinate(this ulong bitMove)
        {
            if (!bitMove.IsValid()) return (-1, -1);
            var index = bitMove.BitScanReverse();
            return (index / 8, index % 8);
        }

        public static string ToNotation(this ulong bitMove)
        {
            return bitMove.IsValid() ? bitMove.ToCoordinate().ToNotation() : "PASSED";
        }

        // ------------------------------------ Coordinate ------------------------------------
        public static bool IsValid(this (int row, int col) coordinate)
        {
            return !(coordinate.row == -1 || coordinate.col == -1);
        }

        public static ulong ToBitMove(this (int row, int col) coordinate)
        {
            return coordinate.IsValid() ? 0UL.SetBitAtCoordinate(coordinate.row, coordinate.col) : 0UL;
        }

        public static string ToNotation(this (int row, int col) coordinate)
        {
            return coordinate.IsValid() ? "" + ColumnName[coordinate.col] + RowName[coordinate.row] : "PASSED";
        }

        // ------------------------------------ Notation ------------------------------------
        public static bool IsValid(this string notation)
        {
            return !(
                notation.Length != 2 ||
                ColumnName.IndexOf(notation[0]) == -1 ||
                RowName.IndexOf(notation[1]) == -1
            );
        }

        public static ulong ToBitMove(this string notation)
        {
            var lower = notation.ToLower();
            return lower.IsValid() ? lower.ToCoordinate().ToBitMove() : 0UL;
        }

        public static (int row, int col) ToCoordinate(this string notation)
        {
            var lower = notation.ToLower();
            if (!lower.IsValid()) return (-1, -1);
            var col = ColumnName.IndexOf(lower[0]);
            var row = RowName.IndexOf(lower[1]);
            return (row, col);
        }
    }
}