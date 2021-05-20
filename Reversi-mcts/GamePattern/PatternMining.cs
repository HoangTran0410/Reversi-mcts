using System;
using Reversi_mcts.Utils;

namespace Reversi_mcts.GamePattern
{
    // Class này dùng để chứa thông tin thống kê của tập các pattern
    [Serializable]
    public class PatternMining
    {
        public int PatternCommonLength; // số lượng cells trong pattern (PatternShape.BitCellsArray.Length)
        public PatternShape PatternShape;

        public float[,,] Gamma; // giá trị gamma

        [NonSerialized] // https://stackoverflow.com/a/1792051/11898496
        public ushort[,,] Win; // tử số: Wi

        [NonSerialized]
        public ushort[,,] Candidate; // số lần xuất hiện của 1 pattern

        [NonSerialized]
        public float[,,] GammaDenominator; // Mẫu số: Tổng(Cij/E)

        /* NOTE:
         * Cả 4 mảng 3 chiều trên có định dạng: [pattern-id-index, bit-cell-index, player]
         * 
         * bit-cell-index:     index của bitCell trong pattern (PatternShape.IndexOfBitCell)
         * pattern-id-index:   index của unique id của 1 pattern (PatternShape.CalculatePatternCode)
         * player:             0 hoặc 1. màu của người chơi
         */

        private PatternMining(int patternCommonLength)
        {
            PatternCommonLength = patternCommonLength;

            var iCard = MathUtils.Power3(patternCommonLength);
            Gamma = new float[iCard, patternCommonLength, 2];
            GammaDenominator = new float[iCard, patternCommonLength, 2];
            Win = new ushort[iCard, patternCommonLength, 2];
            Candidate = new ushort[iCard, patternCommonLength, 2];

            for (var i = 0; i < iCard; i++)
            {
                for (var j = 0; j < patternCommonLength; j++)
                {
                    Gamma[i, j, 0] = 1;
                    GammaDenominator[i, j, 0] = 0;
                    Win[i, j, 0] = 0;
                    Candidate[i, j, 0] = 0;

                    Gamma[i, j, 1] = 1;
                    GammaDenominator[i, j, 1] = 0;
                    Win[i, j, 1] = 0;
                    Candidate[i, j, 1] = 0;
                }
            }
        }

        public static PatternMining CreatePatternMining(PatternShape patternShape)
        {
            var patternMining = new PatternMining(patternShape.BitCellsArray.Length);
            patternMining.PatternShape = patternShape;
            
            return patternMining;
        }
    }
}