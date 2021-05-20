using System;
using System.Collections.Generic;
using System.Text;
using Reversi_mcts.Core.Board;
using Reversi_mcts.Utils;

namespace Reversi_mcts.GamePattern
{
    [Serializable]
    public class PatternShape
    {
        public ulong[] BitCellsArray; // những ô đen + đỏ (trong paper của nqhuy) - dưới dạng bit
        public ulong TargetBitCell; // ô đỏ (target cell) - dưới dạng bit

        public PatternShape(ulong[] bitCellsArray, ulong targetBitCell)
        {
            BitCellsArray = bitCellsArray;
            TargetBitCell = targetBitCell;
        }

        // chuyển dạng ("a1,b1,c1,d1,g1,h1", "a1") về PatternShape
        public static PatternShape Parse(string strNotations, string targetCellNotation)
        {
            // parse target cell
            var targetBitCell = targetCellNotation.ToBitMove();

            // parse bit cells array
            var split = strNotations.Split(',');
            var bitCellsArray = new ulong[split.Length];
            for (var i = 0; i < split.Length; i++)
            {
                bitCellsArray[i] = split[i].ToBitMove();
            }

            return new PatternShape(bitCellsArray, targetBitCell);
        }
    }

    public static class PatternShapeExt
    {
        // trả về unique id của pattern (pattern = patternShape & gameBoard)
        public static int CalculatePatternId(this PatternShape ps, BitBoard bitBoard)
        {
            // ---------- Version của thầy ---------- 
            // var len = ps.BitCellsArray.Length;
            // var pattern = new int[len];
            // for (var i = 0; i < len; i++)
            //     pattern[i] = bitBoard.GetPieceAt(ps.BitCellsArray[i]);
            //
            // var result = 0;
            // for (var i = 0; i < len; i++)
            //     result += pattern[len - i - 1] * MathUtils.Power3(i);

            // ---------- Version của nhóm - Cho kết quả giống thầy ---------- 
            var result = 0;
            var len = ps.BitCellsArray.Length;
            var idx = len - 1;
            foreach (var bitCell in ps.BitCellsArray)
            {
                var cellValue = bitBoard.GetPieceAt(bitCell);
                result += cellValue * MathUtils.Power3(idx);
                idx--;
            }

            // Console.WriteLine(ps.HumanReadablePatternCode(result));

            return result;
        }

        public static string HumanReadablePatternCode(this PatternShape ps, int patternCode)
        {
            var str = new StringBuilder(ps.BitCellsArray.Length);
            for (var i = 0; i < ps.BitCellsArray.Length; i++)
            {
                str.Insert(0, patternCode % 3);
                patternCode /= 3;
            }

            return str.ToString();
        }

        // Trả về index của bitCell trong BitCellsArray
        public static int IndexOfBitCell(this PatternShape ps, ulong bitCell)
        {
            return Array.IndexOf(ps.BitCellsArray, bitCell);
        }

        // Tạo ra các pattern được flip/mirror/rotate, rồi bỏ tất cả vào 1 List, trả về List
        public static List<PatternShape> Sym8(this PatternShape ps)
        {
            var result = new List<PatternShape>(8);

            // gộp mảng BitCellsArray vào 1 ulong, để tiện cho việc flip/rotate/mirror
            var bitCellsUlong = 0UL;
            foreach (var position in ps.BitCellsArray)
            {
                bitCellsUlong |= position;
            }

            var tempBitCellsArray = bitCellsUlong.ToArrayBitMove();
            var tempTargetCell = ps.TargetBitCell;

            // 1. Normal
            result.Add(new PatternShape(tempBitCellsArray, tempTargetCell));

            // 2. FlipVertical
            tempBitCellsArray = bitCellsUlong.FlipVertical().ToArrayBitMove();
            tempTargetCell = ps.TargetBitCell.FlipVertical();
            result.Add(new PatternShape(tempBitCellsArray, tempTargetCell));

            // 3. MirrorHorizontal
            tempBitCellsArray = bitCellsUlong.MirrorHorizontal().ToArrayBitMove();
            tempTargetCell = ps.TargetBitCell.MirrorHorizontal();
            result.Add(new PatternShape(tempBitCellsArray, tempTargetCell));

            // 4. FlipDiagA1H8
            tempBitCellsArray = bitCellsUlong.FlipDiagA1H8().ToArrayBitMove();
            tempTargetCell = ps.TargetBitCell.FlipDiagA1H8();
            result.Add(new PatternShape(tempBitCellsArray, tempTargetCell));

            // 5. FlipDiagA8H1
            tempBitCellsArray = bitCellsUlong.FlipDiagA8H1().ToArrayBitMove();
            tempTargetCell = ps.TargetBitCell.FlipDiagA8H1();
            result.Add(new PatternShape(tempBitCellsArray, tempTargetCell));

            // 6. Rotate180
            tempBitCellsArray = bitCellsUlong.Rotate180().ToArrayBitMove();
            tempTargetCell = ps.TargetBitCell.Rotate180();
            result.Add(new PatternShape(tempBitCellsArray, tempTargetCell));

            // 7. Rotate90Clockwise
            tempBitCellsArray = bitCellsUlong.Rotate90Clockwise().ToArrayBitMove();
            tempTargetCell = ps.TargetBitCell.Rotate90Clockwise();
            result.Add(new PatternShape(tempBitCellsArray, tempTargetCell));

            // 8. Rotate90AntiClockwise
            tempBitCellsArray = bitCellsUlong.Rotate90AntiClockwise().ToArrayBitMove();
            tempTargetCell = ps.TargetBitCell.Rotate90AntiClockwise();
            result.Add(new PatternShape(tempBitCellsArray, tempTargetCell));

            return result;
        }

        // Kiem tra xem pattern hien tai co phai la cha (chứa) pattern other hay khong.
        public static bool Contains(this PatternShape ps, PatternShape other)
        {
            if (ps.TargetBitCell != other.TargetBitCell)
                return false;

            foreach (var pos in other.BitCellsArray)
                if (Array.IndexOf(ps.BitCellsArray, pos) < 0)
                    return false;

            return true;
        }
    }
}