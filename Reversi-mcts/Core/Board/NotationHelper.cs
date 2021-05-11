using System;
using System.Collections.Generic;

namespace Reversi_mcts.Core.Board
{
    public static class NotationHelper
    {
        public static readonly List<char> ColumnName = new List<char>()
            {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'};

        public static readonly List<char> RowName = new List<char>()
            {'1', '2', '3', '4', '5', '6', '7', '8'};

        // ------------------------------------ BitMove ------------------------------------
        public static (byte row, byte col) ToCoordinate(this ulong bitMove)
        {
            var index = bitMove.BitScanReverse();
            return (
                (byte) (index / 8),
                (byte) (index % 8)
            );
        }

        public static string ToNotation(this ulong bitMove)
        {
            return bitMove.ToCoordinate().ToNotation();
        }

        // ------------------------------------ Coordinate ------------------------------------
        public static ulong ToBitMove(this (byte row, byte col) coordinate)
        {
            return 0UL.SetBitAtCoordinate(coordinate.row, coordinate.col);
        }

        public static string ToNotation(this (byte row, byte col) coordinate)
        {
            return "" + ColumnName[coordinate.col] + RowName[coordinate.row];
        }
        
        // ------------------------------------ Notation ------------------------------------
        public static ulong ToBitMove(this string notation)
        {
            var (row, col) = notation.ToCoordinate();
            if (row == -1 || col == -1) return 0UL;
            return 0UL.SetBitAtCoordinate((byte)row, (byte)col);
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