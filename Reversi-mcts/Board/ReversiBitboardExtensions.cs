using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    public static class ReversiBitboardExtensions
    {
        // -------------------------- Cached - For performance boost --------------------------
        public static Direction[] DirectionValues = (Direction[])Enum.GetValues(typeof(Direction));
        public static byte Black = 0;
        public static byte White = 1;

        // ------------------------------------ Basic Stuffs ------------------------------------
        public static ReversiBitboard Clone(this ReversiBitboard board)
        {
            return new ReversiBitboard(board);
        }

        public static bool Equals(this ReversiBitboard board, ReversiBitboard other)
        {
            return board.BitBoard[Black] == other.BitBoard[Black] &&
                board.BitBoard[White] == other.BitBoard[White];
        }

        public static ulong GetEmpties(this ReversiBitboard board)
        {
            return ~board.BitBoard[Black] & ~board.BitBoard[White];
        }

        public static ulong GetFilled(this ReversiBitboard board)
        {
            return board.BitBoard[Black] | board.BitBoard[White];
        }

        public static byte GetScore(this ReversiBitboard board, byte player)
        {
            return board.BitBoard[player].PopCount();
        }

        // ------------------------------------ Move Stuffs ------------------------------------
        public static bool MakeMove(this ReversiBitboard board, byte player, byte row, byte col)
        {
            ulong movePoint = BitboardExtensions.CoordinateToULong(row, col);
            if (!IsLegalMove(board, player, movePoint))
            {
                return false;
            }

            ulong wouldFlips = board.GetWouldFlips(player, movePoint);
            if (wouldFlips == 0)
            {
                return false;
            }

            board.BitBoard[player] |= wouldFlips | movePoint;
            board.BitBoard[1 ^ player] ^= wouldFlips;

            return true;
        }

        public static bool IsGameComplete(this ReversiBitboard board)
        {
            return ~board.GetFilled() == 0 || !board.HasAnyPlayerHasAnyLegalMove();
        }

        public static bool HasLegalMoves(this ReversiBitboard board, byte player)
        {
            return board.GetLegalMoves(player) != 0;
        }

        public static bool HasAnyPlayerHasAnyLegalMove(this ReversiBitboard board)
        {
            return board.HasLegalMoves(Black) || board.HasLegalMoves(White);
        }

        public static bool IsLegalMove(this ReversiBitboard board, byte player, ulong movePoint)
        {
            ulong legalMoves = board.GetLegalMoves(player);
            return (movePoint & legalMoves) != 0;
        }

        public static ulong GetLegalMoves(this ReversiBitboard board, byte player)
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

        public static ulong GetWouldFlips(this ReversiBitboard board, byte player, ulong movePoint)
        {
            // TODO improve this function, make it faster
            ulong playerBitboard = board.BitBoard[player];
            ulong opponentBitboard = board.BitBoard[1 ^ player];
            ulong wouldFlips = 0UL;

            foreach (Direction dir in DirectionValues)
            {
                ulong potentialEndPoint = movePoint.Shift(dir);
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

                if (foundEndPoint)
                {
                    wouldFlips |= potentialWouldFlips;
                }
            }

            return wouldFlips;
        }

        // ------------------------------------ Display Stuffs ------------------------------------
        public static void Draw(this ReversiBitboard board)
        {
            for (var i = 0; i < 64; i++)
            {
                if (i % 8 == 0) Console.WriteLine();

                var pos = 1UL << i;
                bool isBlack = (board.BitBoard[Black] & pos) > 0;
                bool isWhite = (board.BitBoard[White] & pos) > 0;

                if (isBlack && isWhite) Console.Write("X ");
                else if (isBlack) Console.Write("b ");
                else if (isWhite) Console.Write("w ");
                else Console.Write(". ");
            }
            Console.WriteLine();
        }
    }
}
