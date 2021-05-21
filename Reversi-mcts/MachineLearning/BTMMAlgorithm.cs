using System;
using System.Collections.Generic;
using System.Diagnostics;
using Reversi_mcts.Board;
using Reversi_mcts.Utils;

namespace Reversi_mcts.MachineLearning
{
    public static class BTMMAlgorithm
    {
        public static List<PatternMining> ListPatternMining = new List<PatternMining>();
        private static int _gameCount = 0;
        private static int _gameMiss = 0;
        private static int _count = 0;

        // https://stackoverflow.com/a/3906931/11898496
        // Contain list of game records
        private static List<List<ulong>> _parsedGamesMoves = new List<List<ulong>>();

        // legalMoves corresponding to each of ParsedGamesMoves
        private static List<List<List<ulong>>> _parsedGamesLegalMoves = new List<List<List<ulong>>>();

        public static void Run()
        {
            LoadGameDatabase();
            InitPatternMinings();
            Train(20);

            //SaveTrainingData();
            //LoadTrainingData();
        }

        private static void LoadTrainingData()
        {
            // PatternMinings: List
            //      - PatternMining: Instant of Class
            //          - Gamma: [,,] - mảng 3 chiều

            Console.WriteLine("Loading trained data...");
            ListPatternMining = FileUtils.ReadFromBinaryFile<List<PatternMining>>(Constant.TrainedDataFilePath);
        }

        private static void SaveTrainingData()
        {
            Console.WriteLine("Saving trained data...");
            FileUtils.WriteToBinaryFile(Constant.TrainedDataFilePath, ListPatternMining);
        }

        // Load game-record từ file
        // Đọc và lưu tất cả MOVE
        // Tính và lưu tất cả LEGALMOVES cho từng move trong từng game-record
        private static void LoadGameDatabase()
        {
            const string filePath = Constant.GameRecordFilePath;
            Console.WriteLine("\nParsing game-records from {0}...", filePath);

            try
            {
                var parser = new GameRecordParser();
                parser.Parse(filePath);
                _parsedGamesMoves = parser.ParsedGamesMoves;
                _parsedGamesLegalMoves = parser.ParsedGamesLegalMoves;
                _gameCount = parser.GameCount;
                _gameMiss = parser.GameMiss;
            }
            catch (Exception caught)
            {
                Console.WriteLine("Error: {0}", caught);
                Process.GetCurrentProcess().Kill();
            }

            Console.WriteLine("> Parsed {0} games. Miss {1} games.", _gameCount, _gameMiss);
        }

        // PatternMinings là danh sách tất cả patternShape sau khi được Flip/Rotate/Mirror
        // Thầy có 25 PatternShape ban đầu, sau khi flip/rotate/mirror 8 hướng thì được 25*8=200 pattern-minings
        private static void InitPatternMinings()
        {
            var patternShapes = new List<PatternShape>();

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
            Console.WriteLine("\nParsing {0} PatternShapes of nqhuy...", patternShapesString.Count);
            foreach (var (notations, targetNotation) in patternShapesString)
            {
                var ps = PatternShape.Parse(notations, targetNotation);
                patternShapes.AddRange(ps.Sym8());
            }

            Console.WriteLine("> Parsed {0} pattern shapes.", patternShapes.Count);

            // ---------- Tạo danh sách PatternMinings từ PatternShapes ----------
            Console.WriteLine("\nCreate PatternMinnings from PatternShapes...");
            var progressBar = new ProgressBar();
            for (var i = 0; i < patternShapes.Count; i++)
            {
                progressBar.Report((double) i / patternShapes.Count);
                ListPatternMining.Add(PatternMining.CreatePatternMining(patternShapes[i]));
            }

            progressBar.Dispose();
            Console.WriteLine("> Created {0} pattern minings.", ListPatternMining.Count);
        }

        private static void Train(int epoch = 20)
        {
            // ---------- split test/train ----------
            Console.WriteLine("\nSplitting train/test...");
            const double testPercentage = 5; // 5% test
            var testSize = (int) (testPercentage / 100 * _gameCount);
            var trainSize = _gameCount - testSize;
            Console.WriteLine("> Train size: {0} ({1}%)\n> Test size: {2} ({3}%)",
                trainSize, 100 - testPercentage,
                testSize, testPercentage);

            // ---------- calculate Wi ----------
            WinGamma(testSize, _gameCount);

            // ---------- train ----------
            Console.WriteLine("\nStarting train {0} epochs ...", epoch);
            for (var i = 0; i < epoch; i++)
            {
                Console.WriteLine("\n---------------------------------");
                Console.WriteLine("----------- Epoch {0} ------------", i + 1);
                Console.WriteLine("---------------------------------");

                CalculateGammaTest(0, testSize);
                CalculateGamma(testSize, _gameCount);
            }

            Console.WriteLine("\n> Done training.");
        }

