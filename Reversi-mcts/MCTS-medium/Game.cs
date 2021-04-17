using System.Collections.Generic;

namespace Reversi_mcts.MCTS_medium
{
    class Game
    {
        public Game() { }

        /** Generate and return the initial game state. */
        public State Start()
        {
            return new State(new List<Play>(), new Board(), 1);
        }

        /** Return the current player's legal plays from given state. */
        public List<Play> LegalPlays(State state)
        {
            List<Play> legalPlays = new List<Play>();
            return legalPlays;
        }

        /** Advance the given state and return it. */
        public State NextState(State state, Play play)
        {
            return new State(new List<Play>(), new Board(), 1);
        }

        /** Return the winner of the game. */
        public int Winner(State state)
        {
            return 0;
            // -1, 1, 0, -2
        }
    }
}
