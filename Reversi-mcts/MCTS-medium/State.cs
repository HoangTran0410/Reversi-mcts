using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts.MCTS_medium
{
    class State
    {
        List<Play> playHistory; // TODO: có thể xóa nếu tìm được cách hash tốt hơn
        Board board;
        int player; // TODO: có thể gộp chung vào bitboard

        public State(List<Play> playHistory, Board board, int player)
        {
            this.playHistory = playHistory;
            this.board = board;
            this.player = player;
        }

        public bool isPlayer(int player)
        {
            return (player == this.player);
        }

        public string hash()
        {
            return this.playHistory.GetHashCode().ToString();
        }

        // Note: If hash uses board, multiple parents possible
    }
}
