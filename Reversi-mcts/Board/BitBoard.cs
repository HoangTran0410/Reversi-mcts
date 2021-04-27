using System;

namespace Reversi_mcts
{
    public class BitBoard
    {
        // Why use ulong instead of long: https://stackoverflow.com/a/9924991/11898496
        public readonly static ulong
            InitialPositionBlack = 0x810000000, // 00000000 00000000 00000000 00001000 00010000 00000000 00000000 00000000
            InitialPositionWhite = 0x1008000000;// 00000000 00000000 00000000 00010000 00001000 00000000 00000000 00000000

        public ulong[] Pieces { get; }

        public BitBoard()
        {
            Pieces = new ulong[] { InitialPositionBlack, InitialPositionWhite};
        }
        public BitBoard(BitBoard other) {
            Pieces = new ulong[] { other.Pieces[0], other.Pieces[1] };
        }
        public BitBoard(ulong blackPiece, ulong whitePiece)
        {
            Pieces = new ulong[] { blackPiece, whitePiece };
        }
    }

    public static class BitboardExtensions
    {
        // -------------------------- Cached - For performance boost --------------------------
        public static Direction[] DirectionValues = (Direction[])Enum.GetValues(typeof(Direction));
        public static byte Black = 0;
        public static byte White = 1;

        // ------------------------------------ Basic Stuffs ------------------------------------
        public static BitBoard Clone(this BitBoard board)
        {
            return new BitBoard(board);
        }

        public static bool Equals(this BitBoard board, BitBoard other)
        {
            return board.Pieces[Black] == other.Pieces[Black] &&
                board.Pieces[White] == other.Pieces[White];
        }

        public static ulong GetEmpties(this BitBoard board)
        {
            return ~board.Pieces[Black] & ~board.Pieces[White];
        }

        public static ulong GetFilled(this BitBoard board)
        {
            return board.Pieces[Black] | board.Pieces[White];
        }

        public static byte GetScore(this BitBoard board, byte player)
        {
            return board.Pieces[player].PopCount();
        }

        // ------------------------------------ Move Stuffs ------------------------------------
        public static bool MakeMove(this BitBoard board, byte player, byte row, byte col)
        {
            return board.MakeMove(player, BitBoardHelper.CoordinateToULong(row, col));
        }
        public static bool MakeMove(this BitBoard board, byte player, ulong movePoint)
        {
            if (!IsLegalMove(board, player, movePoint))
            {
                return false;
            }

            ulong wouldFlips = board.GetWouldFlips(player, movePoint);
            if (wouldFlips == 0)
            {
                return false;
            }

            board.Pieces[player] |= wouldFlips | movePoint;
            board.Pieces[1 ^ player] ^= wouldFlips;

            return true;
        }

        public static bool IsGameComplete(this BitBoard board)
        {
            return ~board.GetFilled() == 0 || !board.HasAnyPlayerHasAnyLegalMove();
        }

        public static bool HasLegalMoves(this BitBoard board, byte player)
        {
            return board.GetLegalMoves(player) != 0;
        }

        public static bool HasAnyPlayerHasAnyLegalMove(this BitBoard board)
        {
            return board.HasLegalMoves(Black) || board.HasLegalMoves(White);
        }

        public static bool IsLegalMove(this BitBoard board, byte player, ulong movePoint)
        {
            ulong legalMoves = board.GetLegalMoves(player);
            return (movePoint & legalMoves) != 0;
        }

        public static ulong GetLegalMoves(this BitBoard board, byte player)
        {
            // https://intellitect.com/when-to-use-and-not-use-var-in-c/
            // https://stackoverflow.com/a/41505/11898496
            ulong moves = 0UL;
            ulong playerPiece = board.Pieces[player];
            ulong opponentPiece = board.Pieces[1 ^ player];
            ulong empties = board.GetEmpties();

            // https://stackoverflow.com/a/105402/11898496
            foreach (Direction dir in DirectionValues)
            {
                ulong candidates = opponentPiece & playerPiece.Shift(dir);
                while (candidates != 0)
                {
                    ulong candidatesShifted = candidates.Shift(dir);
                    moves |= empties & candidatesShifted;
                    candidates = opponentPiece & candidatesShifted;
                }
            }

            return moves;
        }

        public static ulong GetWouldFlips(this BitBoard board, byte player, ulong movePoint)
        {
            // TODO improve this function, make it faster
            ulong playerPiece = board.Pieces[player];
            ulong opponentPiece = board.Pieces[1 ^ player];
            ulong wouldFlips = 0UL;

            foreach (Direction dir in DirectionValues)
            {
                ulong potentialEndPoint = movePoint.Shift(dir);
                ulong potentialWouldFlips = 0;

                do
                {
                    if ((potentialEndPoint & playerPiece) > 0)
                    {
                        wouldFlips |= potentialWouldFlips;
                        break;
                    }

                    potentialEndPoint &= opponentPiece;
                    potentialWouldFlips |= potentialEndPoint;
                    potentialEndPoint = potentialEndPoint.Shift(dir);
                }
                while (potentialEndPoint > 0);
            }

            return wouldFlips;
        }

        // ------------------------------------ Display Stuffs ------------------------------------
        public static void Draw(this BitBoard board)
        {
            for (var i = 0; i < 64; i++)
            {
                if (i % 8 == 0) Console.WriteLine();

                var pos = 1UL << i;
                bool isBlack = (board.Pieces[Black] & pos) != 0;
                bool isWhite = (board.Pieces[White] & pos) != 0;

                if (isBlack && isWhite) Console.Write("X ");
                else if (isBlack) Console.Write("b ");
                else if (isWhite) Console.Write("w ");
                else Console.Write(". ");
            }
            Console.WriteLine();
        }
    }
}
