using System.Collections.Generic;

namespace Reversi_mcts.Board
{
    // Class lưu trạng thái bàn cờ
    public class State
    {
        public BitBoard Board { get; } // Bàn cờ hiện tại
        public byte Player { get; set; } // Lươt chơi hiện tại. Người tiếp theo sẽ được đánh cờ.
        public ulong BitLegalMoves { get; set; } // Lưu mọi legalmoves dưới dạng bit bằng 1 biến ulong

        // Mặc định người chơi Black sẽ đánh trước
        public State() : this(new BitBoard(), Constant.Black)
        {
        }

        public State(BitBoard board, byte player)
        {
            Player = player;
            Board = board;
            BitLegalMoves = board.GetLegalMoves(player);
        }

        public State(BitBoard board, byte player, ulong bitLegalMoves)
        {
            Player = player;
            Board = board;
            BitLegalMoves = bitLegalMoves;
        }

        // Tự đánh cờ theo game record, và trả về trạng thái bàn cờ cuối cùng trong ván game đó.
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
                state.NextState(notation.ToBitMove());
            }

            return state;
        }
    }

    public static class StateExtensions
    {
        public static State Clone(this State state)
        {
            return new State(state.Board.Clone(), state.Player, state.BitLegalMoves);
        }

        public static bool IsTerminal(this State state)
        {
            return state.Board.IsGameComplete();
        }

        // Đánh cờ tại vị trí move. Đánh trực tiếp vào bàn cờ, ko tạo State mới.
        public static State NextState(this State state, ulong move)
        {
            // move == 0 => passing move
            if (move != 0) state.Board.MakeMove(state.Player, move);

            // switch player
            state.Player = Constant.Opponent(state.Player);

            // reCalculate legalmoves
            state.BitLegalMoves = state.Board.GetLegalMoves(state.Player);

            return state;
        }

        public static List<ulong> GetListLegalMoves(this State state)
        {
            return state.BitLegalMoves.ToListBitMove();
        }

        // Định dạng ulong[] ít tốn Ram hơn List<ulong>
        public static ulong[] GetArrayLegalMoves(this State state)
        {
            return state.BitLegalMoves.ToArrayBitMove();
        }

        // Optimized, Working with bit. FASTER THAN get random element from List<ulong> bitMoves.
        // Use for mcts Simulation Phase
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