        // Calculate Wi (win_count for each feature)
        private static void WinGamma(int begin, int end)
        {
            var totalGame = end - begin;
            var progressBar = new ProgressBar();
            Console.WriteLine("\nCalculating Wi for {0} games...", totalGame);

            // Loop với từng game (ván cờ)
            for (var iGame = begin; iGame < end; iGame++)
            {
                var state = new State();
                var listBitMove = _parsedGamesMoves[iGame]; // lấy ra list-move của game thứ iGame từ ParsedGameMoves

                for (var iMove = 0; iMove < listBitMove.Count; iMove++) // Với từng move (nước đi) trong game đang xét
                {
                    var bitMove = listBitMove[iMove];
                    var legalMoves = _parsedGamesLegalMoves[iGame][iMove]; // Lấy ra legalmoves
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
                                var pm = ListPatternMining[iPatternMining];

                                // Lấy ra index của legalMove trong patterShape đó
                                var legalMoveIndex = pm.PatternShape.IndexOfBitCell(legalMove);

                                // Và tính patternId (unique id của pattern)
                                var pCode = pm.PatternShape.CalculatePatternCode(state.Board);

                                // Nếu legalmove này được chọn làm move tiếp theo thì tăng win
                                if (legalMove == bitMove)
                                    pm.Win[pCode, legalMoveIndex, player]++;

                                // Tăng canditate (số lần xuất hiện) cho pattern
                                pm.Candidate[pCode, legalMoveIndex, player]++;
                            }
                        }
                    }

                    // Next State tới move tiếp theo
                    state.NextState(bitMove);
                }

