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

        public static byte GetPieceAt(this BitBoard board, ulong position)
        {
            if ((board.Pieces[Black] & position) != 0) return Constant.BlackCell;
            if ((board.Pieces[White] & position) != 0) return Constant.WhiteCell;
            return Constant.EmptyCell;
        }

        public static void SetPieceAt(this BitBoard board, ulong position, byte color)
        {
            board.Pieces[color] |= position;
        }

        public static void Clear(this BitBoard board)
        {
            board.Pieces[Black] = 0;
            board.Pieces[White] = 0;
        }

        // ------------------------------------ Move Stuffs ------------------------------------
        public static void MakeMove(this BitBoard board, byte player, ulong bitMove)
        {
            var wouldFlips = board.GetWouldFlips(player, bitMove);
            board.Pieces[player] |= wouldFlips | bitMove;
            board.Pieces[Constant.Opponent(player)] ^= wouldFlips;
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
            var opponentPiece = board.Pieces[Constant.Opponent(player)];
            var empties = board.GetEmpties();

            // https://stackoverflow.com/a/105402/11898496
            foreach (var direction in DirectionValues)
            {
                var candidates = opponentPiece & playerPiece.Shift(direction);
                while (candidates != 0)
                {
                    var candidatesShifted = candidates.Shift(direction);
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
            var opponentPiece = board.Pieces[Constant.Opponent(player)];
            var wouldFlips = 0UL;

            foreach (var direction in DirectionValues)
            {
                var potentialEndPoint = bitMove.Shift(direction);
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
                    potentialEndPoint = potentialEndPoint.Shift(direction);
                } while (potentialEndPoint != 0);
            }

            return wouldFlips;
        }

        // ------------------------------------ Flip Rotate ------------------------------------
        public static BitBoard Rotate180(this BitBoard board)
        {
            var clone = board.Clone();
            clone.Pieces[Black] = clone.Pieces[Black].Rotate180();
            clone.Pieces[White] = clone.Pieces[White].Rotate180();
            return clone;
        }

        public static BitBoard Rotate90Clockwise(this BitBoard board)
        {
            var clone = board.Clone();
            clone.Pieces[Black] = clone.Pieces[Black].Rotate90Clockwise();
            clone.Pieces[White] = clone.Pieces[White].Rotate90Clockwise();
            return clone;
        }

        public static BitBoard Rotate90AntiClockwise(this BitBoard board)
        {
            var clone = board.Clone();
            clone.Pieces[Black] = clone.Pieces[Black].Rotate90AntiClockwise();
            clone.Pieces[White] = clone.Pieces[White].Rotate90AntiClockwise();
            return clone;
        }

        public static BitBoard FlipVertical(this BitBoard board)
        {
            var clone = board.Clone();
            clone.Pieces[Black] = clone.Pieces[Black].FlipVertical();
            clone.Pieces[White] = clone.Pieces[White].FlipVertical();
            return clone;
        }

        public static BitBoard MirrorHorizontal(this BitBoard board)
        {
            var clone = board.Clone();
            clone.Pieces[Black] = clone.Pieces[Black].MirrorHorizontal();
            clone.Pieces[White] = clone.Pieces[White].MirrorHorizontal();
            return clone;
        }

        public static BitBoard FlipDiagA8H1(this BitBoard board)
        {
            var clone = board.Clone();
            clone.Pieces[Black] = clone.Pieces[Black].FlipDiagA8H1();
            clone.Pieces[White] = clone.Pieces[White].FlipDiagA8H1();
            return clone;
        }

        public static BitBoard FlipDiagA1H8(this BitBoard board)
        {
            var clone = board.Clone();
            clone.Pieces[Black] = clone.Pieces[Black].FlipDiagA1H8();
            clone.Pieces[White] = clone.Pieces[White].FlipDiagA1H8();
            return clone;
        }

        // ------------------------------------ Display Stuffs ------------------------------------
        public static void Display(this BitBoard board)
        {
            Console.WriteLine("  a b c d e f g h");

            var row = 0;
            for (var i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                {
                    row++;
                    if (i != 0) Console.WriteLine();
                    Console.Write(row + " ");
                }

                var pos = 1UL << i;
                var isBlack = (board.Pieces[Black] & pos) != 0;
                var isWhite = (board.Pieces[White] & pos) != 0;

                if (isBlack && isWhite) Console.Write(Constant.WrongCellStr);
                else if (isBlack) Console.Write(Constant.BlackPieceStr);
                else if (isWhite) Console.Write(Constant.WhitePieceStr);
                else Console.Write(Constant.EmptyCellStr);
            }

            Console.WriteLine();
        }

        public static void DisplayWithLastMove(this BitBoard board, ulong lastBitMove)
        {
            Console.WriteLine("  a b c d e f g h");

            var moveIndex = lastBitMove.BitScanReverse();
            var row = 0;
            for (var i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                {
                    row++;
                    if (i != 0) Console.WriteLine();
                    Console.Write(row + " ");
                }

                var pos = 1UL << i;
                var isBlack = (board.Pieces[Black] & pos) != 0;
                var isWhite = (board.Pieces[White] & pos) != 0;

                if (isBlack && isWhite) Console.Write(Constant.WrongCellStr);
                else if (isBlack) Console.Write(moveIndex == i ? Constant.LastBlackMoveStr : Constant.BlackPieceStr);
                else if (isWhite) Console.Write(moveIndex == i ? Constant.LastWhiteMoveStr : Constant.WhitePieceStr);
                else Console.Write(Constant.EmptyCellStr);
            }

            Console.WriteLine();
        }

        public static void DisplayWithLegalMoves(this BitBoard board, byte player)
        {
            Console.WriteLine("  a b c d e f g h");

            var legalMoves = board.GetLegalMoves(player);
            var row = 0;
            for (var i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                {
                    row++;
                    if (i != 0) Console.WriteLine();
                    Console.Write(row + " ");
                }

                var pos = 1UL << i;
                var isBlack = (board.Pieces[Black] & pos) != 0;
                var isWhite = (board.Pieces[White] & pos) != 0;

                if (isBlack && isWhite) Console.Write(Constant.WrongCellStr);
                else if (isBlack) Console.Write(Constant.BlackPieceStr);
                else if (isWhite) Console.Write(Constant.WhitePieceStr);
                else if ((legalMoves & pos) != 0) Console.Write(Constant.LegalMoveStr);
                else Console.Write(Constant.EmptyCellStr);
            }

            Console.WriteLine();
        }

        public static void DisplayWithLastMoveAndLegalMoves(this BitBoard board, ulong lastBitMove, byte player)
        {
            Console.WriteLine("  a b c d e f g h");

            var legalMoves = board.GetLegalMoves(player);
            var moveIndex = lastBitMove.BitScanReverse();
            var row = 0;
            for (var i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                {
                    row++;
                    if (i != 0) Console.WriteLine();
                    Console.Write(row + " ");
                }

                var pos = 1UL << i;
                var isBlack = (board.Pieces[Black] & pos) != 0;
                var isWhite = (board.Pieces[White] & pos) != 0;

                if (isBlack && isWhite) Console.Write(Constant.WrongCellStr);
                else if (isBlack) Console.Write(moveIndex == i ? Constant.LastBlackMoveStr : Constant.BlackPieceStr);
                else if (isWhite) Console.Write(moveIndex == i ? Constant.LastWhiteMoveStr : Constant.WhitePieceStr);
                else if ((legalMoves & pos) != 0) Console.Write(Constant.LegalMoveStr);
                else Console.Write(Constant.EmptyCellStr);
            }

            Console.WriteLine();
        }
    }
}