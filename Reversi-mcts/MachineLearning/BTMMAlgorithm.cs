using System;
using System.Collections.Generic;
using System.Diagnostics;
using Reversi_mcts.Board;
using Reversi_mcts.Utils;

namespace Reversi_mcts.MachineLearning
{
    public static class BTMMAlgorithm
    {
        private static List<PatternMining> _listPatternMining = new List<PatternMining>();
        private static int _gameCount = 0;
        private static int _gameMiss = 0;
        private static int _count = 0;

        // https://stackoverflow.com/a/3906931/11898496
        private static Dictionary<int, State> _parsedStates = new Dictionary<int, State>();
        private static List<List<ulong>> _parsedMoves = new List<List<ulong>>();
        private static List<List<List<ulong>>> _parsedLegalMoves = new List<List<List<ulong>>>();

        public static bool IsModelReady()
        {
            return _listPatternMining.Count > 0;
        }

        public static void TrainGameRecord(string gameRecordPath, string savePath)
        {
            LoadGameDatabase(gameRecordPath);
            InitListPatternMining();
            Train(20);
            SaveTrainedData(savePath);
        }

        public static void LoadTrainedData(string filePath)
        {
            Console.WriteLine($"Loading trained data from {filePath} ...");
            _listPatternMining = FileUtils.ReadFromBinaryFile<List<PatternMining>>(filePath);
            Console.WriteLine("> Loaded.");
        }

        private static void SaveTrainedData(string filePath)
        {
            Console.WriteLine($"Saving trained data to {filePath} ...");
            FileUtils.WriteToBinaryFile(filePath, _listPatternMining);
            Console.WriteLine($"> Saved.");
        }

        // Load game-record từ file
        // Đọc và lưu tất cả States(nếu cần), Moves (trong từng State), LegalMoves (của từng Move)
        private static void LoadGameDatabase(string filePath)
        {
            Console.WriteLine($"\nParsing game-records from {filePath}...");

            try
            {
                var parser = new GameRecordParser();
                parser.Parse(filePath);
                _parsedStates = parser.ParsedStates;
                _parsedMoves = parser.ParsedMoves;
                _parsedLegalMoves = parser.ParsedLegalMoves;
                _gameCount = parser.GameCount;
                _gameMiss = parser.GameMiss;
            }
            catch (Exception caught)
            {
                Console.WriteLine($"[!] Error: {caught}");
                Process.GetCurrentProcess().Kill();
            }

            Console.WriteLine($"> Parsed {_gameCount} games. Miss {_gameMiss} games.");
        }

