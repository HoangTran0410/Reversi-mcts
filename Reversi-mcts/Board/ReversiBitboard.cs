using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{

    public class ReversiBitboard
    {
        // Why use ulong instead of long: https://stackoverflow.com/a/9924991/11898496
        public readonly static ulong
            InitialPositionBlack = 0x810000000, // 00000000 00000000 00000000 00001000 00010000 00000000 00000000 00000000
            InitialPositionWhite = 0x1008000000;// 00000000 00000000 00000000 00010000 00001000 00000000 00000000 00000000

        public ulong[] BitBoard { get; }

        public ReversiBitboard() : this(InitialPositionBlack, InitialPositionWhite) { }
        public ReversiBitboard(ReversiBitboard other) : this(other.BitBoard[0], other.BitBoard[1]) { }
        public ReversiBitboard(ulong blackBitboard, ulong whiteBitboard)
        {
            BitBoard = new ulong[] { blackBitboard, whiteBitboard };
        }
    }
}
