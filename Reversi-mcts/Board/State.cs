using System.Collections.Generic;

namespace Reversi_mcts.Board
{
    // Class lưu trạng thái bàn cờ
    public class State
    {
        public BitBoard Board { get; } // Bàn cờ hiện tại
        public byte Player { get; set; } // Lươt chơi hiện tại. Người sẽ đánh nước cờ tiếp theo.
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
                var notation = recordText.Substring(i, 2).ToLower();
                state.NextState(notation.ToBitMove());
                if(!state.HasLegalMoves()) state.SwapPlayer();
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

        public static bool IsEquals(this State state, State other)
        {
            return state.Player == other.Player && state.Board.IsEquals(other.Board);
        }

        /// <summary>
        /// Check if the game is terminal or not.
        /// <para>State is Terminal if No one have any legal move OR The board is full.</para>
        /// </summary>
        public static bool IsTerminal(this State state)
        {
            return state.Board.IsGameComplete();
        }

        /// <summary>
        /// Check if current player of the State have any legal move.
        /// </summary>
        public static bool HasLegalMoves(this State state)
        {
            return state.BitLegalMoves != 0;
        }
        
        /// <summary>
        /// Đánh cờ tại vị trí move. Đánh trực tiếp vào bàn cờ, ko tạo State mới. 
        /// </summary>
        public static State NextState(this State state, ulong move)
        {
            // if move == 0 => passing move
            if (move != 0) state.Board.MakeMove(state.Player, move);
            state.SwapPlayer();
            return state;
        }

        /// <summary>
        /// Swap current player with current opponent
        /// </summary>
        public static void SwapPlayer(this State state)
        {
            state.SetPlayer(Constant.Opponent(state.Player));
        }

        /// <summary>
        /// Set current player for the State
        /// </summary>
        public static void SetPlayer(this State state, byte player)
        {
            state.Player = player;
            state.BitLegalMoves = state.Board.GetLegalMoves(state.Player);
        }

        /// <summary>
        /// Get List of legal moves (List ulong)
        /// </summary>
        public static List<ulong> GetListLegalMoves(this State state)
        {
            return state.BitLegalMoves.ToListBitMove();
        }
        
        /// <summary>
        /// Get Array of legal moves (ulong[])
        /// <para>Định dạng ulong[] ít tốn Ram hơn List ulong</para>
        /// </summary>
        public static ulong[] GetArrayLegalMoves(this State state)
        {
            return state.BitLegalMoves.ToArrayBitMove();
        }
        
        /// <summary>
        /// Get random move from legal moves
        /// <para>Optimized, Working with bit. FASTER THAN get random element from List<ulong> bitMove</para>
        /// <para>Use for MCTS Simulation Phase</para>
        /// </summary>
        /// <returns>A bit move (ulong) represent the random move selected</returns>
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

        /// <summary>
        /// Get winner of the state
        /// </summary>
        /// <param name="state"></param>
        /// <returns>Winner of the state (Constant.Black, Constant.White or Constant.Draw)</returns>
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