                progressBar.Report((double) (iGame - begin) / totalGame);
            }

            progressBar.Dispose();
            Console.WriteLine("> Done.");
        }

        private static void CalculateGammaTest(int begin, int end)
        {
            Console.WriteLine("\nCalculate GammaTest...");

            var progress = new ProgressBar();
            double likelihood = 0;
            double prob = 0;
            var beginTime = DateTime.Now;

            var count = 0;
            for (var iGame = begin; iGame < end; iGame++)
            {
                var state = new State();
                var listBitMove = _parsedGamesMoves[iGame];
                for (var iMove = 0; iMove < _parsedGamesMoves[iGame].Count; iMove++)
                {
                    var move = listBitMove[iMove];
                    var legalMoves = _parsedGamesLegalMoves[iGame][iMove];

                    if (legalMoves.Count > 1)
                    {
                        var val = StrongOfAction(state, move) / CalculateE(state, legalMoves);
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
            Console.WriteLine("> Time spent = {0:0,000} ms", (DateTime.Now - beginTime).TotalMilliseconds);
            Console.WriteLine("> Aver_Prop: {0}", prob / count);
        }

        // Calculate and update all strengths of all features in which attent in N moves
        private static void CalculateGamma(int begin, int end)
        {
            // Tính mẫu số SUM(Cij/E)
            CalculateGammaDenominator(begin, end);

            // ------------------------------------------------------------------
            // ------------------- Tính Gamma sau khi biết mẫu số ---------------
            // ------------------------------------------------------------------
            // Calculate gamma after accumulating Cij/Ej for each feature
            Console.WriteLine("\nCalculate Gamma ...");
            var progress = new ProgressBar();
            for (var i = 0; i < ListPatternMining.Count; i++)
            {
                var pm = ListPatternMining[i];
                var len = pm.PatternCommonLength;
                var iCard = MathUtils.Power3(len);

                for (var iPatId = 0; iPatId < iCard; iPatId++) // loop through all pattern-id indexes
                {
                    for (var iBitCell = 0; iBitCell < len; iBitCell++) // loop through all bit-cell-index
                    {
                        for (var player = 0; player <= 1; player++) // loop through 2 player: black-white
                        {
                            var denominator = pm.GammaDenominator[iPatId, iBitCell, player];
                            if (denominator != 0)
                            {
                                var win = pm.Win[iPatId, iBitCell, player];
                                var gamma = pm.Gamma[iPatId, iBitCell, player];
                                var gamma1 = Math.Pow(gamma, 0.75) * Math.Pow(win / denominator, 0.25);

                                pm.Gamma[iPatId, iBitCell, player] =
                                    ((float) gamma1).LimitToRange(0.01f, 100f);

                                // reset gamma denominator
                                pm.GammaDenominator[iPatId, iBitCell, player] = 0;
                            }
                        }
                    }
                }

                progress.Report((double) i / ListPatternMining.Count);
            }

            progress.Dispose();
            Console.WriteLine("> Done.");
        }

        // Tính mẫu số: SUM(cij / e)
        private static void CalculateGammaDenominator(int begin, int end)
        {
            Console.WriteLine("\nCalculate GammaDenominator SUM(Cij / E)...");
            var progress = new ProgressBar();
            var likelihood = 0.0;
            var prob = 0.0;
            var beginTime = DateTime.Now;
            for (var iGame = begin; iGame < end; iGame++)
            {
                var state = new State();
                var listBitMove = _parsedGamesMoves[iGame];

                // loop through all move in current game
                for (var moveIdx = 0; moveIdx < listBitMove.Count; moveIdx++)
                {
                    var bitMove = listBitMove[moveIdx];
                    var legalMoves = _parsedGamesLegalMoves[iGame][moveIdx];
                    var turn = state.Player;
                    var n = legalMoves.Count;

                    if (n > 1)
                    {
                        var e = 0f; // calculateE(gameBoard, legalMoves);
                        var strongLegalMoves = new float[n];
                        var iMove = -1;

                        // calculate E, find iMove
                        for (var k = 0; k < n; k++)
                        {
                            strongLegalMoves[k] = StrongOfAction(state, legalMoves[k]);
                            if (legalMoves[k] == bitMove)
                                iMove = k;
                            e += strongLegalMoves[k];
                        } // end loop calculate E

                        var val = strongLegalMoves[iMove] / e;

                        for (var k = 0; k < n; k++)
                        {
                            var patternsIndexes = IdentifyPattern(legalMoves[k]);
                            foreach (var t in patternsIndexes)
                            {
                                var pattern = ListPatternMining[t];

                                var pos = pattern.PatternShape.IndexOfBitCell(legalMoves[k]);
                                var patternCode = pattern.PatternShape.CalculatePatternCode(state.Board);
                                var candidate = pattern.Candidate[patternCode, pos, turn];
                                if (candidate > 10)
                                {
                                    var cij = CalculateCij(strongLegalMoves[k], t, patternCode, pos, turn);
                                    ListPatternMining[t].GammaDenominator[patternCode, pos, turn] += cij / e;
                                }
                            }
                        } // end loop k

                        likelihood += Math.Log(val);
                        prob += val;
                    } // end if

                    state.NextState(bitMove);
                }

                progress.Report((double) (iGame - begin) / (end - begin));
            }

            progress.Dispose();
            Console.WriteLine("> Likelihood = {0:0.0000000000000000000000000}", likelihood / _count);
            Console.WriteLine("> Time spent = {0:0,000} ms", (DateTime.Now - beginTime).TotalMilliseconds);
        }

        private static float CalculateCij(float strong, int patternCode, int id, int index, int turn)
        {
            var gamma = ListPatternMining[patternCode].Gamma[id, index, turn];
            return strong / gamma;
        }

        // Tính E
        private static float CalculateE(State state, List<ulong> legalMoves)
        {
            float e = 0;
            foreach (var bitMove in legalMoves)
                e += StrongOfAction(state, bitMove);
            return e;
        }

        // Trả về sức mạnh của 1 move trong 1 state
        public static float StrongOfAction(State state, ulong bitMove)
        {
            var relatedPatterns = IdentifyPattern(bitMove);
            var turn = state.Player;
            var strong = 1f;
            foreach (var t in relatedPatterns)
            {
                var pos = ListPatternMining[t].PatternShape.IndexOfBitCell(bitMove);
                var patternCode = ListPatternMining[t].PatternShape.CalculatePatternCode(state.Board);
                var gamma = ListPatternMining[t].Gamma[patternCode, pos, turn];
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
                for (var iPattern = 0; iPattern < ListPatternMining.Count; iPattern++)
                {
                    var patternShape = ListPatternMining[iPattern].PatternShape;

                    // Nếu ô đang xét là targetCell của pattern đang xét => Add index vào patternIndexes
                    if (patternShape.TargetBitCell == bitCell)
                        patternIndexes.Add(iPattern);
                }

                // Lưu indexes để có thể dùng lại
                result[bitCell] = patternIndexes.ToArray();
            }

            return result;
        }
    }
}