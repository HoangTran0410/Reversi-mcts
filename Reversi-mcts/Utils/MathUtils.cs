namespace Reversi_mcts.Utils
{
    public static class MathUtils
    {
        public static int Power3(int exp)
        {
            return exp switch
            {
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
                _ => 1
            };
        }
    }
}