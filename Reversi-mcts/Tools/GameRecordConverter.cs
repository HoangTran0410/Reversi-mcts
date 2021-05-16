using System;
using System.Text;
using Reversi_mcts.Core.Board;
using Reversi_mcts.Core.MonteCarlo;

namespace Reversi_mcts.Tools
{
    public static class GameRecordConverter
    {
        private const string GameRecord64Header =
            "Black,White,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,Moves";

        private const string Black = "b";
        private const string White = "w";
        private const string Empty = "_";
        private const string LegalMove = "l";
        private const string NextMove = "n";
        private const string PlayerSeparator = ".";

        public static string _gameRecord = "0|38|F5F6E6F4G5G6G4E7F7F8H6H4H5H7E8D8H3H2G3F2F3E2F1E3D2D1C5C4E1G1D3C2C3C6";
        public static string _simpleBoard = "w.___________________________wb______bbb__________________________";
        
        public static void ToGameRecord64(string gameRecord)
        {
            var gameRecord64 = new StringBuilder();

            var split = gameRecord.Split("|");
            var score = split[0] + "," + split[1] + ",";
            var kifuText = split[2];

            // score
            gameRecord64.Append(score);

            var state = new State();
            for (var i = 0; i < 60; i ++)
            {
                if (i < kifuText.Length - 2)
                {
                    var nextMove = kifuText.Substring(i * 2, 2).ToLower();
                    state = state.NextState(nextMove.ToBitMove());

                    // simple board
                    var simpleBoard = ToSimpleBoard(state, nextMove);
                    gameRecord64.Append(simpleBoard);

                    // PASSING MOVE
                    if (!state.Board.HasLegalMoves(state.Player))
                    {
                        state = state.NextState(0);
                    }
                }
                else
                {
                    gameRecord64.Append(",");
                }
            }
            
            Console.WriteLine(gameRecord64.ToString());
        }

        public static string ToSimpleBoard(State state, string nextMove)
        {
            var simpleBoard = new StringBuilder();
            
            var board = state.Board;
            var playerMakeMove = state.Player;
            var nextMoveIndex = nextMove.ToBitMove().BitScanForward();
            var legalMoves = board.GetLegalMoves(playerMakeMove);

            // player
            simpleBoard.Append(state.Player == Constant.Black ? Black : White);
            simpleBoard.Append(PlayerSeparator);
            
            // board values
            for (var i = 0; i < 64; i++)
            {
                var pos = 1UL << i;
                var isBlack = (board.Pieces[Constant.Black] & pos) != 0;
                var isWhite = (board.Pieces[Constant.White] & pos) != 0;

                if (isBlack) simpleBoard.Append(Black);
                else if (isWhite) simpleBoard.Append(White);
                else if ((legalMoves & pos) != 0) simpleBoard.Append(LegalMove);
                else simpleBoard.Append(Empty);
            }
            
            return simpleBoard.ToString();
        }

        // https://stackoverflow.com/a/9367179/11898496
        public static string ReplaceAt(this string str, int index, char replacer)
        {
            var sb = new StringBuilder(str);
            sb[index] = replacer;
            return sb.ToString();
        }
    }
}