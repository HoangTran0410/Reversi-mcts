using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    public class State
    {
        public BitBoard Board { get; set; }
        public byte Player { get; set; }
        public byte Opponent { get { return (byte)(1 ^ Player); } }
        public ulong BitLegalMoves { get; set; }

        public State(BitBoard board, byte player)
        {
            Player = player;
            Board = board;
            BitLegalMoves = Board.GetLegalMoves(Player);
        }

        public State(State state)
        {
            Player = state.Player;
            Board = state.Board.Clone();
            BitLegalMoves = state.BitLegalMoves;
        }
    }

    public static class ReversiStateExtensions
    {
        public static bool IsTerminal(this State state)
        {
            return state.Board.IsGameComplete();
        }

        public static State NextState(this State state, ulong move)
        {
            BitBoard newBoard = state.Board.Clone();
            
            // move == 0 => passing move
            if(move != 0) newBoard.MakeMove(state.Player, move);
            
            // switch player
            byte newPlayer = state.Opponent;

            return new State(newBoard, newPlayer);
        }

        public static ulong GetRandomMove(this State state)
        {
            ulong moves = state.BitLegalMoves;
            ulong move = 0;
            
            int movesCount = moves.PopCount();
            int index = Constant.Random.Next(0, movesCount);

            while(index-- >= 0)
            {
                move = moves.HighestOneBit();
                moves ^= move;
            }

            return move;
        }
        
        public static byte Winner(this State state)
        {
            if (state.Board.IsGameComplete())
            {
                int blackScore = state.Board.CountPieces(Constant.Black);
                int whiteScore = state.Board.CountPieces(Constant.White);

                if (blackScore > whiteScore) return Constant.Black;
                if (blackScore < whiteScore) return Constant.White;
                return Constant.Draw;
            }
            return Constant.GameNotCompleted;
        }
    }
}