        // ListPatternMining là danh sách tất cả patternShape sau khi được Flip/Rotate/Mirror
        // Thầy có 25 PatternShape ban đầu, sau khi flip/rotate/mirror 8 hướng thì được 25*8=200 pattern-mining
        private static void InitListPatternMining()
        {
            // patternShape của thầy, dưới dạng string
            var patternShapesString = new List<(string, string)>
            {
                // 0
                ("a1,b1,c1,d1,e1,f1,g1,h1,b2", "b1"),
                ("a1,b1,c1,d1,a2,b2,c2,a3", "b1"),
                ("a1,b1,f1,c2,g2,b3,g7", "b1"),

                // 1
                ("a1,b1,c1,d1,e1,f1,h1,b2,c2,e2,e3", "c1"),
                ("c1,d1,c2,d2,f2,b3,e3,f4", "c1"),
                ("a1,b1,c1,b2,c2,d2,a3,c3,c6", "c1"),

                // 2
                ("c1,d1,e1,f1,h1,f2,d5", "d1"),
                ("a1,d1,f1,g1,h1,c2,d2,e2,f2,f3", "d1"),
                ("c1,d1,e1,c2,d2,e2,c3,d3,e4", "d1"),

                // 3
                ("a1,b1,c1,a2,b2,a3,f6", "b2"),
                ("a1,h1,b2,b3,b5,b5,b6,a7,b7", "b2"),

                // 4
                ("c2,b3,c3,d3,a4,b4,c4,f4", "c2"),
                ("a2,c2,d2,c3,d3,b4,d4,c6", "c2"),
                ("c1,c2,d2,e2,e3,c4,d6,f6", "c2"),

                // 5
                ("a1,b2,c2,d2,e2,c3,d3,e3,c4,f4,a5", "d2"),
                ("h1,a2,c2,d2,e2,f2,d3,e3,c6", "d2"),
                ("d2,f2,c3,e3,c5,e5,f6", "d2"),

                // 6
                ("c1,f2,c3,d3,f3,c4,e4,f4,c5", "d3"),
                ("c3,d3,e3,f4,a5,d6,e6,d7,b8", "d3"),
                ("e2,c3,d3,d4,f4,f5,c6,d6,e6,g6", "d3"),
                ("c3,d3,e3,d4,b5", "d3"),

                // 7
                ("h1,c3,e3,h3,f4,c5,d6,f6,a8,c8", "c3"),
                ("a1,c3,d3,c4,d4,c5,e5,c6,b7,c7", "c3"),

                // 8
                ("a1,b1,c1,d1,g1,h1", "a1"),
                ("a1,b1,c1,a2,b2,a3", "a1")
            };

            // ---------- TEST: Hiển thị patternShape dưới dạng board để kiểm tra ----------
            // var index = 1;
            // foreach (var (item1, item2) in patternShapesString)
            // {
            //     var board = new BitBoard();
            //     board.Clear();
            //     foreach (var notation in item1.Split(","))
            //     {
            //         board.SetPieceAt(notation.ToBitMove(), Constant.Black);
            //     }
            //     board.SetPieceAt(item2.ToBitMove(), Constant.White);
            //     
            //     Console.WriteLine("\n- Pattern Shape {0}:", index);
            //     board.Display();
            //
            //     index++;
            // }

            // ---------- Thêm các patternShapes và các flip/mirror/rotate của nó vào List ----------
            Console.WriteLine($"\nParsing {patternShapesString.Count} PatternShapes of nqhuy...");
            var patternShapes = new List<PatternShape>();
            foreach (var (notations, targetNotation) in patternShapesString)
            {
                var ps = PatternShape.Parse(notations, targetNotation);
                patternShapes.AddRange(ps.Sym8());
            }

            Console.WriteLine($"> Parsed {patternShapes.Count} pattern shapes.");

            // ---------- Tạo ListPatternMining từ PatternShapes ----------
            Console.WriteLine("\nCreate ListPatternMining from PatternShapes...");
            var progressBar = new ProgressBar();
            _listPatternMining.Clear();
            for (var i = 0; i < patternShapes.Count; i++)
            {
                progressBar.Report((double) i / patternShapes.Count);
                _listPatternMining.Add(new PatternMining(patternShapes[i]));
            }

            progressBar.Dispose();
            Console.WriteLine($"> Created {_listPatternMining.Count} pattern minings.");
        }

        private static void Train(int epochs = 20)
        {
            // ---------- split test/train ----------
            Console.WriteLine("\nSplitting train/test...");
            const double testPercent = 5; // 5% test
            var testSize = (int) (testPercent / 100 * _gameCount);
            var trainSize = _gameCount - testSize;
            Console.WriteLine($"> Train size: {trainSize} ({100 - testPercent}%)");
            Console.WriteLine($"> Test size: {testSize} ({testPercent}%)");

            // ---------- calculate Wi ----------
            CalculateWi(testSize, _gameCount);

            // ---------- train ----------
            Console.WriteLine($"\nStarting train {epochs} epochs ...");
            GameLogger.WriteBeginTrain(_gameCount);
            for (var i = 0; i < epochs; i++)
            {
                Console.WriteLine("\n---------------------------------");
                Console.WriteLine($"----------- Epoch {i + 1} ------------");
                Console.WriteLine("---------------------------------");
                CalculateGammaTest(i, 0, testSize);
                CalculateGamma(i, testSize, _gameCount);
            }

            GameLogger.WriteFinishTrain();
            Console.WriteLine("\n> Done training.");
        }

