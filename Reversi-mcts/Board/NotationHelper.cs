namespace Reversi_mcts.Board
{
    public static class NotationHelper
    {
        private const string ColumnName = "abcdefgh";
        private const string RowName = "12345678";

        #region BitMove

        /// <summary>
        /// Check if bitMove is valid (!= 0)
        /// </summary>
        public static bool IsValid(this ulong bitMove)
        {
            return bitMove != 0UL;
        }

        /// <summary>
        /// Convert bitMove to Coordinate
        /// </summary>
        public static (int row, int col) ToCoordinate(this ulong bitMove)
        {
            if (!bitMove.IsValid()) return (-1, -1);
            var index = bitMove.BitScanReverse();
            return (index / 8, index % 8);
        }

        /// <summary>
        /// Convert bitMove to Notation
        /// </summary>
        public static string ToNotation(this ulong bitMove)
        {
            return bitMove.IsValid() ? bitMove.ToCoordinate().ToNotation() : "PASSED";
        }

        #endregion

        #region Coordinate

        /// <summary>
        /// Check if Coordinate is valid (row,col value must be 0 -> 7)
        /// </summary>
        public static bool IsValid(this (int row, int col) coordinate)
        {
            return !(coordinate.row is < 0 or > 7 || coordinate.col is < 0 or > 7);
        }

        /// <summary>
        /// Convert Coordinate to Bitmove
        /// </summary>
        public static ulong ToBitMove(this (int row, int col) coordinate)
        {
            return coordinate.IsValid() ? 0UL.SetBitAtCoordinate(coordinate.row, coordinate.col) : 0UL;
        }

        /// <summary>
        ///  Convert Coordinate to Notation
        /// </summary>
        public static string ToNotation(this (int row, int col) coordinate)
        {
            return coordinate.IsValid() ? "" + ColumnName[coordinate.col] + RowName[coordinate.row] : "PASSED";
        }

        #endregion

        #region Notation

        /// <summary>
        /// Check if Notation is valid
        /// </summary>
        public static bool IsValid(this string notation)
        {
            return !(
                notation.Length != 2 ||
                ColumnName.IndexOf(notation[0]) == -1 ||
                RowName.IndexOf(notation[1]) == -1
            );
        }

        /// <summary>
        /// Convert Notation to Bitmove
        /// </summary>
        public static ulong ToBitMove(this string notation)
        {
            var lower = notation.ToLower();
            return lower.IsValid() ? lower.ToCoordinate().ToBitMove() : 0UL;
        }

        /// <summary>
        /// Convert Notation to Coordinate
        /// </summary>
        public static (int row, int col) ToCoordinate(this string notation)
        {
            var lower = notation.ToLower();
            if (!lower.IsValid()) return (-1, -1);
            var col = ColumnName.IndexOf(lower[0]);
            var row = RowName.IndexOf(lower[1]);
            return (row, col);
        }

        #endregion
    }
}