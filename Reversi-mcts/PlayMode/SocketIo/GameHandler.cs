using System;
using Reversi_mcts.Board;
using Reversi_mcts.MonteCarlo;

namespace Reversi_mcts.PlayMode.SocketIo
{
    public class GameHandler
    {
        private State _state;
        private readonly int _timeOut;
        private string _recordText;

        public GameHandler(int timeOut = 1000)
        {
            _timeOut = timeOut;
            _state = new State();
            _recordText = "";
        }

        public void Restart()
        {
            _state = new State();
            _recordText = "";
        }

        public (int row, int col) PerformAiMove(Algorithm algorithm)
        {
            var move = algorithm switch
            {
                Algorithm.Mcts => Mcts.RunSearch(_state, _timeOut),
                Algorithm.Mcts1 => Mcts.RunSearch1(_state, _timeOut),
                _ => 0UL
            };

            _state.NextState(move);

            var notation = move.ToNotation();
            if (move != 0) _recordText += notation;
            Console.WriteLine("{0} - Me: {1} - Win {2}% - Playout {3}",
                _recordText.Length / 2,
                notation,
                Mcts.LastWinPercentage,
                Mcts.LastPlayout);

            return move == 0 ? (-1, -1) : move.ToCoordinate();
        }

        public void MakeMove(int row, int col)
        {
            var bitMove = (row, col).ToBitMove();
            _state.NextState(bitMove);

            var notation = (row, col).ToNotation();
            if (row != -1 && col != -1) _recordText += notation;
            Console.WriteLine("{0} - Op: {1}", _recordText.Length / 2, notation);

            //_state.Board.DisplayWithLastMoveAndLegalMoves(bitMove, Constant.Opponent(_state.Player));
        }

        public static int GetLastPlayout()
        {
            return Mcts.LastPlayout;
        }
    }
}