using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts.MCTS_medium
{
    class ReversiState
    {
        public byte Player { get; }
        public byte Opponent { get { return (byte)(1 - Player); } }
        public ReversiBitBoardOld Board { get; }
        /// <summary>
        /// The calculated value of this state
        /// </summary>
        public short Value { get; }
        /// <summary>
        /// List of legal states that can be reached from this state
        /// </summary>
        private List<ReversiState> successors;


        public ReversiState(byte player, ReversiBitBoardOld board)
        {
            Player = player;
            Board = board;
        }

        public List<ReversiState> GetSuccessors()
        {
            if (successors == null)
                InitiateSuccessors();
            return successors;
        }

        public bool IsTerminal()
        {
            return Board.IsGameComplete();
        }

        public int GetScore(byte color)
        {
            return Board.GetScore(color);
        }

        private void InitiateSuccessors()
        {
            successors = new List<ReversiState>();
            List<Coordinate> legalTiles = Board.GetLegalMoves(Player);
            List<Move> legalMoves = new List<Move>();
            foreach (Coordinate c in legalTiles)
            {
                legalMoves.Add(new Move(Player, c));
            }
            foreach (Move m in legalMoves)
            {
                ReversiBitBoardOld successorBoard = new ReversiBitBoardOld(Board, m);
                if (successorBoard.HasLegalMoves(Opponent))
                {
                    successors.Add(new ReversiState(Opponent, successorBoard));
                }
                else
                {
                    successors.Add(new ReversiState(Player, successorBoard));
                }
            }
        }

        public override string ToString()
        {
            String playerToMoveString = "";
            if (Player == 0)
                playerToMoveString = "BLACK";
            if (Player == 1)
                playerToMoveString = "WHITE";
            return "Player to move " + playerToMoveString + '\n' + Board;
        }

        /// <summary>
        /// Two states are equal if they have equal boards, and the same player to move
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(ReversiState))
            {
                ReversiState s = (ReversiState)obj;
                if (s.Board.Equals(Board) && s.Player == this.Player)
                    return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
