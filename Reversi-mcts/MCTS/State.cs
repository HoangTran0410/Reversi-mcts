using System;
using System.Collections.Generic;

namespace Reversi_mcts.MCTS
{
    class State
    {
        static readonly Random RandomGenerator = new Random();

        public Board board { get; set; }
        public int playerNo { get; set; }
        public int visitCount { get; set; }
        public double winScore { get; set; }
        public int opponentNo { get { return 3 - playerNo; } }

        public State()
        {
            board = new Board();
        }

        public State(State state)
        {
            this.board = new Board(state.board);
            this.playerNo = state.playerNo;
            this.visitCount = state.visitCount;
            this.winScore = state.winScore;
        }

        public State(Board board)
        {
            this.board = new Board(board);
        }

        public List<State> GetAllPossibleStates()
        {
            List<State> possibleStates = new List<State>();
            List<Position> availablePositions = this.board.GetAvailablePositions();

            foreach(Position p in availablePositions)
            {
                State newState = new State(this.board);
                newState.playerNo = 3 - this.playerNo;
                newState.board.PerformMove(newState.playerNo, p);
                possibleStates.Add(newState);
            }

            return possibleStates;
        }

        public void IncrementVisit()
        {
            this.visitCount++;
        }

        public void AddScore(double score)
        {
            if (this.winScore != Int32.MinValue)
                this.winScore += score;
        }

        public void RandomPlay()
        {
            List<Position> availablePositions = this.board.GetAvailablePositions();
            int totalPossibilities = availablePositions.Count;
            int selectRandom = (int)(RandomGenerator.NextDouble() * totalPossibilities);
            this.board.PerformMove(this.playerNo, availablePositions[selectRandom]);
        }

        public void TogglePlayer()
        {
            this.playerNo = 3 - this.playerNo;
        }
    }
}