        // Calculate Wi (win_count for each feature)
        private static void CalculateWi(int begin, int end)
        {
            var totalGame = end - begin;
            var progressBar = new ProgressBar();
            Console.WriteLine($"\nCalculating Wi for {totalGame} games...");

            // Loop với từng game (ván cờ)
            for (var iGame = begin; iGame < end; iGame++)
            {
                var moves = _parsedMoves[iGame]; // lấy ra list-move của game thứ iGame từ ParsedGameMoves
                var state = _parsedStates.TryGetValue(iGame, out var savedState) ? savedState.Clone() : new State();

                for (var iMove = 0; iMove < moves.Count; iMove++) // Với từng move (nước đi) trong game đang xét
                {
                    var move = moves[iMove];
                    var legalMoves = _parsedLegalMoves[iGame][iMove]; // Lấy ra legalmoves
                    var player = state.Player; // lượt chơi hiện tại

                    // Nếu có từ 2 legalMove trở lên MỚI CẦN TÍNH
                    // Nếu chỉ có 1 legalMove thì đánh nó luôn cần gì tính
                    // Nếu không có legalMove thì pass cần gì tính
                    if (legalMoves.Count > 1)
                    {
                        _count++;

                        // Với từng legalMove
                        foreach (var legalMove in legalMoves)
                        {
                            // Lấy ra các patternShape có targetCell (ô đỏ) là legalMove đang xét
                            var patternIndexes = IdentifyPattern(legalMove);

                            // Với từng patternShape tìm được từ legalMove đang xét
                            foreach (var iPatternMining in patternIndexes)
                            {
                                var pm = _listPatternMining[iPatternMining];

                                // Lấy ra index của legalMove trong patterShape đó
                                var cellIndex = pm.PatternShape.IndexOfBitCell(legalMove);

                                // Và tính patternId (unique id của pattern)
                                var patternCode = pm.PatternShape.CalculatePatternCode(state.Board);

                                // Nếu legalmove này được chọn làm move tiếp theo thì tăng win
                                if (legalMove == move)
                                    pm.AddWin(patternCode, cellIndex, player);

                                // Tăng canditate (số lần xuất hiện) cho pattern
                                pm.AddCandidate(patternCode, cellIndex, player);
                            }
                        }
                    }

                    // Next State tới move tiếp theo
                    state.NextState(move);
                }

                progressBar.Report((double) (iGame - begin) / totalGame);
            }

            progressBar.Dispose();
            Console.WriteLine("> Done.");
        }

        private static void CalculateGammaTest(int loop, int begin, int end)
        {
            Console.WriteLine("\nCalculate GammaTest...");

            var progress = new ProgressBar();
            double likelihood = 0;
            double prob = 0;

            var count = 0;
            for (var iGame = begin; iGame < end; iGame++)
            {
                var state = _parsedStates.TryGetValue(iGame, out var savedState) ? savedState.Clone() : new State();
                var listBitMove = _parsedMoves[iGame];
                for (var iMove = 0; iMove < _parsedMoves[iGame].Count; iMove++)
                {
                    var move = listBitMove[iMove];
                    var legalMoves = _parsedLegalMoves[iGame][iMove];
                    if (legalMoves.Count > 1)
                    {
                        double e = E(state, legalMoves);
                        var val = StrongOfAction(state, move) / e;
                        likelihood += Math.Log(val);
                        prob += val;
                        count++;
                    }

                    state.NextState(move);
                }

                progress.Report((double) (iGame - begin) / (end - begin));
            }

            progress.Dispose();
            Console.WriteLine("> Likelihood = {0:0.0000000000000000000000000}", likelihood / count);
            Console.WriteLine("> Aver_Prop: {0}", prob / count);
            GameLogger.WriteMleLog(loop, likelihood / count, prob / count);
        }

