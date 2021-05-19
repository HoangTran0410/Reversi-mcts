using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Reversi_mcts.Core.Board;
using Reversi_mcts.Core.MonteCarlo;
using Reversi_mcts.Utils;

namespace Reversi_mcts.GameDatabase
{
    public class GameRecordParser
    {
        public int GameCount = 0;
        public int GameMiss = 0;

        public List<List<ulong>> ParsedGamesMoves = new List<List<ulong>>();
        public List<List<List<ulong>>> ParsedGamesLegalMoves = new List<List<List<ulong>>>();

        public void Parse(string fileName)
        {
            Console.WriteLine("Parse GameRecord from '{0}':", fileName);

            var lineIndex = 0;
            var lines = File.ReadLines(fileName);
            var totalLines = lines.Count();
            var progress = new ProgressBar();
            
            Console.WriteLine("- {0} game records.", totalLines);

            // đọc từng dòng trong file
            foreach (var rawLine in lines)
            {
                lineIndex++;
                var line = rawLine.Trim();

                // parse game line, if there are any error, ignore
                List<ulong> tempParsedMoves;
                List<List<ulong>> tempLegalMoves;
                try
                {
                    ParseGame(line, out tempParsedMoves, out tempLegalMoves);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Format error at line: {0}. {1}", lineIndex, e.Message);
                    GameMiss++;
                    continue;
                }

                // Lưu lại parsed-move và parsed-legal-moves vào
                ParsedGamesMoves.Add(tempParsedMoves);
                ParsedGamesLegalMoves.Add(tempLegalMoves);
                GameCount++;

                // update loading bar
                progress.Report((double) lineIndex / totalLines);
            }

            // sleep để thấy loading bar lên 100% :))
            Thread.Sleep(100);
        }

        private static void ParseGame(string strGame, out List<ulong> tempParsedMoves,
            out List<List<ulong>> tempLegalMoves)
        {
            tempParsedMoves = new List<ulong>();
            tempLegalMoves = new List<List<ulong>>();

            // Tách lấy record text (kifu text)
            var recordText = strGame.Split("|")[2];

            // run game theo game record để lấy move và legal move của từng nước cờ
            // code dưới này gần giống hàm State.FromRecordText
            var state = new State();
            for (var i = 0; i < recordText.Length; i += 2)
            {
                ulong move;
                var listLegalMoves = state.GetListLegalMoves();

                if (listLegalMoves.Count == 0)
                {
                    move = 0UL; // passing move
                }
                else
                {
                    var notation = recordText.Substring(i, 2).ToLower();
                    move = notation.ToBitMove();
                }

                // lưu lại move và legal moves của state này
                tempLegalMoves.Add(listLegalMoves);
                tempParsedMoves.Add(move);

                // tới trạng thái tiếp theo của bàn cờ
                state = state.NextState(move);
            }
        }
    }
}