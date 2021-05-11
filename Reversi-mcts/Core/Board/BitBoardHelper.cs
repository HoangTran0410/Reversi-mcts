using System;
using System.Collections.Generic;

namespace Reversi_mcts.Core.Board
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
        // ------------------------------------ Shift Stuffs ------------------------------------
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
        private const ulong
            LeftMask = 0x7F7F7F7F7F7F7F7F; // 01111111 01111111 01111111 01111111 01111111 01111111 01111111 01111111

        private const ulong
            RightMask = 0xFEFEFEFEFEFEFEFE; // 11111110 11111110 11111110 11111110 11111110 11111110 11111110 11111110

        private static ulong ShiftUp(this ulong bits)
        {
            return bits << 8;
        }

        private static ulong ShiftDown(this ulong bits)
        {
            return bits >> 8;
        }

        private static ulong ShiftLeft(this ulong bits)
        {
            return bits << 1 & RightMask;
        }

        private static ulong ShiftRight(this ulong bits)
        {
            return bits >> 1 & LeftMask;
        }

        private static ulong ShiftUpLeft(this ulong bits)
        {
            return bits << 9 & RightMask;
        }

        private static ulong ShiftUpRight(this ulong bits)
        {
            return bits << 7 & LeftMask;
        }

        private static ulong ShiftDownLeft(this ulong bits)
        {
            return bits >> 7 & RightMask;
        }

        private static ulong ShiftDownRight(this ulong bits)
        {
            return bits >> 9 & LeftMask;
        }

        // ------------------------------------ Bit Stuffs ------------------------------------
        public static List<ulong> ToListBitMove(this ulong bits)
        {
            var ret = new List<ulong>(bits.PopCount());
            while (bits != 0)
            {
                var m = bits.HighestOneBit();
                bits ^= m;
                ret.Add(m);
            }

            return ret;
        }

        // https://www.chessprogramming.org/BitScan#De_Bruijn_Multiplication
        private static readonly int[] Index64Reverse =
        {
            0, 47, 1, 56, 48, 27, 2, 60,
            57, 49, 41, 37, 28, 16, 3, 61,
            54, 58, 35, 52, 50, 42, 21, 44,
            38, 32, 29, 23, 17, 11, 4, 62,
            46, 55, 26, 59, 40, 36, 15, 53,
            34, 51, 20, 43, 31, 22, 10, 45,
            25, 39, 14, 33, 19, 30, 9, 24,
            13, 18, 8, 12, 7, 6, 5, 63
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
            return Index64Reverse[(bb * debruijn64) >> 58];
        }

        // https://www.chessprogramming.org/BitScan#Matt_Taylor.27s_Folding_trick
        private static readonly int[] Index64Forward =
        {
            63, 30, 3, 32, 59, 14, 11, 33,
            60, 24, 50, 9, 55, 19, 21, 34,
            61, 29, 2, 53, 51, 23, 41, 18,
            56, 28, 1, 43, 46, 27, 0, 35,
            62, 31, 58, 4, 5, 49, 54, 6,
            15, 52, 12, 40, 7, 42, 45, 16,
            25, 57, 48, 13, 10, 39, 8, 44,
            20, 47, 38, 22, 17, 37, 36, 26
        };

        public static int BitScanForward(this ulong bb)
        {
            uint folded;
            //assert(bb != 0);
            bb ^= bb - 1;
            folded = (uint) bb ^ (uint) (bb >> 32);
            return Index64Forward[folded * 0x78291ACF >> 26];
        }

        public static ulong SetBitAtCoordinate(this ulong bits, byte row, byte col)
        {
            // https://stackoverflow.com/a/24250656/11898496
            return bits | (1UL << Ix(row, col));
        }

        public static ulong RemoveBitAtCoordinate(this ulong bits, byte row, byte col)
        {
            return bits & ~(1UL << Ix(row, col));
        }

        private static int Ix(byte row, byte col)
        {
            return row * 8 + col;
        }

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
                var isSetOne = (bits & pos) > 0;

                Console.Write(isSetOne ? "o " : ". ");
            }

            Console.WriteLine();
        }
    }
}