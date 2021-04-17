using System.Collections.Generic;

namespace Reversi_mcts.MCTS
{
    class Board
    {
        //public enum Status { 
        //    Draw, // 0
        //    P1, // 1
        //    P2, // 2
        //    InProgress // 3
        //}

        public static readonly int InProgress = -1;
        public static readonly int Draw = 0;
        public static readonly int P1 = 1;
        public static readonly int P2 = 2;

        public Board() { }

        public Board(Board board)
        {

        }

        public List<Position> GetAvailablePositions()
        {
            return new List<Position>();
        }

        public void PerformMove(int playerNo, Position position)
        {

        }

        public int CheckStatus()
        {
            return InProgress;
        }
    }
}
