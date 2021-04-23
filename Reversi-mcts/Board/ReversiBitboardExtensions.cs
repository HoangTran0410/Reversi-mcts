using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    public static class ReversiBitboardExtensions
    {
        // -------------------------- PreDefine - For performance boost --------------------------
        /** Caches the direction enum values in a array. */
        public static Direction[] DirectionValues = (Direction[])Enum.GetValues(typeof(Direction));
        /** Integer value for the black player. It is equal to Player.BLACK */
        public static int Black = 0;
        /** Integer value for the white player. It is equal to Player.WHITE */
        public static int White = 1;

        // ------------------------------------ Basic Stuffs ------------------------------------
        /// <summary>
        /// Return a clone of board
        /// </summary>
        /// <param name="board"></param>
        /// <returns>New board cloned</returns>
        public static ReversiBitboard Clone(this ReversiBitboard board)
        {
            return new ReversiBitboard(board);
        }

        /// <summary>
        /// Get all empty cells of board
        /// </summary>
        /// <param name="board"></param>
        /// <returns>Bits that contains all empty cells</returns>
        public static ulong GetEmpties(this ReversiBitboard board)
        {
            return ~board.BitBoard[Black] & ~board.BitBoard[White];
        }

        /// <summary>
        /// Get all played cells (white/black cells) of board
        /// </summary>
        /// <param name="board"></param>
        /// <returns>Bits that contains all played cells</returns>
        public static ulong GetFilled(this ReversiBitboard board)
        {
            return board.BitBoard[Black] | board.BitBoard[White];
        }

        // ------------------------------------ Move Stuffs ------------------------------------
        /// <summary>
        /// Places a piece at coordinate, and turns appropriate pieces
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player">Color of player make move</param>
        /// <param name="coordinate">Position of move</param>
        /// <returns>Returns true if move is legal and completed</returns>
        public static bool MakeMove(this ReversiBitboard board, int player, byte row, byte col)
        {
            ulong startPoint = BitboardExtensions.CoordinateToULong(row, col);
            if (!IsLegalMove(board, player, startPoint))
            {
                return false;
            }

            ulong wouldFlips = board.GetWouldFlips(player, startPoint);
            if (wouldFlips == 0)
            {
                return false;
            }

            board.BitBoard[player] |= wouldFlips | startPoint;
            board.BitBoard[1 ^ player] ^= wouldFlips;

            return true;
        }

        /// <summary>
        /// Check if player has legal moves on current board
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player">Color of player</param>
        /// <returns>Has legal moves or not</returns>
        public static bool HasLegalMoves(this ReversiBitboard board, int player)
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
        public static bool IsLegalMove(this ReversiBitboard board, int player, ulong bitmove)
        {
            ulong legalMoves = board.GetLegalMoves(player);
            return (bitmove & legalMoves) != 0;
        }

        /// <summary>
        /// Get all legal moves of player (Line Cap Moves Algorithm)
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player">Player want to get legal moves</param>
        /// <returns>Bits that contains all legal moves</returns>
        public static ulong GetLegalMoves(this ReversiBitboard board, int player)
        {
            // https://intellitect.com/when-to-use-and-not-use-var-in-c/
            // https://stackoverflow.com/a/41505/11898496
            ulong moves = 0UL;
            ulong bitsp = board.BitBoard[player];
            ulong bitso = board.BitBoard[1 ^ player];
            ulong empty = board.GetEmpties();

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
        /// Get all opponent cells would flip if player play at position (row, col)
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player">Color of player</param>
        /// <param name="row">Row to play</param>
        /// <param name="col">Col to play</param>
        /// <returns>Bits that represent all would flip cells</returns>
        public static ulong GetWouldFlips(this ReversiBitboard board, int player, ulong startPoint)
        {
            ulong playerBitboard = board.BitBoard[player];
            ulong opponentBitboard = board.BitBoard[1 ^ player];
            ulong wouldFlips = 0UL;

            foreach (Direction dir in DirectionValues)
            {
                ulong potentialEndPoint = startPoint.Shift(dir);
                ulong potentialWouldFlips = 0UL;
                bool foundEndPoint = false;

                while (potentialEndPoint != 0)
                {
                    if ((potentialEndPoint & playerBitboard) == potentialEndPoint)
                    {
                        foundEndPoint = true;
                        break;
                    }
                    if ((potentialEndPoint & opponentBitboard) == potentialEndPoint)
                    {
                        potentialWouldFlips |= potentialEndPoint;
                    }

                    potentialEndPoint = potentialEndPoint.Shift(dir);
                }

                if(foundEndPoint)
                {
                    wouldFlips |= potentialWouldFlips;
                }
            }

            return wouldFlips;
        }

        // ------------------------------------ Board Filters Stuffs ------------------------------------
        /// <summary>
        /// Get all empty-neighbour cells of both players
        /// </summary>
        /// <param name="board"></param>
        /// <returns>Bits that contains all neighbour cells</returns>
        public static ulong EmptyNeighbours(this ReversiBitboard board)
        {
            return board.GetFilled().Delation() & board.GetEmpties();
        }

        // ------------------------------------ Display Stuffs ------------------------------------
        /// <summary>
        /// Chuyển board về dạng chuỗi để có thể hiển thị lên console
        /// </summary>
        /// <param name="bitBoard"></param>
        /// <returns>Chuỗi đã được làm đẹp, chứa các quân cờ của black và white</returns>
        public static string ToDisplayString(this ReversiBitboard bitBoard)
        {
            string white = bitBoard.BitBoard[White].ToBinaryString();
            string black = bitBoard.BitBoard[Black].ToBinaryString();
            string both = BitboardExtensions.CombineBinaryString(white, black);
            return both.Pretty();
        }
    }
}