        // Calculate and update all strengths of all features in which attent in N moves
        private static void CalculateGamma(int loop, int begin, int end)
        {
            // Tính mẫu số SUM(Cij/E)
            CalculateGammaDenominator(loop, begin, end);

            // ------------------------------------------------------------------
            // ------------------- Tính Gamma sau khi biết mẫu số ---------------
            // ------------------------------------------------------------------
            // Calculate gamma after accumulating Cij/Ej for each feature
            Console.WriteLine("\nCalculate Gamma ...");
            var progress = new ProgressBar();
            for (var iPat = 0; iPat < _listPatternMining.Count; iPat++)
            {
                var patternMining = _listPatternMining[iPat];
                var len = patternMining.PatternShape.ArrayBitCells.Length;
                var iCard = MathUtils.Power3(len);

                for (var patternCode = 0; patternCode < iCard; patternCode++) // loop through all pattern-code
                {
                    for (var cellIndex = 0; cellIndex < len; cellIndex++) // loop through all cell-index
                    {
                        for (var player = 0; player <= 1; player++) // loop through 2 player: black-white
                        {
                            var denominator = patternMining.GetGammaDenominator(patternCode, cellIndex, player);
                            if (denominator != 0)
                            {
                                var win = patternMining.GetWin(patternCode, cellIndex, player);
                                var oldGamma = patternMining.GetGamma(patternCode, cellIndex, player);
                                var newGamma = win / denominator;
                                
                                // Smooth Step: https://en.wikipedia.org/wiki/Smoothstep
                                var smoothStep = Math.Pow(oldGamma, 0.75) * Math.Pow(newGamma, 0.25);
                                var value = ((float) smoothStep).LimitToRange(0.01f, 100f);
                                patternMining.SetGamma(patternCode, cellIndex, player, value);

                                // reset gamma denominator
                                patternMining.SetGammaDenominator(patternCode, cellIndex, player, 0);
                            }
                        }
                    }
                }

                progress.Report((double) iPat / _listPatternMining.Count);
            }

            progress.Dispose();
            Console.WriteLine("> Done.");
        }

        // Tính mẫu số: SUM(cij / e)
        private static void CalculateGammaDenominator(int loop, int begin, int end)
        {
            Console.WriteLine("\nCalculate GammaDenominator SUM(Cij / E)...");
            double likelihood = 0;
            double prob = 0;
            var progress = new ProgressBar();
            for (var iGame = begin; iGame < end; iGame++)
            {
                // Console.SetCursorPosition(0, Console.CursorTop - 1);
                // Console.WriteLine("Working with {0}", iGame);
                var state = _parsedStates.TryGetValue(iGame, out var savedState) ? savedState.Clone() : new State();

                // loop through all move in current game
                var moves = _parsedMoves[iGame];
                for (var iMove = 0; iMove < moves.Count; iMove++)
                {
                    var move = moves[iMove];
                    var player = state.Player;
                    var legalMoves = _parsedLegalMoves[iGame][iMove];
                    var lenLegalMoves = legalMoves.Count;

                    // Nếu có từ 2 legal move trở lên mới tính
                    if (lenLegalMoves > 1)
                    {
                        // tính E và sức mạnh từng legalmoves
                        var e = 0f; // E(state, legalMoves);
                        var strongLegalMoves = new float[lenLegalMoves];
                        var iChoseLegalMove = -1;
                        for (var iLegalMove = 0; iLegalMove < lenLegalMoves; iLegalMove++)
                        {
                            strongLegalMoves[iLegalMove] = StrongOfAction(state, legalMoves[iLegalMove]);
                            e += strongLegalMoves[iLegalMove];
                            if (legalMoves[iLegalMove] == move)
                                iChoseLegalMove = iLegalMove;
                        }
                        
                        var val = strongLegalMoves[iChoseLegalMove] / e;

                        // Tính mẫu số và cập nhật vào _listPatternMining
                        for (var k = 0; k < lenLegalMoves; k++)
                        {
                            var patternsIndexes = IdentifyPattern(legalMoves[k]);
                            foreach (var iPat in patternsIndexes)
                            {
                                var pattern = _listPatternMining[iPat];
                                var cellIndex = pattern.PatternShape.IndexOfBitCell(legalMoves[k]);
                                var patternCode = pattern.PatternShape.CalculatePatternCode(state.Board);
                                var candidate = pattern.GetCandidate(patternCode, cellIndex, player);
                                if (candidate > 10)
                                {
                                    var cij = Cij(strongLegalMoves[k], iPat, patternCode, cellIndex, player);
                                    _listPatternMining[iPat].AddGammaDenominator(patternCode, cellIndex, player, 
                                        cij / e);
                                }
                            }
                        } // end loop k

                        likelihood += Math.Log(val);
                        prob += val;

                    } // end if

                    state.NextState(move);
                } // end loop moves

                progress.Report((double) (iGame - begin) / (end - begin));
            } // end loop games

            progress.Dispose();
            Console.WriteLine("> Likelihood = {0:0.0000000000000000000000000}", likelihood / _count);
            Console.WriteLine("> Aver_Prop: {0}", prob / _count);
            GameLogger.WriteMleLog(loop, likelihood / _count, prob / _count);
        }

