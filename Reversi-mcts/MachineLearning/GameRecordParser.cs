using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Reversi_mcts.Board;
using Reversi_mcts.Utils;

namespace Reversi_mcts.MachineLearning
{
    public class GameRecordParser
    {
        public int GameCount = 0;
        public int GameMiss = 0;

        public Dictionary<int, State> ParsedStates = new Dictionary<int, State>();
        public List<List<ulong>> ParsedMoves = new List<List<ulong>>();
        public List<List<List<ulong>>> ParsedLegalMoves = new List<List<List<ulong>>>();

        public void SaveParsedGame(string filePathToSave)
        {
            Console.WriteLine($"> Calculating score for {ParsedMoves.Count} games...");
            var progress = new ProgressBar();
            var lines = new StringBuilder();

            for (var iState = 0; iState < ParsedMoves.Count; iState++)
            {
                // // if moves too short
                // var moveCount = ParsedMoves[iState].Count;
                // if (moveCount < 10)
                // {
                //     Console.WriteLine($"Game {iState} is too short ({moveCount} moves). Pass.");
                //     continue;
                // }

                // get record text
                var recordText = "";
                foreach (var move in ParsedMoves[iState])
                    if (move != 0)
                        recordText += move.ToNotation();

                // calculate score
                var finalState = State.FromRecordText(recordText);
                var blackScore = finalState.Board.CountPieces(Constant.Black);
                var whiteScore = finalState.Board.CountPieces(Constant.White);
                var line = $"{blackScore}|{whiteScore}|{recordText}";

                // Lưu dữ liệu state vào cuối nếu state này không giống state mặc định
                if (ParsedStates.TryGetValue(iState, out var parsedState))
                {
                    var blackPiece = parsedState.Board.Pieces[Constant.Black];
                    var whitePiece = parsedState.Board.Pieces[Constant.White];
                    var player = parsedState.Player;
                    line += $"|{blackPiece}|{whitePiece}|{player}";
                }

                lines.AppendLine(line);
                progress.Report(iState * 1.0 / ParsedMoves.Count);
            }

            progress.Dispose();
            Console.WriteLine("> Saving to file...");
            File.WriteAllText(filePathToSave, lines.ToString());
            Console.WriteLine("> Saved");
        }

        public void Parse(string filePath)
        {
            Console.WriteLine("> Reading...");

            var isGgfFormat = FileUtils.CheckFileExtension(filePath, ".ggf");
            Console.WriteLine(isGgfFormat
                ? "> Found file extension '.ggf'. Using GGF Parser."
                : "> Using game record (kifu) Parser.");

            var lineIndex = 0;
            var lines = File.ReadLines(filePath);
            var rawLines = lines as string[] ?? lines.ToArray();
            var totalLines = rawLines.Count();
            var progress = new ProgressBar();
            var defaultState = new State();

            Console.WriteLine($"- Found {totalLines} game records.");

            // đọc từng dòng trong file
            foreach (var rawLine in rawLines)
            {
                lineIndex++;
                var line = rawLine.Trim();

                // parse game line, if there are any error, ignore
                State state;
                List<ulong> moves;
                List<List<ulong>> legalMoves;
                try
                {
                    if (isGgfFormat) ParseGgfGame(line, out state, out moves, out legalMoves);
                    else ParseGame(line, out state, out moves, out legalMoves);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Format error at line: {lineIndex} - {e.Message}");
                    GameMiss++;
                    continue;
                }

                // Lưu lại parsed-move và parsed-legal-moves vào
                if (!state.IsEquals(defaultState)) ParsedStates.Add(lineIndex - 1, state);
                ParsedMoves.Add(moves);
                ParsedLegalMoves.Add(legalMoves);
                GameCount++;

                // update loading bar
                progress.Report((double) lineIndex / totalLines);
            }

            progress.Dispose();
        }

        private static void ParseGame(
            string strGame,
            out State state,
            out List<ulong> moves,
            out List<List<ulong>> legalMoves)
        {
            var split = strGame.Split('|');

            // Tách dữ liệu state (nếu có)
            if (split.Length > 3)
            {
                var blackOke = ulong.TryParse(split[3], out var blackPiece);
                var whiteOke = ulong.TryParse(split[4], out var whitePiece);
                var playerOke = byte.TryParse(split[5], out var player);
                if (!blackOke || !whiteOke || !playerOke)
                    throw new Exception("Invalid board value.");

                state = new State(new BitBoard(blackPiece, whitePiece), player);
            }
            else state = new State();

            // Tách lấy record text (kifu text)
            var recordText = split[2];

            // run game theo game record để lấy move và legal move của từng nước cờ
            // code dưới này gần giống hàm State.FromRecordText
            moves = new List<ulong>();
            legalMoves = new List<List<ulong>>();

            var stateClone = state.Clone();
            for (var i = 0; i < recordText.Length; i += 2)
            {
                // Kiểm tra xem có phải passing move hay không
                if (!stateClone.Board.HasLegalMoves(stateClone.Player))
                {
                    // lưu lại Passing move
                    legalMoves.Add(new List<ulong>());
                    moves.Add(0UL);

                    // Đổi lượt
                    stateClone.SwapPlayer();
                }

                var notation = recordText.Substring(i, 2).ToLower();
                var bitMove = notation.ToBitMove();
                var listLegalMoves = stateClone.GetListLegalMoves();

                if (listLegalMoves.IndexOf(bitMove) == -1)
                    throw new Exception("Invalid move.");

                // lưu lại move và legal moves của state này
                legalMoves.Add(listLegalMoves);
                moves.Add(bitMove);

                // tới trạng thái tiếp theo của bàn cờ
                stateClone.NextState(bitMove);
            }
        }

