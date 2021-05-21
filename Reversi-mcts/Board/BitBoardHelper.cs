using System;
using System.Collections.Generic;

namespace Reversi_mcts.Board
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

        // ------------------------------------ Flip Rotate ------------------------------------
        // https://www.chessprogramming.org/Flipping_Mirroring_and_Rotating
        public static ulong Rotate180(this ulong b)
        {
            return b.FlipVertical().MirrorHorizontal();
        }

        public static ulong Rotate90Clockwise(this ulong b)
        {
            return b.FlipDiagA1H8().FlipVertical();
        }

        public static ulong Rotate90AntiClockwise(this ulong b)
        {
            return b.FlipVertical().FlipDiagA1H8();
        }

        public static ulong FlipVertical(this ulong b)
        {
            return ((b << 56)) |
                   ((b << 40) & 0x00ff000000000000) |
                   ((b << 24) & 0x0000ff0000000000) |
                   ((b << 8) & 0x000000ff00000000) |
                   ((b >> 8) & 0x00000000ff000000) |
                   ((b >> 24) & 0x0000000000ff0000) |
                   ((b >> 40) & 0x000000000000ff00) |
                   ((b >> 56));
        }

        public static ulong MirrorHorizontal(this ulong b)
        {
            const ulong k1 = 0x5555555555555555;
            const ulong k2 = 0x3333333333333333;
            const ulong k4 = 0x0f0f0f0f0f0f0f0f;
            b = ((b >> 1) & k1) + 2 * (b & k1);
            b = ((b >> 2) & k2) + 4 * (b & k2);
            b = ((b >> 4) & k4) + 16 * (b & k4);
            return b;
        }

        // do bitboard của mình ngược so với người ta
        // mình: index 0 ở A1
        // người ta: index 0 ở H8
        // => Nên hàm FlipDiag A8H1 và A1H8 sẽ đổi code với nhau
        public static ulong FlipDiagA8H1(this ulong b)
        {
            const ulong k1 = 0x5500550055005500;
            const ulong k2 = 0x3333000033330000;
            const ulong k4 = 0x0f0f0f0f00000000;
            var t = k4 & (b ^ (b << 28));
            b ^= t ^ (t >> 28);
            t = k2 & (b ^ (b << 14));
            b ^= t ^ (t >> 14);
            t = k1 & (b ^ (b << 7));
            b ^= t ^ (t >> 7);
            return b;
        }

        public static ulong FlipDiagA1H8(this ulong b)
        {
            const ulong k1 = 0xaa00aa00aa00aa00;
            const ulong k2 = 0xcccc0000cccc0000;
            const ulong k4 = 0xf0f0f0f00f0f0f0f;
            var t = b ^ (b << 36);
            b ^= k4 & (t ^ (b >> 36));
            t = k2 & (b ^ (b << 18));
            b ^= t ^ (t >> 18);
            t = k1 & (b ^ (b << 9));
            b ^= t ^ (t >> 9);
            return b;
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

        // Định dạng ulong[] ít tốn Ram hơn List<ulong>
        public static ulong[] ToArrayBitMove(this ulong bits)
        {
            var result = new ulong[bits.PopCount()];
            var index = 0;
            while (bits != 0)
            {
                var m = bits.HighestOneBit();
                bits ^= m;
                result[index] = m;
                index++;
            }

            return result;
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

        public static ulong SetBitAtCoordinate(this ulong bits, int row, int col)
        {
            return bits.SetBitAdIndex(Ix(row, col));
        }

        public static ulong RemoveBitAtCoordinate(this ulong bits, int row, int col)
        {
            return bits.RemoveBitAtIndex(Ix(row, col));
        }

        public static ulong SetBitAdIndex(this ulong bits, int index)
        {
            // https://stackoverflow.com/a/24250656/11898496
            return bits | (1UL << index);
        }

        public static ulong RemoveBitAtIndex(this ulong bits, int index)
        {
            return bits & ~(1UL << index);
        }

        private static int Ix(int row, int col)
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
        public static void Display(this ulong bits)
        {
            for (var i = 0; i < 64; i++)
            {
                if (i % 8 == 0 && i != 0) Console.WriteLine();

                var pos = 1UL << i;
                var isSetOne = (bits & pos) > 0;

                Console.Write(isSetOne ? "o " : ". ");
            }

            Console.WriteLine();
        }
    }
}