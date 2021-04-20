using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    public enum Color : byte
    {
        Black = 0,
        White = 1,
        Empty = 2
    }

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

    public class ReversiBitBoard
    {
        // Why use ulong instead of long: https://stackoverflow.com/a/9924991/11898496
        public readonly static ulong
            InitialPositionBlack = 0x810000000, // 00000000 00000000 00000000 00001000 00010000 00000000 00000000 00000000
            InitialPositionWhite = 0x1008000000,// 00000000 00000000 00000000 00010000 00001000 00000000 00000000 00000000
            LeftMask = 0x7F7F7F7F7F7F7F7F,      // 01111111 01111111 01111111 01111111 01111111 01111111 01111111 01111111
            RightMask = 0xFEFEFEFEFEFEFEFE;     // 11111110 11111110 11111110 11111110 11111110 11111110 11111110 11111110

        public ulong BlackPiece { get; }
        public ulong WhitePiece { get; }

        public ReversiBitBoard() : this(InitialPositionBlack, InitialPositionWhite) { }

        public ReversiBitBoard(ulong blackPiece, ulong whitePiece)
        {
            BlackPiece = blackPiece;
            WhitePiece = whitePiece;
        }
    }

    public static class Extensions
    {
        // ------------------------------------ Pre Define ------------------------------------
        /** Caches the direction enum values in a array. */
        public static Direction[] DirectionValues = (Direction[])Enum.GetValues(typeof(Direction));

        // ------------------------------------ Basic Stuffs ------------------------------------
        /// <summary>
        /// Return a clone of board
        /// </summary>
        /// <param name="board"></param>
        /// <returns>New board cloned</returns>
        public static ReversiBitBoard Clone(this ReversiBitBoard board)
        {
            return new ReversiBitBoard(board.BlackPiece, board.WhitePiece);
        }

        /// <summary>
        /// Get pieces of color
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player"></param>
        /// <returns>Pieces of color in board</returns>
        public static ulong GetPieceOf(this ReversiBitBoard board, Color player)
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
        public static ulong GetOpponentPieceOf(this ReversiBitBoard board, Color player)
        {
            if (player == Color.Black) return board.WhitePiece;
            return board.BlackPiece;
        }

        /// <summary>
        /// Get all empty cells of board
        /// </summary>
        /// <param name="board"></param>
        /// <returns>Bits that contains all empty cells</returns>
        public static ulong EmptyCells(this ReversiBitBoard board)
        {
            return ~board.BlackPiece & ~board.WhitePiece;
        }

        /// <summary>
        /// Get all played cells (white/black cells) of board
        /// </summary>
        /// <param name="board"></param>
        /// <returns>Bits that contains all played cells</returns>
        public static ulong PlayedCells(this ReversiBitBoard board)
        {
            return board.BlackPiece | board.WhitePiece;
        }

        // ------------------------------------ Move Stuffs ------------------------------------
        /// <summary>
        /// Places a piece at coordinate, and turns appropriate pieces
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player">Color of player make move</param>
        /// <param name="coordinate">Position of move</param>
        /// <returns>Returns true if move is legal and completed</returns>
        public static bool MakeMove(this ReversiBitBoard board, Color player, byte row, byte col)
        {
            if (board.IsLegalMove(player, row, col))
            {
                ulong endPoints = board.GetEndPoints(player, row, col);

                Console.WriteLine(endPoints.ToPrettyString());

                //List<Coordinate> piecesToTurn = new List<Coordinate>();
                //piecesToTurn.Add(coordinate);

                //List<Coordinate> endPoints = GetEndPoints(color, coordinate);
                //piecesToTurn.AddRange(endPoints);

                //foreach (Coordinate c in endPoints)
                //    piecesToTurn.AddRange(Coordinate.Between(coordinate, c));

                //foreach (Coordinate c in piecesToTurn)
                //{
                //    SetPieceAtPosition(color, GetLong(c));
                //}
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if player has legal moves on current board
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player">Color of player</param>
        /// <returns>Has legal moves or not</returns>
        public static bool HasLegalMoves(this ReversiBitBoard board, Color player)
        {
            return board.GetLegalMoves(player) != 0;
        }

        /// <summary>
        /// Returns the boolean value telling if the move, done by the specified player, is legal.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player">The player moving</param>
        /// <param name="coordinate">The square where to put the new disk</param>
        /// <returns>True if the move is legal, otherwise false</returns>
        public static bool IsLegalMove(this ReversiBitBoard board, Color player, byte row, byte col)
        {
            ulong position = CoordinateToLong(row, col);
            ulong legalMoves = board.GetLegalMoves(player);
            return (position & legalMoves) != 0;
        }

        /// <summary>
        /// Get all legal moves of player (Line Cap Moves Algorithm)
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player">Player want to get legal moves</param>
        /// <returns>Bits that contains all legal moves</returns>
        public static ulong GetLegalMoves(this ReversiBitBoard board, Color player)
        {
            // https://intellitect.com/when-to-use-and-not-use-var-in-c/
            // https://stackoverflow.com/a/41505/11898496
            ulong moves = 0UL;
            ulong bitsp = board.GetPieceOf(player);
            ulong bitso = board.GetOpponentPieceOf(player);
            ulong empty = board.EmptyCells();

            // https://stackoverflow.com/a/105402/11898496
            foreach (Direction dir in DirectionValues)
            {
                ulong candidates = bitso & bitsp.Shift(dir);
                while (candidates != 0)
                {
                    ulong candidatesShifted = candidates.Shift(dir);
                    moves |= empty & candidatesShifted;
                    candidates = bitso & candidatesShifted;
                }
            }

            return moves;
        }

        /// <summary>
        /// Get all cells can be flip if player play at position (row, col)
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player">Color of player</param>
        /// <param name="row">Row to play</param>
        /// <param name="col">Col to play</param>
        /// <returns>Bits that represent all valid cells</returns>
        public static ulong GetEndPoints(this ReversiBitBoard board, Color player, byte row, byte col)
        {
            ulong playerPiece = board.GetPieceOf(player);
            ulong startPoint = CoordinateToLong(row, col);
            ulong endPoints = 0L;

            foreach (Direction dir in DirectionValues)
            {
                ulong potentialEndPoint = startPoint.Shift(dir);
                while (potentialEndPoint != 0)
                {
                    if ((potentialEndPoint & playerPiece) == potentialEndPoint)
                    {
                        endPoints |= potentialEndPoint;
                        break;
                    }

                    potentialEndPoint = potentialEndPoint.Shift(dir);
                }
            }

            return endPoints;
        }

        // ------------------------------------ Board Filters Stuffs ------------------------------------
        /// <summary>
        /// Get all empty-neighbour cells of both players
        /// </summary>
        /// <param name="board"></param>
        /// <returns>Bits that contains all neighbour cells</returns>
        public static ulong EmptyNeighbours(this ReversiBitBoard board)
        {
            return board.PlayedCells().Delation() & board.EmptyCells();
        }

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
            switch (direction)
            {
                case Direction.Up: return bits.ShiftUp();
                case Direction.Down: return bits.ShiftDown();
                case Direction.Left: return bits.ShiftLeft();
                case Direction.Right: return bits.ShiftRight();
                case Direction.UpLeft: return bits.ShiftUpLeft();
                case Direction.UpRight: return bits.ShiftUpRight();
                case Direction.DownLeft: return bits.ShiftDownLeft();
                case Direction.DownRight: return bits.ShiftDownRight();
            }
            return 0;
        }

        // https://www.gamedev.net/forums/topic/646988-generating-moves-in-reversi/
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
            return (bits & ReversiBitBoard.RightMask) << 1;
        }
        public static ulong ShiftRight(this ulong bits)
        {
            return (bits & ReversiBitBoard.LeftMask) >> 1;
        }
        public static ulong ShiftUpLeft(this ulong bits)
        {
            return bits.ShiftUp().ShiftLeft();
        }
        public static ulong ShiftUpRight(this ulong bits)
        {
            return bits.ShiftUp().ShiftRight();
        }
        public static ulong ShiftDownLeft(this ulong bits)
        {
            return bits.ShiftDown().ShiftLeft();
        }
        public static ulong ShiftDownRight(this ulong bits)
        {
            return bits.ShiftDown().ShiftRight();
        }

        // ------------------------------------ Bit Stuffs ------------------------------------
        /// <summary>
        /// Convert coordinate (row, col) to binary bits (long)
        /// </summary>
        /// <param name="coordinate">Coordinate (row, col) to convert</param>
        /// <returns>Bits that represent coordinate</returns>
        public static ulong CoordinateToLong(byte row, byte col)
        {
            return 0UL.SetBitAtCoordinate(row, col);
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
            return (8 - row) * 8 + (8 - col);
        }

        /// <summary>
        /// Count the number of bits set to 1 in a bitboard (long)
        /// </summary>
        /// <param name="bits"></param>
        /// <returns>Count number of bit 1</returns>
        public static int PopCount(this ulong bits)
        {
            //https://stackoverflow.com/a/51388846/11898496
            int count = 0;
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
        /// Phiên bản ngắn gọn của l.ToBinaryString().Pretty()
        /// </summary>
        /// <param name="bits"></param>
        /// <returns>Chuỗi string được định dạng giống 1 board 8x8</returns> 
        public static string ToPrettyString(this ulong bits)
        {
            return bits.ToBinaryString().Pretty();
        }

        /// <summary>
        /// Chuyển biến kiểu long về kiểu string (binary) với độ dài cụ thể.
        /// <para>Tự động thêm 0 phía trước nếu ko đủ dài</para>
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="totalWidth">Độ dài chuỗi binary string (mặc định là 64)</param>
        /// <returns>Chuỗi binary string với độ dài truyền vào</returns>
        public static string ToBinaryString(this ulong bits, byte totalWidth = 64)
        {
            // https://stackoverflow.com/a/23905301/11898496
            StringBuilder s = new StringBuilder(128);
            string binString = Convert.ToString((long)bits, 2).PadLeft(totalWidth, '0');
            for (byte i = 0; i < binString.Length; i++)
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
            var s = new StringBuilder(white.Length);
            for (byte i = 0; i < white.Length && i < black.Length; i++)
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
        /// <returns>Chuỗi string được định dạng giống 1 board 8x8</returns>
        public static string Pretty(this string binaryStr, char one = 'o', char zero = '.')
        {
            var s = new StringBuilder();
            byte lineLength = 8;
            byte beginIndex = 0;
            byte endIndex = lineLength;
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
