using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reversi_mcts.Board;
using Reversi_mcts.Utils;

namespace Reversi_mcts.MachineLearning
{
    public class GameRecordParser
    {
        public int GameCount = 0;
        public int GameMiss = 0;

        public readonly List<List<ulong>> ParsedGamesMoves = new List<List<ulong>>();
        public readonly List<List<List<ulong>>> ParsedGamesLegalMoves = new List<List<List<ulong>>>();

        public void Parse(string fileName)
        {
            var lineIndex = 0;
            var lines = File.ReadLines(fileName);
            var rawLines = lines as string[] ?? lines.ToArray();
            var totalLines = rawLines.Count();
            var progress = new ProgressBar();

            Console.WriteLine($"- Found {totalLines} game records.");

            // đọc từng dòng trong file
            foreach (var rawLine in rawLines)
            {
                lineIndex++;
                var line = rawLine.Trim();

                // parse game line, if there are any error, ignore
                List<ulong> tempMoves;
                List<List<ulong>> tempLegalMoves;
                try
                {
                    ParseGame(line, out tempMoves, out tempLegalMoves);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Format error at line: {lineIndex}. {e.Message}");
                    GameMiss++;
                    continue;
                }

                // Lưu lại parsed-move và parsed-legal-moves vào
                ParsedGamesMoves.Add(tempMoves);
                ParsedGamesLegalMoves.Add(tempLegalMoves);
                GameCount++;

                // update loading bar
                progress.Report((double) lineIndex / totalLines);
            }

            progress.Dispose();
        }

        private static void ParseGame(string strGame, out List<ulong> tempMoves, out List<List<ulong>> tempLegalMoves)
        {
            tempMoves = new List<ulong>();
            tempLegalMoves = new List<List<ulong>>();

            // Tách lấy record text (kifu text)
            var recordText = strGame.Split("|")[2];

            // run game theo game record để lấy move và legal move của từng nước cờ
            // code dưới này gần giống hàm State.FromRecordText
            var state = new State();
            for (var i = 0; i < recordText.Length; i += 2)
            {
                // Kiểm tra xem có phải passing move hay không
                if (!state.Board.HasLegalMoves(state.Player))
                {
                    // lưu lại Passing move
                    tempLegalMoves.Add(new List<ulong>());
                    tempMoves.Add(0UL);

                    // Đổi lượt
                    state.Player = Constant.Opponent(state.Player);
                }

                var notation = recordText.Substring(i, 2).ToLower();
                var bitMove = notation.ToBitMove();
                var listLegalMoves = state.GetListLegalMoves();

                // lưu lại move và legal moves của state này
                tempLegalMoves.Add(listLegalMoves);
                tempMoves.Add(bitMove);

                // tới trạng thái tiếp theo của bàn cờ
                state.NextState(bitMove);
            }
        }

        private static void ParseGGFGame()
        {
        }
    }
}