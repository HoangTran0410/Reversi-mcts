using System;

namespace Reversi_mcts.Utils
{
    public static class MathUtils
    {
        // tính nhanh 3^exp
        public static int Power3(int exp)
        {
            return exp switch
            {
                0 => 1,
                1 => 3,
                2 => 9,
                3 => 27,
                4 => 81,
                5 => 243,
                6 => 729,
                7 => 2187,
                8 => 6561,
                9 => 19683,
                10 => 59049,
                11 => 177147,
                12 => 531441,
                13 => 1594323,
                14 => 4782969,
                15 => 14348907,
                16 => 43046721,
                _ => (int) Math.Pow(3, exp)
            };
        }

        public static float LimitToRange(this float value, float lowerLimit, float upperLimit)
        {
            if (value < lowerLimit) return lowerLimit;
            if (value > upperLimit) return upperLimit;
            return value;
        }
    }
}