using System;

namespace Reversi_mcts.Board
{
    public class BitBoard
    {
        // Why use ulong instead of long: https://stackoverflow.com/a/9924991/11898496
        public ulong[] Pieces { get; }

        public BitBoard()
        {
            Pieces = new ulong[] {0x810000000, 0x1008000000};
        }

        public BitBoard(BitBoard board)
        {
            Pieces = new[] {board.Pieces[0], board.Pieces[1]};
        }
    }

    public static class BitboardExtensions
    {
        // -------------------------- Cached - For performance boost --------------------------
        private static readonly Direction[] DirectionValues = (Direction[]) Enum.GetValues(typeof(Direction));
        private const byte Black = Constant.Black;
        private const byte White = Constant.White;

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

        public static byte CountPieces(this BitBoard board, byte player)
        {
            return board.Pieces[player].PopCount();
        }

        // ------------------------------------ Move Stuffs ------------------------------------
        public static void MakeMove(this BitBoard board, byte player, byte row, byte col)
        {
            var bitMove = BitBoardHelper.CoordinateToULong(row, col);
            board.MakeMove(player, bitMove);
        }

        public static void MakeMove(this BitBoard board, byte player, ulong bitMove)
        {
            var wouldFlips = board.GetWouldFlips(player, bitMove);
            board.Pieces[player] |= wouldFlips | bitMove;
            board.Pieces[1 ^ player] ^= wouldFlips;
        }

        public static bool IsGameComplete(this BitBoard board)
        {
            return board.GetEmpties() == 0 || !board.HasAnyPlayerHasAnyLegalMove();
        }

        public static bool HasLegalMoves(this BitBoard board, byte player)
        {
            return board.GetLegalMoves(player) != 0;
        }

        public static bool HasAnyPlayerHasAnyLegalMove(this BitBoard board)
        {
            return board.HasLegalMoves(Black) || board.HasLegalMoves(White);
        }

        public static bool IsLegalMove(this BitBoard board, byte player, ulong bitMove)
        {
            var legalMoves = board.GetLegalMoves(player);
            return (bitMove & legalMoves) != 0;
        }

        public static ulong GetLegalMoves(this BitBoard board, byte player)
        {
            // https://intellitect.com/when-to-use-and-not-use-var-in-c/
            // https://stackoverflow.com/a/41505/11898496
            var moves = 0UL;
            var playerPiece = board.Pieces[player];
            var opponentPiece = board.Pieces[1 ^ player];
            var empties = board.GetEmpties();

            // https://stackoverflow.com/a/105402/11898496
            foreach (var dir in DirectionValues)
            {
                var candidates = opponentPiece & playerPiece.Shift(dir);
                while (candidates != 0)
                {
                    var candidatesShifted = candidates.Shift(dir);
                    moves |= empties & candidatesShifted;
                    candidates = opponentPiece & candidatesShifted;
                }
            }

            return moves;
        }

        // https://github.com/ledpup/Othello/blob/cf3f21ebe2550393cf9300d01f02182184364b91/Othello.Model/Play.cs#L61
        public static ulong GetWouldFlips(this BitBoard board, byte player, ulong bitMove)
        {
            // TODO improve this function, make it faster
            var playerPiece = board.Pieces[player];
            var opponentPiece = board.Pieces[1 ^ player];
            var wouldFlips = 0UL;

            foreach (var dir in DirectionValues)
            {
                var potentialEndPoint = bitMove.Shift(dir);
                var potentialWouldFlips = 0UL;

                do
                {
                    if ((potentialEndPoint & playerPiece) != 0)
                    {
                        wouldFlips |= potentialWouldFlips;
                        break;
                    }

                    potentialEndPoint &= opponentPiece;
                    potentialWouldFlips |= potentialEndPoint;
                    potentialEndPoint = potentialEndPoint.Shift(dir);
                } while (potentialEndPoint != 0);
            }

            return wouldFlips;
        }

        // ------------------------------------ Display Stuffs ------------------------------------
        public static void Draw(this BitBoard board)
        {
            for (var i = 0; i < 64; i++)
            {
                if (i % 8 == 0 && i != 0) Console.WriteLine();

                var pos = 1UL << i;
                var isBlack = (board.Pieces[Black] & pos) != 0;
                var isWhite = (board.Pieces[White] & pos) != 0;

                if (isBlack && isWhite) Console.Write("X ");
                else if (isBlack) Console.Write("b ");
                else if (isWhite) Console.Write("w ");
                else Console.Write(". ");
            }

            Console.WriteLine();
        }

        public static void DrawWithLastMove(this BitBoard board, ulong lastBitMove)
        {
            var moveIndex = lastBitMove.BitScanReverse();

            for (var i = 0; i < 64; i++)
            {
                if (i % 8 == 0 && i != 0) Console.WriteLine();

                var pos = 1UL << i;
                var isBlack = (board.Pieces[Black] & pos) != 0;
                var isWhite = (board.Pieces[White] & pos) != 0;

                if (isBlack && isWhite) Console.Write("X ");
                else if (isBlack) Console.Write(moveIndex == i ? "B " : "b ");
                else if (isWhite) Console.Write(moveIndex == i ? "W " : "w ");
                else Console.Write(". ");
            }

            Console.WriteLine();
        }

        public static void DrawWithLegalMoves(this BitBoard board, byte player)
        {
            var legalMoves = board.GetLegalMoves(player);

            for (var i = 0; i < 64; i++)
            {
                if (i % 8 == 0 && i != 0) Console.WriteLine();

                var pos = 1UL << i;
                var isBlack = (board.Pieces[Black] & pos) != 0;
                var isWhite = (board.Pieces[White] & pos) != 0;

                if (isBlack && isWhite) Console.Write("X ");
                else if (isBlack) Console.Write("b ");
                else if (isWhite) Console.Write("w ");
                else if ((legalMoves & pos) != 0) Console.Write("_ ");
                else Console.Write(". ");
            }

            Console.WriteLine();
        }

        public static void DrawWithLastMoveAndLegalMoves(this BitBoard board, ulong lastBitMove, byte player)
        {
            var legalMoves = board.GetLegalMoves(player);
            var moveIndex = lastBitMove.BitScanReverse();

            for (var i = 0; i < 64; i++)
            {
                if (i % 8 == 0 && i != 0) Console.WriteLine();

                var pos = 1UL << i;
                var isBlack = (board.Pieces[Black] & pos) != 0;
                var isWhite = (board.Pieces[White] & pos) != 0;

                if (isBlack && isWhite) Console.Write("X ");
                else if (isBlack) Console.Write(moveIndex == i ? "B " : "b ");
                else if (isWhite) Console.Write(moveIndex == i ? "W " : "w ");
                else if ((legalMoves & pos) != 0) Console.Write("_ ");
                else Console.Write(". ");
            }

            Console.WriteLine();
        }
    }
}