        private static float Cij(float strong, int iPat, int patternCode, int index, int player)
        {
            var gamma = _listPatternMining[iPat].GetGamma(patternCode, index, player);
            return strong / gamma;
        }

        // Tính E
        private static float E(State state, List<ulong> legalMoves)
        {
            float e = 0;
            foreach (var move in legalMoves)
                e += StrongOfAction(state, move);
            return e;
        }

        // Trả về sức mạnh của 1 move trong 1 state
        public static float StrongOfAction(State state, ulong move)
        {
            var relatedPatterns = IdentifyPattern(move);
            var player = state.Player;
            var strong = 1f;
            foreach (var iPat in relatedPatterns)
            {
                var cellIndex = _listPatternMining[iPat].PatternShape.IndexOfBitCell(move);
                var patternCode = _listPatternMining[iPat].PatternShape.CalculatePatternCode(state.Board);
                var gamma = _listPatternMining[iPat].GetGamma(patternCode, cellIndex, player);
                strong *= gamma;
            }

            return strong;
        }

        // identify indices of pattern-minings that related to an action
        // Định dạng: Dictionary<bitMove, [pattern-idx1, pattern-idx2, pattern-idx3,...]>
        private static Dictionary<ulong, int[]> _identifyPatternCached;

        // Trả về index của những patternShape có targetCell (ô đỏ) == bitMove
        private static int[] IdentifyPattern(ulong bitMove)
        {
            // nếu chưa có thì init, có rồi thì thôi -> chỉ init 1 lần
            _identifyPatternCached ??= InitIdentifyPatternCached();

            return _identifyPatternCached[bitMove];
        }

        private static Dictionary<ulong, int[]> InitIdentifyPatternCached()
        {
            // tương ứng với 64 cells trên bàn cờ
            // Mỗi cell sẽ thuộc 1 hoặc nhiều patternShape nào đó
            var result = new Dictionary<ulong, int[]>(64);

            // loop qua từng ô trên bàn cờ (8x8=64 ô)
            for (var iCell = 0; iCell < 64; iCell++)
            {
                var patternIndexes = new List<int>();
                var bitCell = 0UL.SetBitAdIndex(iCell);

                // loop qua từng patternShapes
                for (var iPat = 0; iPat < _listPatternMining.Count; iPat++)
                {
                    var patternShape = _listPatternMining[iPat].PatternShape;

                    // Nếu ô đang xét là targetCell của pattern đang xét => Add index vào patternIndexes
                    if (patternShape.TargetBitCell == bitCell)
                        patternIndexes.Add(iPat);
                }

                // Lưu indexes để có thể dùng lại
                result[bitCell] = patternIndexes.ToArray();
            }

            return result;
        }
    }
}