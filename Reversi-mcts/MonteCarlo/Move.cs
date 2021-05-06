using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    public class Move
    {
        public byte Player { get; }
        public ulong BitMove { get; set; }

        public Move(byte player, ulong bitMove)
        {
            Player = player;
            BitMove = bitMove;
        }

        public Move(byte player, byte row, byte col)
        {
            Player = player;
            BitMove = 0UL.SetBitAtCoordinate(row, col);
        }

        public int Hash()
        {
            return GetHashCode();
        }
    }
}
