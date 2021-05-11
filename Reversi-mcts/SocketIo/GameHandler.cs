using Reversi_mcts.Core.Board;
using Reversi_mcts.Core.MonteCarlo;

namespace Reversi_mcts.SocketIo
{
    public class GameHandler
    {
        public State State { get; private set; }
        public byte Color { get; private set; }
        public int TimeOut { get; set; }

        public GameHandler(int timeOut = 1000)
        {
            TimeOut = timeOut;
            State = InitialState();
        }

        private static State InitialState()
        {
            return new State(new BitBoard(), Constant.Black);
        }

        public void Restart()
        {
            State = InitialState();
        }

        public void SetColor(byte color)
        {
            Color = color;
        }

        public (int row, int col) PerformAiMove()
        {
            var move = Mcts.RunSearch(State, TimeOut);
            if (move == 0) return (-1, -1);
            State = State.NextState(move);
            return move.ToCoordinate();
        }

        public void MakeMove(int row, int col)
        {
            State = State.NextState(((byte) row, (byte) col).ToBitMove());
        }

        public int GetLastPlayout()
        {
            return Mcts.LastPlayout;
        }
    }
}