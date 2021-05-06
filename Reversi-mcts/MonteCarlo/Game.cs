using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    public class Game
    {
        private State CurrentState;

        public Game(State startState)
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

        public List<State> PossibleMoves()
        {
            return CurrentState.GetSuccessors();
        }

        /**
		 * Changes the current state of the game, to one of the current states successors.
		 * @param state
		 */
        public void PerformeMove(State state)
        {
            bool stateChanged = false;
            foreach (State s in CurrentState.GetSuccessors())
                if (state.Equals(s))
                {
                    CurrentState = s;
                    stateChanged = true;
                    break;
                }
            if (!stateChanged) throw new Exception("That state is not reachable from the current state");
        }

        /// <summary>
        /// Returns a string representation of the game
        /// </summary>
        /// <returns></returns>
        public void Draw()
        {
            CurrentState.Draw();
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
                int blackScore = CurrentState.GetScore(Constant.Black);
                int whiteScore = CurrentState.GetScore(Constant.White);

                if (blackScore > whiteScore) return Constant.Black;
                if (blackScore < whiteScore) return Constant.White;
                return Constant.Draw;
            }
            throw new Exception("Asked for the winner before game was completed");
        }
    }
}
