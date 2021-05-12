using System;
using Reversi_mcts.Core.Board;
using Reversi_mcts.Core.MonteCarlo;

namespace Reversi_mcts.PlayMode.SocketIo
{
    public class GameHandler
    {
        private State State { get; set; }
        private int TimeOut { get; }
        private string KifuText;

        public GameHandler(int timeOut = 1000)
        {
            TimeOut = timeOut;
            State = new State();
            KifuText = "";
        }

        public void Restart()
        {
            State = new State();
            KifuText = "";
        }

        public (int row, int col) PerformAiMove()
        {
            var move = Mcts.RunSearch(State, TimeOut);
            State = State.NextState(move);

            var notation = move.ToNotation();
            if(move != 0) KifuText += notation;
            Console.WriteLine("{0} - Me: {1} - Win {2}% - Playout {3}", 
                KifuText.Length / 2, 
                notation, 
                Mcts.LastWinPercentage, 
                Mcts.LastPlayout);

            return move == 0 ? (-1, -1) : move.ToCoordinate();
        }

        public void MakeMove(int row, int col)
        {
            var coordinate = (row, col);
            State = State.NextState(coordinate.ToBitMove());

            var notation = coordinate.ToNotation();
            if(row != -1 && col != -1) KifuText += notation;
            Console.WriteLine("{0} - Op: {1}", KifuText.Length / 2, notation);
        }

        public int GetLastPlayout()
        {
            return Mcts.LastPlayout;
        }
    }
}