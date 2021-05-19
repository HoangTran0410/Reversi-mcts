using System;
using System.Collections.Generic;
using Reversi_mcts.Core.Board;
using Reversi_mcts.Core.MonteCarlo;
using Reversi_mcts.Utils;

namespace Reversi_mcts.GamePattern
{
    public class PatternShape
    {
        public ulong[] ElementPositions; // những ô đen + đỏ (trong paper của nqhuy)
        public ulong MustEmptyPosition; // ô đỏ (target cell)

        public PatternShape(ulong[] elementPositions, ulong mustEmptyPosition)
        {
            ElementPositions = elementPositions;
            MustEmptyPosition = mustEmptyPosition;
        }

        // trả về unique code (id) của pattern này khi áp vào gameBoard (?)
        public int CalculatePatternCode(BitBoard bitBoard)
        {
            var len = ElementPositions.Length;
            var pattern = new int[len];
            for (var i = 0; i < len; i++)
                pattern[i] = bitBoard.GetPieceAt(ElementPositions[i]);
            return CalculatePatternCodeFromPatternElements(pattern);
        }

        private int CalculatePatternCodeFromPatternElements(int[] patternElements)
        {
            var result = 0;
            var len = ElementPositions.Length;
            for (var i = 0; i < len; i++)
                result += (patternElements[len - i - 1] * MathUtils.Power3(i));
            return result;
        }

        public int ElementIndexFromElementPosition(ulong elementPosition)
        {
            return Array.IndexOf(ElementPositions, elementPosition);
        }

        public static PatternShape Parse(string strElementPositions, string strMustEmptyPosition)
        {
            // parse must empty position
            var mustEmptyPosition = strMustEmptyPosition.ToBitMove();

            // parse element postions
            var elementPositions = new List<ulong>();
            foreach (var notation in strElementPositions.Split(','))
                elementPositions.Add(notation.ToBitMove());
            
            return new PatternShape(elementPositions.ToArray(), mustEmptyPosition);
        }

        // Kiem tra xem pattern hien tai co phai la cha (chứa) pattern other hay khong.
        public bool Contains(PatternShape other)
        {
            if (MustEmptyPosition != other.MustEmptyPosition)
                return false;
            
            foreach (var pos in other.ElementPositions)
                if (Array.IndexOf(ElementPositions, pos) < 0)
                    return false;
            
            return true;
        }
        
    }
}