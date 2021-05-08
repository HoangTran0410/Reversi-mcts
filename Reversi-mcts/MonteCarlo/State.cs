using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    public class State
    {
        public List<ulong> MoveHistory { get; set; }
        public BitBoard Board { get; set; }
        public byte Player { get; set; }
        public byte Opponent { get { return (byte)(1 ^ Player); } }

        public State(List<ulong> moveHistory, BitBoard board, byte player)
        {
            MoveHistory = moveHistory;
            Player = player;
            Board = board;
        }
    }

    public static class ReversiStateExtensions
    {
        public static int Hash(this State state)
        {
            return state.GetHashCode();
        }
    }
}
