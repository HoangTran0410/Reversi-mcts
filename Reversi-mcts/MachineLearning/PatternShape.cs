using System;
using System.Collections.Generic;
using System.Text;
using Reversi_mcts.Board;
using Reversi_mcts.Utils;

namespace Reversi_mcts.MachineLearning
{
    [Serializable]
    public class PatternShape
    {
        public ulong[] ArrayBitCells; // những ô đen + đỏ (trong paper của nqhuy) - dưới dạng bit
        public ulong TargetBitCell; // ô đỏ (target cell) - dưới dạng bit

        public PatternShape(ulong[] arrayBitCells, ulong targetBitCell)
        {
            ArrayBitCells = arrayBitCells;
            TargetBitCell = targetBitCell;
        }

        /// <summary>
        /// Convert ("a1,b1,c1,d1,g1,h1", "a1") to PatternShape
        /// </summary>
        public static PatternShape Parse(string strNotations, string targetCellNotation)
        {
            // parse target cell
            var targetBitCell = targetCellNotation.ToBitMove();

            // parse bit cells array
            var notations = strNotations.Split(',');
            var arrayBitCells = new ulong[notations.Length];
            for (var i = 0; i < notations.Length; i++)
                arrayBitCells[i] = notations[i].ToBitMove();

            return new PatternShape(arrayBitCells, targetBitCell);
        }
    }

    public static class PatternShapeExt
    {
        /// <summary>
        /// Calculate index of pattern (unique id of pattern)
        /// <para>Trả về index (unique id) của 1 pattern cụ thể (pattern = patternShape & gameBoard)</para>
        /// </summary>
        public static int CalculatePatternCode(this PatternShape ps, BitBoard bitBoard)
        {
            // ---------- Version của thầy ---------- 
            // var len = ps.ArrayBitCells.Length;
            // var pattern = new int[len];
            // for (var i = 0; i < len; i++)
            //     pattern[i] = bitBoard.GetPieceAt(ps.ArrayBitCells[i]);
            //
            // var result = 0;
            // for (var i = 0; i < len; i++)
            //     result += pattern[len - i - 1] * MathUtils.Power3(i);

            // ---------- Version của nhóm - ra kết quả giống thầy, nhưng optimized, không cần dùng array ---------- 
            var result = 0;
            var len = ps.ArrayBitCells.Length;
            for (var i = 0; i < len; i++)
            {
                var cellPos = ps.ArrayBitCells[i];
                var cellValue = bitBoard.GetPieceAt(cellPos);
                result += cellValue * MathUtils.Power3(len - i - 1);
            }

            return result;
        }

        /// <summary>
        /// Return index of bitCell in BitCellsArray
        /// </summary>
        public static int IndexOfBitCell(this PatternShape ps, ulong bitCell)
        {
            return Array.IndexOf(ps.ArrayBitCells, bitCell);
        }

        #region Sym8 - Flip,Rotate

        /// <summary>
        /// Create List of pattern flipped/mirrored/rotated
        /// <para>Tạo ra các pattern được flip/mirror/rotate, rồi bỏ tất cả vào 1 List, trả về List</para>
        /// </summary>
        public static List<PatternShape> Sym8(this PatternShape ps)
        {
            var result = new List<PatternShape>();
            var ps2 = ps.CopyByFlipMainDiagonal();
            result.Add(ps);
            result.Add(ps.CopyByFlipVertical());
            result.Add(ps.CopyByFlipHorizontal());
            result.Add(ps.CopyByFlipSubDiagonal());
            result.Add(ps2);
            result.Add(ps2.CopyByFlipVertical());
            result.Add(ps2.CopyByFlipHorizontal());
            result.Add(ps2.CopyByFlipSubDiagonal());
            return result;
        }

        private static PatternShape CopyByFlipHorizontal(this PatternShape ps)
        {
            var targetBitCell = ps.TargetBitCell.MirrorHorizontal();
            var bitCellArray = new ulong[ps.ArrayBitCells.Length];
            for (var i = 0; i < bitCellArray.Length; i++)
                bitCellArray[i] = ps.ArrayBitCells[i].MirrorHorizontal();
            return new PatternShape(bitCellArray, targetBitCell);
        }

        private static PatternShape CopyByFlipVertical(this PatternShape ps)
        {
            var targetBitCell = ps.TargetBitCell.FlipVertical();
            var bitCellArray = new ulong[ps.ArrayBitCells.Length];
            for (var i = 0; i < bitCellArray.Length; i++)
                bitCellArray[i] = ps.ArrayBitCells[i].FlipVertical();
            return new PatternShape(bitCellArray, targetBitCell);
        }

        private static PatternShape CopyByFlipMainDiagonal(this PatternShape ps)
        {
            var targetBitCell = ps.TargetBitCell.FlipDiagA8H1();
            var bitCellArray = new ulong[ps.ArrayBitCells.Length];
            for (var i = 0; i < bitCellArray.Length; i++)
                bitCellArray[i] = ps.ArrayBitCells[i].FlipDiagA8H1();
            return new PatternShape(bitCellArray, targetBitCell);
        }

        private static PatternShape CopyByFlipSubDiagonal(this PatternShape ps)
        {
            var targetBitCell = ps.TargetBitCell.FlipDiagA1H8();
            var bitCellArray = new ulong[ps.ArrayBitCells.Length];
            for (var i = 0; i < bitCellArray.Length; i++)
                bitCellArray[i] = ps.ArrayBitCells[i].FlipDiagA1H8();
            return new PatternShape(bitCellArray, targetBitCell);
        }

        #endregion

        #region Utils

        // Kiem tra xem pattern hien tai co phai la cha (chứa) pattern other hay khong.
        public static bool Contains(this PatternShape ps, PatternShape other)
        {
            if (ps.TargetBitCell != other.TargetBitCell)
                return false;

            foreach (var pos in other.ArrayBitCells)
                if (Array.IndexOf(ps.ArrayBitCells, pos) < 0)
                    return false;

            return true;
        }

        public static string HumanReadablePatternCode(this PatternShape ps, int patternCode)
        {
            var str = new StringBuilder(ps.ArrayBitCells.Length);
            for (var i = 0; i < ps.ArrayBitCells.Length; i++)
            {
                str.Insert(0, patternCode % 3);
                patternCode /= 3;
            }

            return str.ToString();
        }

        #endregion
    }
}