using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    public class ReversiBitBoard
    {
        public readonly static long
            INITIAL_POSITION_BLACK = 68853694464L,  // 00000000 00000000 00000000 00010000 00001000 00000000 00000000 00000000
            INITIAL_POSITION_WHITE = 34628173824L,  // 00000000 00000000 00000000 00001000 00010000 00000000 00000000 00000000
            LEFT_MASK = -9187201950435737472L,      // 10000000 10000000 10000000 10000000 10000000 10000000 10000000 10000000
            RIGHT_MASK = 72340172838076673L;        // 00000001 00000001 00000001 00000001 00000001 00000001 00000001 00000001

        public long BlackPiece { get; set; }
        public long WhitePiece { get; set; }

        public ReversiBitBoard()
        {
            BlackPiece = INITIAL_POSITION_BLACK;
            WhitePiece = INITIAL_POSITION_WHITE;
        }
    }

    public static class Extensions
    {
        // ------------------------------------ Basic Stuffs ------------------------------------
        public enum Color : byte
        {
            Black = 0,
            White = 1,
            Empty = 2
        }

        /// <summary>
        /// Get pieces of color
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player"></param>
        /// <returns>Pieces of color in board</returns>
        public static long GetPieceOf(this ReversiBitBoard board, Color player)
        {
            if (player == Color.Black) return board.BlackPiece;
            return board.WhitePiece;
        }

        /// <summary>
        /// Get pieces of opponent color
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player"></param>
        /// <returns>Pieces of opponent color in board</returns>
        public static long GetOpponentPieceOf(this ReversiBitBoard board, Color player)
        {
            if (player == Color.Black) return board.WhitePiece;
            return board.BlackPiece;
        }

        /// <summary>
        /// Get all empty cells of board
        /// </summary>
        /// <param name="board"></param>
        /// <returns>Bits that contains all empty cells</returns>
        public static long EmptyCells(this ReversiBitBoard board)
        {
            return ~board.BlackPiece & ~board.WhitePiece;
        }

        /// <summary>
        /// Get all played cells (white/black cells) of board
        /// </summary>
        /// <param name="board"></param>
        /// <returns>Bits that contains all played cells</returns>
        public static long PlayedCells(this ReversiBitBoard board)
        {
            return board.BlackPiece | board.WhitePiece;
        }

        // ------------------------------------ Move Stuffs ------------------------------------
        /// <summary>
        /// Get all legal moves of player
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public static long GetLegalMoves(this ReversiBitBoard board, Color player)
        {
            return 0;
        }

        // ------------------------------------ Board Filters Stuffs ------------------------------------
        /// <summary>
        /// Get all empty-neighbour cells of both players
        /// </summary>
        /// <param name="board"></param>
        /// <returns>Bits that contains all neighbour cells</returns>
        public static long EmptyNeighbours(this ReversiBitBoard board)
        {
            return board.PlayedCells().Delation() & board.EmptyCells();
        }

        // ------------------------------------ Line Move Filters ------------------------------------
        public static long LineCapMoves(this ReversiBitBoard board, Color player)
        {
            var moves = 0L;
            var bitsp = board.GetPieceOf(player);
            var bitso = board.GetOpponentPieceOf(player);
            var empty = board.EmptyCells();

            // https://stackoverflow.com/a/105402/11898496
            foreach (Direction dir in (Direction[])Enum.GetValues(typeof(Direction)))
            {
                long candidates = bitso & bitsp.Shift(dir);
                while (candidates != 0)
                {
                    moves |= empty & candidates.Shift(dir);
                    candidates = bitso & candidates.Shift(dir);
                }
            }

            return moves;
        }

        // ------------------------------------ Shift Stuffs ------------------------------------
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

        /// <summary>
        /// Shifts the bits in the direction
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="direction">Direction to shift</param>
        /// <returns>Shifted bits</returns>
        public static long Shift(this long bits, Direction direction)
        {
            switch (direction)
            {
                case Direction.UpLeft: return bits.ShiftUpLeft();
                case Direction.Up: return bits.ShiftUp();
                case Direction.UpRight: return bits.ShiftUpRight();
                case Direction.Left: return bits.ShiftLeft();
                case Direction.Right: return bits.ShiftRight();
                case Direction.DownLeft: return bits.ShiftDownLeft();
                case Direction.Down: return bits.ShiftDown();
                case Direction.DownRight: return bits.ShiftDownRight();
            }
            return 0;
        }

        public static long ShiftDown(this long bits)
        {
            return bits >> 8;
        }
        public static long ShiftDownLeft(this long bits)
        {
            long dlShift = bits >> 7;
            return dlShift & ~ReversiBitBoard.RIGHT_MASK;
        }
        public static long ShiftDownRight(this long bits)
        {
            long drShift = bits >> 9;
            return drShift & ~ReversiBitBoard.LEFT_MASK;
        }
        public static long ShiftLeft(this long bits)
        {
            long lShift = bits << 1;
            return lShift & ~ReversiBitBoard.RIGHT_MASK;
        }
        public static long ShiftRight(this long bits)
        {
            long rShift = bits >> 1;
            return rShift & ~ReversiBitBoard.LEFT_MASK;
        }
        public static long ShiftUp(this long bits)
        {
            return bits << 8;
        }
        public static long ShiftUpLeft(this long bits)
        {
            long ulShift = bits << 9;
            return ulShift & ~ReversiBitBoard.RIGHT_MASK;
        }
        public static long ShiftUpRight(this long bits)
        {
            long urShift = bits << 7;
            return urShift & ~ReversiBitBoard.LEFT_MASK;
        }

        // ------------------------------------ Morphological Operations ------------------------------------
        /// <summary>
        /// Expand the on-bits in all square (diagonal) directions.
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="step">Number of steps to expand</param>
        /// <returns></returns>
        public static long Delation(this long bits, int step = 1)
        {
            long ret = bits;
            while (step-- > 0)
            {
                ret =
                    ret |
                    ret.ShiftDown() |
                    ret.ShiftDownLeft() |
                    ret.ShiftDownRight() |
                    ret.ShiftLeft() |
                    ret.ShiftRight() |
                    ret.ShiftUp() |
                    ret.ShiftUpLeft() |
                    ret.ShiftUpRight();
            }
            return ret;
        }

        /// <summary>
        /// Shrinks the on-bits in all square (diagonal) directions. Reverse operation of dilation.
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="step">Number of steps to shrink</param>
        /// <returns></returns>
        public static long Erosion(this long bits, int step = 1)
        {
            long ret = bits;
            while (step-- > 0)
            {
                ret =
                    ret &
                    ret.ShiftDown() &
                    ret.ShiftDownLeft() &
                    ret.ShiftDownRight() &
                    ret.ShiftLeft() &
                    ret.ShiftRight() &
                    ret.ShiftUp() &
                    ret.ShiftUpLeft() &
                    ret.ShiftUpRight();
            }
            return ret;
        }

        // ------------------------------------ Bit Stuffs ------------------------------------
        public static long HighestOneBit(this long i)
        {
            i |= (i >> 1);
            i |= (i >> 2);
            i |= (i >> 4);
            i |= (i >> 8);
            i |= (i >> 16);
            i |= (i >> 32);
            return (long)((ulong)i - ((ulong)i >> 1));

            // highestOnBit hoạt động ntn:
            // https://stackoverflow.com/a/53369641/11898496
            // https://www.tutorialspoint.com/java/lang/long_highestonebit.htm
            // https://stackoverflow.com/questions/28846601/java-integer-highestonebit-in-c-sharp

            // right shift co 2 loại:
            // https://stackoverflow.com/a/2811372/11898496
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators#right-shift-operator-
        }

        // ------------------------------------ Display Stuffs ------------------------------------
        /// <summary>
        /// Chuyển board về dạng chuỗi để có thể hiển thị lên console
        /// </summary>
        /// <param name="bitBoard"></param>
        /// <returns>Chuỗi đã được làm đẹp, chứa các quân cờ của black và white</returns>
        public static string ToDisplayString(this ReversiBitBoard bitBoard)
        {
            string white = bitBoard.WhitePiece.ToBinaryString();
            string black = bitBoard.BlackPiece.ToBinaryString();
            string both = CombineBinaryString(white, black);
            return both.Pretty();
        }

        /// <summary>
        /// Chuyển biến kiểu long về kiểu string (binary) với độ dài cụ thể.
        /// <para>Tự động thêm 0 phía trước nếu ko đủ dài</para>
        /// </summary>
        /// <param name="l"></param>
        /// <param name="totalWidth">Độ dài chuỗi binary string (mặc định là 64)</param>
        /// <returns>Chuỗi binary string với độ dài truyền vào</returns>
        public static string ToBinaryString(this long l, int totalWidth = 64)
        {
            // https://stackoverflow.com/a/23905301/11898496
            StringBuilder s = new StringBuilder(128);
            string binString = Convert.ToString(l, 2).PadLeft(totalWidth, '0');
            for (int i = 0; i < binString.Length; i++)
            {
                s.Append(binString[i]);
            }
            return s.ToString();
        }

        /// <summary>
        /// Gộp 2 binary string của 2 pieces vào 1 single string.
        /// <para>Vị trí của white sẽ đánh dấu 'w'</para>
        /// <para>Vị trí của black sẽ đánh dấu 'b'</para>
        /// <para>Vị trí trống đánh dấu '.' (chấm)</para>
        /// <para>Vị trí không hợp lệ (vừa white vừa black 1 chỗ) đánh dấu 'X'</para>
        /// </summary>
        /// <param name="white">Binary string của white</param>
        /// <param name="black">Binary string của black</param>
        /// <returns>Chuỗi string biểu diễn vị trí của black và white trên board</returns>
        public static string CombineBinaryString(string white, string black)
        {
            // https://www.stdio.vn/java/toi-uu-xu-ly-chuoi-voi-stringbuilder-phan-1-9RG31h
            StringBuilder s = new StringBuilder(white.Length);
            for (int i = 0; i < white.Length && i < black.Length; i++)
                if (white[i] == '1' && black[i] == '1')
                    s.Append('X');
                else if (white[i] == '1')
                    s.Append('w');
                else if (black[i] == '1')
                    s.Append('b');
                else
                    s.Append('.');
            return s.ToString();
        }

        /// <summary>
        /// Làm đẹp chuỗi binary string để hiển thị lên console
        /// <para>Thêm các ký tự xuống dòng, khoảng trắng để tạo thành chuỗi có hình vuông 8x8</para>
        /// </summary>
        /// <param name="binaryStr">Chuỗi CombineBinaryString của black và white</param>
        /// <param name="one">Ký tự thay thế cho ký tự "1"</param>
        /// <param name="zero">Ký tự thay thế cho ký tự "0"</param>
        /// <returns>Chuỗi string sau khi làm đẹp</returns>
        public static string Pretty(this string binaryStr, char one = 'o', char zero = '.')
        {
            StringBuilder s = new StringBuilder();
            int lineLength = 8;
            int beginIndex = 0;
            int endIndex = lineLength;
            while (endIndex <= binaryStr.Length)
            {
                string sub = binaryStr.Substring(beginIndex, lineLength);
                foreach (char c in sub)
                {
                    char ch = c == '1' ? one : (c == '0' ? zero : c);
                    s.Append(ch + " ");
                }
                s.Append('\n');
                beginIndex = endIndex;
                endIndex += lineLength;
            }
            return s.ToString();
        }
    }
}
