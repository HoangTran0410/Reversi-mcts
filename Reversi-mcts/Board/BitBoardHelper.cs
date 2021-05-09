using System;
using System.Collections.Generic;

namespace Reversi_mcts
{
    public enum Direction : byte
    {
        UpLeft = 1,
        Up = 2,
        UpRight = 3,
        Left = 4,
        Right = 6,
        DownLeft = 7,
        Down = 8,
        DownRight = 9
    }

    public static class BitBoardHelper
    {
        // ------------------------------------ Morphological Operations ------------------------------------
        /// <summary>
        /// Expand the on-bits in all square (diagonal) directions.
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="step">Number of steps to expand</param>
        /// <returns></returns>
        public static ulong Delation(this ulong bits, byte step = 1)
        {
            ulong ret = bits;
            while (step-- > 0)
            {
                ret =
                    ret |
                    ret.ShiftUp() |
                    ret.ShiftDown() |
                    ret.ShiftLeft() |
                    ret.ShiftRight() |
                    ret.ShiftUpLeft() |
                    ret.ShiftUpRight() |
                    ret.ShiftDownLeft() |
                    ret.ShiftDownRight();
            }
            return ret;
        }

        /// <summary>
        /// Shrinks the on-bits in all square (diagonal) directions. Reverse operation of dilation.
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="step">Number of steps to shrink</param>
        /// <returns></returns>
        public static ulong Erosion(this ulong bits, byte step = 1)
        {
            ulong ret = bits;
            while (step-- > 0)
            {
                ret =
                    ret &
                    ret.ShiftUp() &
                    ret.ShiftDown() &
                    ret.ShiftLeft() &
                    ret.ShiftRight() &
                    ret.ShiftUpLeft() &
                    ret.ShiftUpRight() &
                    ret.ShiftDownLeft() &
                    ret.ShiftDownRight();
            }
            return ret;
        }

        // ------------------------------------ Shift Stuffs ------------------------------------
        /// <summary>
        /// Shifts the bits in the direction
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="direction">Direction to shift</param>
        /// <returns>Shifted bits</returns>
        public static ulong Shift(this ulong bits, Direction direction)
        {
            return direction switch
            {
                Direction.Up => bits.ShiftUp(),
                Direction.Down => bits.ShiftDown(),
                Direction.Left => bits.ShiftLeft(),
                Direction.Right => bits.ShiftRight(),
                Direction.UpLeft => bits.ShiftUpLeft(),
                Direction.UpRight => bits.ShiftUpRight(),
                Direction.DownLeft => bits.ShiftDownLeft(),
                Direction.DownRight => bits.ShiftDownRight(),
                _ => 0
            };
        }

        // https://github.com/EivindEE/Reversi/blob/f26556919d1c6041c21079e0a54d12fd761a2b39/src/com/eivind/reversi/game/ReversiBitboard.java#L493
        private static ulong LEFT_MASK = 0x7F7F7F7F7F7F7F7F; // 01111111 01111111 01111111 01111111 01111111 01111111 01111111 01111111
        private static ulong RIGHT_MASK = 0xFEFEFEFEFEFEFEFE; // 11111110 11111110 11111110 11111110 11111110 11111110 11111110 11111110
        public static ulong ShiftUp(this ulong bits)
        {
            return bits << 8;
        }
        public static ulong ShiftDown(this ulong bits)
        {
            return bits >> 8;
        }
        public static ulong ShiftLeft(this ulong bits)
        {
            return bits << 1 & RIGHT_MASK;
        }
        public static ulong ShiftRight(this ulong bits)
        {
            return bits >> 1 & LEFT_MASK;
        }
        public static ulong ShiftUpLeft(this ulong bits)
        {
            return bits << 9 & RIGHT_MASK;
        }
        public static ulong ShiftUpRight(this ulong bits)
        {
            return bits << 7 & LEFT_MASK;
        }
        public static ulong ShiftDownLeft(this ulong bits)
        {
            return bits >> 7 & RIGHT_MASK;
        }
        public static ulong ShiftDownRight(this ulong bits)
        {
            return bits >> 9 & LEFT_MASK;
        }

        // ------------------------------------ Bit Stuffs ------------------------------------
        /// <summary>
        /// Convert coordinate (row, col) to binary bits (long)
        /// </summary>
        /// <param name="coordinate">Coordinate (row, col) to convert</param>
        /// <returns>Bits that represent coordinate</returns>
        public static ulong CoordinateToULong(byte row, byte col)
        {
            // TODO faster implementation: use predefine ulong table
            return 0UL.SetBitAtCoordinate(row, col);
        }