        private static void ParseGgfGame(
            string strGame,
            out State state,
            out List<ulong> moves,
            out List<List<ulong>> legalMoves)
        {
            state = new State();
            moves = new List<ulong>();
            legalMoves = new List<List<ulong>>();

            //-----------------------------------------------------
            // kiểm tra xem có bắt đầu và kết thúc bằng (; ;)
            //-----------------------------------------------------
            if (strGame.StartsWith("1 ") || strGame.StartsWith("2 "))
                strGame = strGame[2..];

            if (!strGame.StartsWith("(;") || !strGame.EndsWith(";)"))
                throw new Exception("strGame does not contain any game");

            //-----------------------------------------------------
            // tách các cặp prop args ra, tính initial board
            //-----------------------------------------------------
            var regex = new Regex(@"(?<prop>[^\[]+)\[(?<args>[^\]]*)\]");
            var parsedChunks = regex.Matches(strGame.Substring(2, strGame.Length - 4));
            foreach (Match match in parsedChunks)
            {
                var prop = match.Groups["prop"].Value;
                var args = match.Groups["args"].Value;

                switch (prop)
                {
                    case "BO":
                    {
                        state = ParseBoardChunk(args) ?? throw new Exception("Invalid board.");
                        moves.Clear();
                        break;
                    }
                    case "B":
                    case "W":
                    {
                        moves.Add(ParseMovesChunk(args));
                        break;
                    }
                }
            }

            //-----------------------------------------------------
            // tính và lưu legal moves
            //-----------------------------------------------------
            var stateClone = state.Clone();
            foreach (var move in moves)
            {
                // Passing move
                if (move == 0)
                {
                    legalMoves.Add(new List<ulong>());
                    stateClone.SwapPlayer();
                }
                // Normal move
                else
                {
                    var listLegalMoves = stateClone.GetListLegalMoves();
                    if (listLegalMoves.IndexOf(move) == -1)
                        throw new Exception("Invalid move.");

                    legalMoves.Add(listLegalMoves);
                    stateClone.NextState(move);
                }
            }
        }

        //------------------------------------------------
        // parse Initial board setup
        //------------------------------------------------
        private static State ParseBoardChunk(string args)
        {
            // thông số của BO là mot chuoi cac segment, moi segment cach nhau bang khoan trang
            // segment thu nhat la kich thuoc cua ban co
            // cac segment con lai la cac day cac o cua ban co
            // segment cuoi cung cho biet day la nuoc di cua ai
            var segments = args.Split(' ');

            // kiem tra xem du lieu co hop le hay khong
            if (segments.Length < 10 || segments[0] != "8")
                return null;
            for (var i = 1; i <= 8; i++)
                if (segments[i].Length < 8)
                    segments[i] += new string('-', 8 - segments[i].Length);

            // xay dung ban co
            var state = new State();
            state.Board.Clear();

            for (var row = 0; row < 8; row++)
            for (var col = 0; col < 8; col++)
            {
                var ch = segments[row + 1][col];

                // xác định xem tại vị trí (x,y) là quân cờ gì
                switch (ch)
                {
                    case 'O':
                        state.Board.SetPieceAt((row, col).ToBitMove(), Constant.White);
                        break;
                    case '*':
                        state.Board.SetPieceAt((row, col).ToBitMove(), Constant.Black);
                        break;
                    case '-':
                        // empty cell
                        break;
                    default:
                        return null;
                }
            }

            state.SetPlayer(segments[9] == "*" ? Constant.Black : Constant.White);
            state.ReCalculateLegalMoves();
            return state;
        }

        //------------------------------------------------
        // parse moves
        //------------------------------------------------
        private static ulong ParseMovesChunk(string args)
        {
            // thông số của B|W là
            // o move
            // o evalution
            // o time
            if (args.StartsWith("pa", StringComparison.CurrentCultureIgnoreCase))
                return 0UL;
            var segments = args.Split('/');
            return segments[0].ToBitMove();
        }
    }
}