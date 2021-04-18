using System;
using System.Collections.Generic;

namespace Reversi_mcts.MCTS_medium
{
    class ReversiGame
    {
        private ReversiState CurrentState;

        public ReversiGame(ReversiState startState)
        {
            CurrentState = startState;
        }

        /**
		 * Returns true if the game is completed
		 * @return
		 */
        public bool IsGameComplete()
        {
            return CurrentState.IsTerminal();
        }

        public List<ReversiState> PossibleMoves()
        {
            return CurrentState.GetSuccessors();
        }

        /**
		 * Changes the current state of the game, to one of the current states successors.
		 * @param state
		 */
        public void PerformeMove(ReversiState state)
        {
            bool stateChanged = false;
            foreach (ReversiState s in CurrentState.GetSuccessors())
                if (state.Equals(s))
                {
                    CurrentState = s;
                    stateChanged = true;
                    break;
                }
            if (!stateChanged)
                throw new Exception("That state is not reachable from the current state");
        }

        /// <summary>
        /// Returns a string representation of the game
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return CurrentState.ToString();
        }

        /// <summary>
        /// Should only be called if the game is completed
        /// Returns 0 if winner is Black
        /// Returns 1 if winner is White
        /// Returns -1 if game is Draw
        /// </summary>
        /// <returns></returns>
        public byte Winner()
        {
            if (IsGameComplete())
            {
                int blackScore = CurrentState.GetScore(ReversiBitBoard.BLACK);
                int whiteScore = CurrentState.GetScore(ReversiBitBoard.WHITE);
                if (blackScore > whiteScore)
                    return ReversiBitBoard.BLACK;
                if (blackScore < whiteScore)
                    return ReversiBitBoard.WHITE;
                return -1;
            }
            throw new Exception("Asked for the winner before game was completed");
        }
    }
}
