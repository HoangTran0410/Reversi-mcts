using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    public class State
    {
        public BitBoard Board { get; set; }
        // Player là Người sẽ makeMove tiếp theo (lượt chơi) hay người vừa makeMove
        // => Người sẽ makeMove tiếp theo
        public byte Player { get; set; }
        public byte Opponent { get { return (byte)(1 ^ Player); } }
        public List<State> Successors { get; set; }

        public State(byte player, BitBoard board)
        {
            Player = player;
            Board = board;
        }
    }

    public static class ReversiStateExtensions
    {
        public static List<State> GetSuccessors(this State state)
        {
            if (state.Successors == null)
                state.InitiateSuccessors();
            return state.Successors;
        }

        private static void InitiateSuccessors(this State state)
        {
            state.Successors = new List<State>();
            List<ulong> legalTiles = state.Board.GetLegalMoves(state.Player).ToListUlong();
            List<Move> legalMoves = new List<Move>();

            foreach (ulong bitMove in legalTiles)
            {
                legalMoves.Add(new Move(state.Player, bitMove));
            }

            BitBoard tempBoard = state.Board.Clone();

            foreach (Move m in legalMoves)
            {
                BitBoard successorBoard = tempBoard.MakeMove(m);
                if (successorBoard.HasLegalMoves(state.Opponent))
                {
                    state.Successors.Add(new State(state.Opponent, successorBoard));
                }
                else
                {
                    state.Successors.Add(new State(state.Player, successorBoard));
                }
            }
        }

        public static bool IsTerminal(this State state)
        {
            return state.Board.IsGameComplete();
        }

        public static int GetScore(this State state, byte color)
        {
            return state.Board.GetScore(color);
        }

        public static void Draw(this State state)
        {
            Console.WriteLine("Player to move " + state.Player);
            state.Board.DrawWithLegalMoves(state.Player);
        }

        public static bool Equals(this State state, State other)
        {
            return (state.Board.Equals(other.Board) && state.Player == other.Player);
        }

        public static int Hash(this State state)
        {
            return state.GetHashCode();
        }
    }
}
