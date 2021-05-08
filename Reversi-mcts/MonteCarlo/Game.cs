using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    public class Game
    {
        public Game()
        {
        }

        /** Generate and return the initial game state. */
        public State Start()
        {
            return new State(new List<ulong>(), new BitBoard(), Constant.Black);
        }

        /** Return the current player's legal moves from given state. */
        public List<ulong> LegalMoves(State state)
        {
            return state.Board.GetLegalMoves(state.Player).ToListUlong();
        }

        /** Advance the given state and return it. */
        public State NextState(State state, ulong move)
        {
            // clone list: https://stackoverflow.com/a/222623/11898496
            List<ulong> newHistory = new List<ulong>(state.MoveHistory); 
            newHistory.Add(move);

            BitBoard newBoard = move == 0 ? 
                state.Board.Clone() : // Passing move
                state.Board.MakeMove(state.Player, move); // Make move

            byte newPlayer = state.Opponent;

            //newBoard.DrawWithLastMoveAndLegalMoves(move, newPlayer); // Log

            return new State(newHistory, newBoard, newPlayer);
        }

        /** Return the winner of the game. */
        public byte Winner(State state)
        {
            if (state.Board.IsGameComplete())
            {
                int blackScore = state.Board.GetScore(Constant.Black);
                int whiteScore = state.Board.GetScore(Constant.White);

                if (blackScore > whiteScore) return Constant.Black;
                if (blackScore < whiteScore) return Constant.White;
                return Constant.Draw;
            }
            return Constant.GameNotCompleted;
        }
    }
}
