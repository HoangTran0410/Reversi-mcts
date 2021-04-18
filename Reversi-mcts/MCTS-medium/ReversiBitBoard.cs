using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts.MCTS_medium
{
    // https://stackoverflow.com/a/28846859/11898496
    public static class Extensions
    {
        public static long HighestOneBit(this long number)
        {
            return (long)Math.Pow(2, Convert.ToString(number, 2).Length - 1);
        }
    }

    // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum
    // https://github.com/EivindEE/Reversi/blob/master/src/com/eivind/reversi/game/Color.java
    public enum Color : byte
    {
        Black = (byte)0,
        White = (byte)1
    }

    // https://github.com/EivindEE/Reversi/blob/master/src/com/eivind/reversi/game/ReversiBitboard.java
    class ReversiBitBoard
    {
        public readonly static int
            BLACK = 0,
            EMPTY = -1,
            WHITE = 1;

        public readonly static int
            NUMBER_OF_ROWS = 8,
            NUMBER_OF_COLUMNS = 8;

        private readonly static long
            INITIAL_POSITION_BLACK = 68853694464L,
            INITIAL_POSITION_WHITE = 34628173824L,
            LEFT_MASK = -9187201950435737472L,
            RIGHT_MASK = 72340172838076673L;

        private long[] pieces;


        /// <summary>
        /// Constructs a board with pieces in the starting positions of reversi
        /// </summary>
        public ReversiBitBoard()
            : this(INITIAL_POSITION_BLACK, INITIAL_POSITION_WHITE) { }

        /// <summary>
        /// Constructs a board with pieces in the positions given by blackPieces and whitePieces
        /// </summary>
        /// <param name="blackPieces">Black pieces</param>
        /// <param name="whitePieces">White pieces</param>
        public ReversiBitBoard(long blackPieces, long whitePieces)
            : this(new long[] { blackPieces, whitePieces }) { }

        /**
		 * Constructs a board with pieces in the positions given by the array
		 */
        public ReversiBitBoard(long[] pieces)
        {
            if (pieces.Length != 2)
                throw new Exception("Number of players must be 2 but was " + pieces.Length);
            this.pieces = new long[] { pieces[0], pieces[1] };
        }

        /**
		 * Constructs a board with pieces in the positions given by the array
		 */
        public ReversiBitBoard(long[] pieces, Move move)
            : this(pieces)
        {
            if (!MakeMove(move.Player, move.Coordinate))
                throw new Exception("Illegal move. No such move allowed on position");
        }


        public ReversiBitBoard(ReversiBitBoard board, Move m)
            : this(board.pieces, m) { }

        // https://stackoverflow.com/questions/169973/when-should-i-use-a-list-vs-a-linkedlist
        // https://stackoverflow.com/questions/5983059/why-is-a-linkedlist-generally-slower-than-a-list
        public List<Coordinate> GetLegalMoves(int player)
        {
            List<Coordinate> legalMoves = new List<Coordinate>();
            legalMoves.AddRange(MovesUpLeft(player));
            legalMoves.AddRange(MovesUp(player));
            legalMoves.AddRange(MovesUpRight(player));
            legalMoves.AddRange(MovesLeft(player));
            legalMoves.AddRange(MovesRight(player));
            legalMoves.AddRange(MovesDownLeft(player));
            legalMoves.AddRange(MovesDown(player));
            legalMoves.AddRange(MovesDownRight(player));
            return legalMoves;

        }

        /// <summary>
        /// Returns the score of the player with the given color
        /// Should be called with one of the class constants.
        /// </summary>
        /// <param name="color">color of player u want to calculate score</param>
        /// <returns>score of given player's color</returns>
        public int GetScore(int color)
        {
            int score = 0;
            long remainingPieces = pieces[color];
            while (remainingPieces != 0L)
            {
                score++;
                remainingPieces ^= remainingPieces.HighestOneBit();
                // https://www.tutorialspoint.com/java/lang/long_highestonebit.htm
                // https://stackoverflow.com/questions/28846601/java-integer-highestonebit-in-c-sharp
            }
            return score;
        }

        /// <summary>
        /// Return the int corresponding to the class constant of the content of the tile
        /// Should be used in conjunction with the class constants
        /// </summary>
        /// <param name="c"></param>
        /// <returns>int value of color</returns>
        public int GetTile(Coordinate c)
        {
            long position = Getlong(c);
            if ((position & pieces[BLACK]) == position)
                return BLACK;
            else if ((position & pieces[WHITE]) == position)
                return WHITE;
            else if ((position & EmptyBoard()) == position)
                return EMPTY;
            else
                throw new Exception("Coordinate " + c + "does not appear  on the board");
        }

        /// <summary>
        /// Returns true if the player with the given index has legal moves
        /// Should be called with either the static BLACK or WHITE
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public bool HasLegalMoves(int color)
        {
            return GetLegalMoves(color).Count > 0;
        }

        /// <summary>
        /// Returns true if the tile at c is empty
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsEmpty(Coordinate c)
        {
            long position = Getlong(c);
            return (position & EmptyBoard()) == position;
        }

        /// <summary>
        /// Returns true if none of the players have legal moves
        /// </summary>
        /// <returns></returns>
        public bool IsGameComplete()
        {
            return !HasLegalMoves(BLACK) && !HasLegalMoves(WHITE);
        }

        /// <summary>
        /// Returns a string representation of the ReversiBoard
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string s;
            string white = LongToString(pieces[WHITE]);
            string black = LongToString(pieces[BLACK]);
            string both = CombineBoards(white, black);
            s = DisplaySideBySide(white, black, both);
            return s;
        }

        /// <summary>
        /// Return a string represent for 2 string pieces
        /// </summary>
        /// <param name="white"></param>
        /// <param name="black"></param>
        /// <returns></returns>
        private string CombineBoards(string white, string black)
        {
            // https://www.stdio.vn/java/toi-uu-xu-ly-chuoi-voi-stringbuilder-phan-1-9RG31h
            StringBuilder s = new StringBuilder(white.Length);
            for (int i = 0; i < white.Length && i < black.Length; i++)
                if (white[i] == ' ')
                    s.Append(' ');
                else if (white[i] == '1' && black[i] == '1')
                    s.Append('X');
                else if (white[i] == '1')
                    s.Append('w');
                else if (black[i] == '1')
                    s.Append('b');
                else
                    s.Append('.');

            return s.ToString();
        }


        private string DisplaySideBySide(string white, string black, string both)
        {
            string s = "";
            int lineLength = 16;
            int beginIndex = 0;
            int endIndex = lineLength;
            while (endIndex <= black.Length || endIndex <= white.Length)
            {
                s += white.Substring(beginIndex, endIndex) + '\t'
                  + black.Substring(beginIndex, endIndex) + '\t'
                  + both.Substring(beginIndex, endIndex) + '\n';
                beginIndex = endIndex;
                endIndex += lineLength;
            }
            return s;
        }

        private long EmptyBoard()
        {
            return ~pieces[BLACK] & ~pieces[WHITE];
        }

        private Coordinate GetCoordinate(long position)
        {
            Coordinate theCoordinate = null;
            long L = 1L;
            int counter = 0;
            while (theCoordinate == null && counter < 64)
            {
                if (L == position)
                {
                    return new Coordinate((7 - (counter % 8)), counter / 8);
                }
                L = L << 1;
                counter++;
            }
            // If the long does not map to a coordinate on the board.
            throw new Exception("long " + position + "does not map to a coordinate on the BitBoard");
        }

        private List<Coordinate> GetEndPointDown(int color, long startPoint)
        {
            long potentialEndPoint = ShiftDown(startPoint);
            while (potentialEndPoint != 0)
            {
                if (ValidEndPoint(color, potentialEndPoint))
                {
                    return LongToCoordinateList(potentialEndPoint);
                }
                potentialEndPoint = ShiftDown(potentialEndPoint);
            }
            return new List<Coordinate>();
        }

        private List<Coordinate> GetEndPointDownLeft(int color, long startPoint)
        {
            long potentialEndPoint = ShiftDownLeft(startPoint);

            while (potentialEndPoint != 0)
            {
                if (ValidEndPoint(color, potentialEndPoint))
                {
                    return LongToCoordinateList(potentialEndPoint);
                }
                potentialEndPoint = ShiftDownLeft(potentialEndPoint);
            }
            return new List<Coordinate>();
        }

        private List<Coordinate> GetEndPointDownRight(int color, long startPoint)
        {
            long potentialEndPoint = ShiftDownRight(startPoint);
            while (potentialEndPoint != 0)
            {
                if (ValidEndPoint(color, potentialEndPoint))
                {
                    return LongToCoordinateList(potentialEndPoint);
                }
                potentialEndPoint = ShiftDownRight(potentialEndPoint);
            }
            return new List<Coordinate>();
        }

        private List<Coordinate> GetEndPointLeft(int color, long startPoint)
        {
            long potentialEndPoint = ShiftLeft(startPoint);
            while (potentialEndPoint != 0)
            {
                if (ValidEndPoint(color, potentialEndPoint))
                {
                    return LongToCoordinateList(potentialEndPoint);
                }
                potentialEndPoint = ShiftLeft(potentialEndPoint);
            }
            return new List<Coordinate>();
        }

        private List<Coordinate> GetEndPointRight(int color, long startPoint)
        {
            long potentialEndPoint = ShiftRight(startPoint);
            while (potentialEndPoint != 0)
            {
                if (ValidEndPoint(color, potentialEndPoint))
                {
                    return LongToCoordinateList(potentialEndPoint);
                }
                potentialEndPoint = ShiftRight(potentialEndPoint);
            }
            return new List<Coordinate>();
        }

        private List<Coordinate> GetEndPoints(int color, Coordinate coordinate)
        {
            List<Coordinate> endPoints = new List<Coordinate>();
            long startPoint = Getlong(coordinate);

            // Adds the potential endpoints from all possible directions
            endPoints.AddRange(GetEndPointUpLeft(color, startPoint));
            endPoints.AddRange(GetEndPointUp(color, startPoint));
            endPoints.AddRange(GetEndPointUpRight(color, startPoint));
            endPoints.AddRange(GetEndPointLeft(color, startPoint));
            endPoints.AddRange(GetEndPointRight(color, startPoint));
            endPoints.AddRange(GetEndPointDownLeft(color, startPoint));
            endPoints.AddRange(GetEndPointDown(color, startPoint));
            endPoints.AddRange(GetEndPointDownRight(color, startPoint));

            return endPoints;
        }
        private List<Coordinate> GetEndPointUp(int color, long startPoint)
        {
            long potentialEndPoint = ShiftUp(startPoint);
            while (potentialEndPoint != 0)
            {
                if (ValidEndPoint(color, potentialEndPoint))
                {
                    return LongToCoordinateList(potentialEndPoint);
                }
                potentialEndPoint = ShiftUp(potentialEndPoint);
            }
            return new List<Coordinate>();
        }

        private List<Coordinate> GetEndPointUpLeft(int color, long startPoint)
        {
            long potentialEndPoint = ShiftUpLeft(startPoint);
            while (potentialEndPoint != 0)
            {
                if (ValidEndPoint(color, potentialEndPoint))
                {
                    return LongToCoordinateList(potentialEndPoint);
                }
                potentialEndPoint = ShiftUpLeft(potentialEndPoint);
            }
            return new List<Coordinate>();
        }
        private List<Coordinate> GetEndPointUpRight(int color, long startPoint)
        {
            long potentialEndPoint = ShiftUpRight(startPoint);
            while (potentialEndPoint != 0)
            {
                if (ValidEndPoint(color, potentialEndPoint))
                {
                    return LongToCoordinateList(potentialEndPoint);
                }
                potentialEndPoint = ShiftUpRight(potentialEndPoint);
            }
            return new List<Coordinate>();
        }


        private long Getlong(Coordinate c)
        {
            long l = 1L;
            l = l << 7 - c.X;
            l = l << (8 * c.Y);
            return l;
        }

        private List<Coordinate> GetlongCoordinates(long position)
        {
            long workingPosition = position;
            List<Coordinate> coordinates = new List<Coordinate>();
            long highestOneBit = workingPosition.HighestOneBit();
            while (highestOneBit != 0)
            {
                coordinates.Add(GetCoordinate(highestOneBit));
                workingPosition ^= highestOneBit;
                highestOneBit = workingPosition.HighestOneBit();
            }
            return coordinates;
        }

        private bool IsLegalMove(int color, Coordinate coordinate)
        {
            foreach (Coordinate c in GetLegalMoves(color))
            {
                if (c.Equals(coordinate))
                    return true;
            }
            return false;
        }


        private List<Coordinate> LongToCoordinateList(long l)
        {
            List<Coordinate> coordinateList = new List<Coordinate>();
            coordinateList.Add(GetCoordinate(l));
            return coordinateList;
        }
        private string LongToString(long l)
        {
            // https://stackoverflow.com/questions/4325267/c-sharp-convert-int-to-string-with-padding-zeros
            return l.ToString("D64");
            //StringBuilder s = new StringBuilder(128);
            //int leadingZeros = Long.numberOfLeadingZeros(l);
            //for (int i = 0; i < leadingZeros; i++)
            //{
            //    s.Append("0 ");
            //}
            //if (s.Length == 64)
            //    return s.ToString();

            //string binString = Long.toBinaryString(l);
            //for (int i = 0; i < binString.Length; i++)
            //{
            //    s.Append(binString[i] + " ");
            //}
            //return s.ToString();
        }

        /**
		 * Places a piece at coordinate, and turns appropriate pieces.
		 * Returns true iff move is legal and completed.
		 * @param color
		 * @param coordinate
		 * @return
		 */
        private bool MakeMove(int color, Coordinate coordinate)
        {
            if (IsLegalMove(color, coordinate))
            {
                List<Coordinate> piecesToTurn = new List<Coordinate>();
                piecesToTurn.Add(coordinate);
                List<Coordinate> endPoints = GetEndPoints(color, coordinate);
                piecesToTurn.AddRange(endPoints);
                foreach (Coordinate c in endPoints)
                    piecesToTurn.AddRange(coordinate.Between(c));
                foreach (Coordinate c in piecesToTurn)
                {
                    SetPieceAtPosition(color, Getlong(c));
                }
                return true;
            }
            return false;
        }

        private List<Coordinate> MovesDown(int player)
        {
            List<Coordinate> downMoves = new List<Coordinate>();
            int otherPlayer = 1 - player;
            long potentialMoves = ShiftDown(pieces[player]) & pieces[otherPlayer];
            long emptyBoard = EmptyBoard();
            while (potentialMoves != 0)
            {
                long legalMoves = ShiftDown(potentialMoves) & emptyBoard;
                downMoves.AddRange(GetlongCoordinates(legalMoves));
                potentialMoves = ShiftDown(potentialMoves) & pieces[otherPlayer];
            }
            return downMoves;
        }

        private List<Coordinate> MovesDownLeft(int player)
        {
            List<Coordinate> downLeftMoves = new List<Coordinate>();
            int otherPlayer = 1 - player;
            long potentialMoves = ShiftDownLeft(pieces[player]) & pieces[otherPlayer];
            long emptyBoard = EmptyBoard();
            while (potentialMoves != 0)
            {
                long legalMoves = ShiftDownLeft(potentialMoves) & emptyBoard;
                downLeftMoves.AddRange(GetlongCoordinates(legalMoves));
                potentialMoves = ShiftDownLeft(potentialMoves) & pieces[otherPlayer];
            }
            return downLeftMoves;
        }

        private List<Coordinate> MovesDownRight(int player)
        {
            List<Coordinate> downRightMoves = new List<Coordinate>();
            int otherPlayer = 1 - player;
            long potentialMoves = ShiftDownRight(pieces[player]) & pieces[otherPlayer];
            long emptyBoard = EmptyBoard();
            while (potentialMoves != 0)
            {
                long legalMoves = ShiftDownRight(potentialMoves) & emptyBoard;
                downRightMoves.AddRange(GetlongCoordinates(legalMoves));
                potentialMoves = ShiftDownRight(potentialMoves) & pieces[otherPlayer];
            }
            return downRightMoves;
        }

        private List<Coordinate> MovesLeft(int player)
        {
            List<Coordinate> leftMoves = new List<Coordinate>();
            int otherPlayer = 1 - player;
            long potentialMoves = ShiftLeft(pieces[player]) & pieces[otherPlayer];
            long emptyBoard = EmptyBoard();
            while (potentialMoves != 0)
            {
                long legalMoves = ShiftLeft(potentialMoves) & emptyBoard;
                leftMoves.AddRange(GetlongCoordinates(legalMoves));
                potentialMoves = ShiftLeft(potentialMoves) & pieces[otherPlayer];
            }
            return leftMoves;
        }

        private List<Coordinate> MovesRight(int player)
        {
            List<Coordinate> rightMoves = new List<Coordinate>();
            int otherPlayer = 1 - player;
            long potentialMoves = ShiftRight(pieces[player]) & pieces[otherPlayer];
            long emptyBoard = EmptyBoard();
            while (potentialMoves != 0)
            {
                long legalMoves = ShiftRight(potentialMoves) & emptyBoard;
                rightMoves.AddRange(GetlongCoordinates(legalMoves));
                potentialMoves = ShiftRight(potentialMoves) & pieces[otherPlayer];
            }
            return rightMoves;
        }

        private List<Coordinate> MovesUp(int player)
        {
            List<Coordinate> upMoves = new List<Coordinate>();
            int otherPlayer = 1 - player;
            long potentialMoves = ShiftUp(pieces[player]) & pieces[otherPlayer];
            long emptyBoard = EmptyBoard();
            while (potentialMoves != 0)
            {
                long legalMoves = ShiftUp(potentialMoves) & emptyBoard;
                upMoves.AddRange(GetlongCoordinates(legalMoves));
                potentialMoves = ShiftUp(potentialMoves) & pieces[otherPlayer];
            }
            return upMoves;
        }

        private List<Coordinate> MovesUpLeft(int player)
        {
            List<Coordinate> upLeftMoves = new List<Coordinate>();
            int otherPlayer = 1 - player;
            long potentialMoves = ShiftUpLeft(pieces[player]) & pieces[otherPlayer];
            long emptyBoard = EmptyBoard();
            while (potentialMoves != 0)
            {
                long legalMoves = ShiftUpLeft(potentialMoves) & emptyBoard;
                upLeftMoves.AddRange(GetlongCoordinates(legalMoves));
                potentialMoves = ShiftUpLeft(potentialMoves) & pieces[otherPlayer];
            }
            return upLeftMoves;
        }

        private List<Coordinate> MovesUpRight(int player)
        {
            List<Coordinate> upRightMoves = new List<Coordinate>();
            int otherPlayer = 1 - player;
            long potentialMoves = ShiftUpRight(pieces[player]) & pieces[otherPlayer];
            long emptyBoard = EmptyBoard();
            while (potentialMoves != 0)
            {
                long legalMoves = ShiftUpRight(potentialMoves) & emptyBoard;
                upRightMoves.AddRange(GetlongCoordinates(legalMoves));
                potentialMoves = ShiftUpRight(potentialMoves) & pieces[otherPlayer];
            }
            return upRightMoves;
        }

        private void SetPieceAtPosition(int color, long position)
        {
            this.pieces[1 - color] &= ~position;
            this.pieces[color] = this.pieces[color] | position;
        }

        private long ShiftDown(long position)
        {
            return position >> 8;
        }

        private long ShiftDownLeft(long position)
        {
            long dlShift = position >> 7;
            return dlShift & ~RIGHT_MASK;
        }

        private long ShiftDownRight(long position)
        {
            long drShift = position >> 9;
            return drShift & ~LEFT_MASK;
        }

        private long ShiftLeft(long position)
        {
            long lShift = position << 1;
            return lShift & ~RIGHT_MASK;
        }

        private long ShiftRight(long position)
        {
            long rShift = position >> 1;
            return rShift & ~LEFT_MASK;
        }

        private long ShiftUp(long position)
        {
            return position << 8;
        }

        private long ShiftUpLeft(long position)
        {
            long ulShift = position << 9;
            return ulShift & ~RIGHT_MASK;
        }


        private long ShiftUpRight(long position)
        {
            long urShift = position << 7; // 7L
            return urShift & ~LEFT_MASK;
        }

        private bool ValidEndPoint(int color, long potentialEndPoint)
        {
            return (potentialEndPoint & pieces[color]) == potentialEndPoint;
        }

        /**
		 * Two boards are equal if they have the same pieces at the same places.
		 */
        public override bool Equals(Object obj)
        {
            if (obj.GetType() == typeof(ReversiBitBoard)){
                ReversiBitBoard b = (ReversiBitBoard)obj;
                if (b.pieces[0] == this.pieces[0] && b.pieces[1] == this.pieces[1])
                    return true;

            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(pieces);
        }
    }
}