        public static List<ulong> ToListUlong(this ulong bits)
        {
            List<ulong> ret = new List<ulong>(bits.PopCount()); // TODO 326MB RAM
            while(bits != 0)
            {
                ulong m = bits.HighestOneBit();
                bits ^= m;
                ret.Add(m); // TODO 1535MB RAM
            }
            return ret;
        }

        public static (byte row, byte col) ToCoordinate(this ulong bits)
        {
            int index = bits.BitScanReverse();
            return (
                (byte)(index / 8),
                (byte)(index % 8)
            );
        }

        // https://www.chessprogramming.org/BitScan#De_Bruijn_Multiplication
        static int[] index64_reverse =
        {
            0, 47,  1, 56, 48, 27,  2, 60,
           57, 49, 41, 37, 28, 16,  3, 61,
           54, 58, 35, 52, 50, 42, 21, 44,
           38, 32, 29, 23, 17, 11,  4, 62,
           46, 55, 26, 59, 40, 36, 15, 53,
           34, 51, 20, 43, 31, 22, 10, 45,
           25, 39, 14, 33, 19, 30,  9, 24,
           13, 18,  8, 12,  7,  6,  5, 63
        };
        public static int BitScanReverse(this ulong bb)
        {
            ulong debruijn64 = 0x03f79d71b4cb0a89;
            // assert(bb != 0);
            bb |= bb >> 1;
            bb |= bb >> 2;
            bb |= bb >> 4;
            bb |= bb >> 8;
            bb |= bb >> 16;
            bb |= bb >> 32;
            return index64_reverse[(bb * debruijn64) >> 58];
        }

        // https://www.chessprogramming.org/BitScan#Matt_Taylor.27s_Folding_trick
        static int[] index64_forward =
        {
            63, 30,  3, 32, 59, 14, 11, 33,
            60, 24, 50,  9, 55, 19, 21, 34,
            61, 29,  2, 53, 51, 23, 41, 18,
            56, 28,  1, 43, 46, 27,  0, 35,
            62, 31, 58,  4,  5, 49, 54,  6,
            15, 52, 12, 40,  7, 42, 45, 16,
            25, 57, 48, 13, 10, 39,  8, 44,
            20, 47, 38, 22, 17, 37, 36, 26
        };
        public static int BitScanForward(this ulong bb)
        {
            uint folded;
            //assert(bb != 0);
            bb ^= bb - 1;
            folded = (uint)bb ^ (uint)(bb >> 32);
            return index64_forward[folded * 0x78291ACF >> 26];
        }

        /// <summary>
        /// Set bit 1 to position (row,col)
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static ulong SetBitAtCoordinate(this ulong bits, byte row, byte col)
        {
            // https://stackoverflow.com/a/24250656/11898496
            return bits | (1UL << Ix(row, col));
        }

        /// <summary>
        /// Set bit 0 to position (row,col)
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static ulong RemoveBitAtCoordinate(this ulong bits, byte row, byte col)
        {
            return bits & ~(1UL << Ix(row, col));
        }

        /// <summary>
        /// Convert position (row, col) to index (0-64)
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static int Ix(byte row, byte col)
        {
            return row * 8 + col;
        }

        /// <summary>
        /// Count the number of bits set to 1 in a bitboard (ulong)
        /// </summary>
        /// <param name="bits"></param>
        /// <returns>Count number of bit 1</returns>
        public static byte PopCount(this ulong bits)
        {
            //https://stackoverflow.com/a/51388846/11898496
            byte count = 0;
            for (; bits != 0; ++count)
                bits &= bits - 1;
            return count;
        }

        public static ulong HighestOneBit(this ulong bits)
        {
            var i = bits;
            i |= (i >> 1);
            i |= (i >> 2);
            i |= (i >> 4);
            i |= (i >> 8);
            i |= (i >> 16);
            i |= (i >> 32);
            return i - (i >> 1);

            // highestOnBit hoạt động ntn:
            // https://stackoverflow.com/a/53369641/11898496
            // https://www.tutorialspoint.com/java/lang/long_highestonebit.htm
            // https://stackoverflow.com/questions/28846601/java-integer-highestonebit-in-c-sharp

            // trong c# không có >>>, nên mới phải cast ulong-long:
            // https://stackoverflow.com/a/2811372/11898496
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators#right-shift-operator-
        }

        // ------------------------------------ Display Stuffs ------------------------------------
        public static void Draw(this ulong bits)
        {
            for (var i = 0; i < 64; i++)
            {
                if (i % 8 == 0) Console.WriteLine();

                var pos = 1UL << i;
                bool isSetOne = (bits & pos) > 0;

                if (isSetOne) Console.Write("o ");
                else Console.Write(". ");
            }
            Console.WriteLine();
        }
    }
}
