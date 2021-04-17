using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts.MCTS_medium
{
    // TODO lưu dạng bit hoặc kifu(a1 b3 ...) để tiết kiệm bộ nhớ
    class Play
    {
        int row, col;

        public Play(int row, int col)
        {
            this.row = row;
            this.col = col;
        }

        public string hash()
        {
            return this.row + "," + this.col;
        }
    }
}
