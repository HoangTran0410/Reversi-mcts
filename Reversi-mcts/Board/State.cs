using System.Collections.Generic;

namespace Reversi_mcts.Board
{
    public class State
    {
        public BitBoard Board { get; }
        public byte Player { get; set; }
        public ulong BitLegalMoves { get; }

        public State() : this(new BitBoard(), Constant.Black)
        {
        }

        public State(BitBoard board, byte player)
        {
            Player = player;
            Board = board;
            BitLegalMoves = board.GetLegalMoves(player);
        }

        public static State FromRecordText(string recordText)
        {
            var state = new State();
            for (var i = 0; i < recordText.Length; i += 2)
            {
                // Check passing move
                if (!state.Board.HasLegalMoves(state.Player))
                {
                    // swap player
                    state.Player = Constant.Opponent(state.Player);
                }
                
                var notation = recordText.Substring(i, 2).ToLower();
                state = state.NextState(notation.ToBitMove());
            }
            return state;
        }
    }

    public static class StateExtensions
    {
        public static bool IsTerminal(this State state)
        {
            return state.Board.IsGameComplete();
        }

        public static State NextState(this State state, ulong move)
        {
            var newBoard = state.Board.Clone();

            // move == 0 => passing move
            if (move != 0) newBoard.MakeMove(state.Player, move);

            // switch player
            var newPlayer = Constant.Opponent(state.Player);

            return new State(newBoard, newPlayer);
        }

        public static List<ulong> GetListLegalMoves(this State state)
        {
            return state.BitLegalMoves.ToListBitMove();
        }

        public static ulong[] GetArrayLegalMoves(this State state)
        {
            return state.BitLegalMoves.ToArrayBitMove();
        }

        public static ulong GetRandomMove(this State state)
        {
            var moves = state.BitLegalMoves;
            ulong move = 0;

            int movesCount = moves.PopCount();
            var index = Constant.Random.Next(0, movesCount);

            while (index-- >= 0)
            {
                move = moves.HighestOneBit();
                moves ^= move;
            }

            return move;
        }

        public static byte Winner(this State state)
        {
            int blackScore = state.Board.CountPieces(Constant.Black);
            int whiteScore = state.Board.CountPieces(Constant.White);

            if (blackScore > whiteScore) return Constant.Black;
            if (blackScore < whiteScore) return Constant.White;
            return Constant.Draw;
        }
    }
}