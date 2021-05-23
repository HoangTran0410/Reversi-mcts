using System;
using System.Collections.Generic;
using Reversi_mcts.Board;
using Reversi_mcts.MonteCarlo;

namespace Reversi_mcts.PlayMode
{
    public static class HumanVsAi
    {
        public static void NewGame(
            byte humanColor = Constant.Black,
            int aiTimeout = 1000,
            Algorithm aiAlgorithm = Algorithm.Mcts)
        {
            var state = new State();
            var winner = Constant.Draw;

            // show initial board
            state.Board.DisplayWithLegalMoves(Constant.Black);

            // begin
            while (!state.IsTerminal())
            {
                var move = state.Player == humanColor ? HumanTurn(state) : AiTurn(state, aiTimeout, aiAlgorithm);

                state.NextState(move);
                winner = state.Winner();

                DrawBoard(move, state);
            }

            if (winner == Constant.Draw) Console.WriteLine("Draw");
            else Console.WriteLine((winner == humanColor ? "YOU" : "AI") + " won!");
        }

        private static void DrawBoard(ulong move, State state)
        {
            state.Board.DisplayWithLastMoveAndLegalMoves(move, state.Player);

            var blackScore = state.Board.CountPieces(Constant.Black);
            var whiteScore = state.Board.CountPieces(Constant.White);
            Console.WriteLine("- Score(b/w): {0}/{1}\n", blackScore, whiteScore);
        }

        private static ulong AiTurn(State state, int aiTimeout, Algorithm aiAlgorithm)
        {
            var move = Mcts.RunSearch(aiAlgorithm, state, aiTimeout);

            Console.Write("Ai move: " + move.ToNotation());
            Console.WriteLine(" - Runtime: {0}ms, Playout: {1}, wins: {2}%",
                Mcts.LastRunTime,
                Mcts.LastPlayout,
                Mcts.LastWinPercentage);

            return move;
        }

        private static ulong HumanTurn(State state)
        {
            // show valid moves
            var validMoves = state.GetListLegalMoves();
            var validNotations = new List<string>(validMoves.Count);
            Console.Write("Valid moves: ");
            foreach (var validMove in validMoves)
            {
                var notation = validMove.ToNotation();
                validNotations.Add(notation);
                Console.Write(notation + ", ");
            }

            // wait for valid move
            while (true)
            {
                Console.Write("\nYour move (notation): ");
                var inputNotation = Console.ReadLine();

                // check valid move
                if (validNotations.IndexOf(inputNotation) != -1)
                {
                    return inputNotation.ToBitMove();
                }

                Console.WriteLine("- Invalid move. Please make another move.");
            }
        }
    }
}