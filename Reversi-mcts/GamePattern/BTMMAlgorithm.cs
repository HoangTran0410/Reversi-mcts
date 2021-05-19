using System;
using System.Collections.Generic;
using Reversi_mcts.Core.Board;
using Reversi_mcts.GameDatabase;

namespace Reversi_mcts.GamePattern
{
    public class BTMMAlgorithm
    {
        // https://stackoverflow.com/a/3906931/11898496
        // Contain list of game records
        public static List<List<ulong>> ParsedGamesMoves = new List<List<ulong>>();

        // legalMoves corresponding to each of ParsedGamesMoves
        public static List<List<List<ulong>>> ParsedGamesLegalMoves = new List<List<List<ulong>>>();

        public static void Run()
        {
            // ----------------------------------------
            // ---------- Load game database ----------
            // ----------------------------------------
            // GameMiningDatabase.EnsureInitialized();
            // ParsedGamesMoves = GameMiningDatabase.ParsedGamesMoves;
            // ParsedGamesLegalMoves = GameMiningDatabase.ParsedGamesLegalMoves;

            // ------------------------------------------------------------
            // ----------  Tao danh sach cac pattern de huan luyen --------
            // ------------------------------------------------------------
            var patternShapes = new List<PatternShape>();

            // patternShape của thầy
            var patternShapesString = new List<(string, string)>
            {
                // 0
                ("a1,b1,c1,d1,e1,f1,g1,h1,b2", "b1"),
                ("a1,b1,c1,d1,a2,b2,c2,a3", "b1"),
                ("a1,b1,f1,c2,g2,b3,g7", "b1"),
                
                // 1
                ("a1,b1,c1,d1,e1,f1,h1,b2,c2,e2,e3", "c1"),
                ("c1,d1,c2,d2,f2,b3,e3,f4","c1"),
                ("a1,b1,c1,b2,c2,d2,a3,c3,c6","c1"),
                
                // 2
                ("c1,d1,e1,f1,h1,f2,d5","d1"),
                ("a1,d1,f1,g1,h1,c2,d2,e2,f2,f3", "d1"),
                ("c1,d1,e1,c2,d2,e2,c3,d3,e4","d1"),
                
                // 3
                ("a1,b1,c1,a2,b2,a3,f6","b2"),
                ("a1,h1,b2,b3,b5,b5,b6,a7,b7", "b2"),
                
                // 4
                ("c2,b3,c3,d3,a4,b4,c4,f4","c2"),
                ("a2,c2,d2,c3,d3,b4,d4,c6","c2"),
                ("c1,c2,d2,e2,e3,c4,d6,f6","c2"),
                
                // 5
                ("a1,b2,c2,d2,e2,c3,d3,e3,c4,f4,a5","d2"),
                ("h1,a2,c2,d2,e2,f2,d3,e3,c6","d2"),
                ("d2,f2,c3,e3,c5,e5,f6","d2"),
                
                // 6
                ("c1,f2,c3,d3,f3,c4,e4,f4,c5","d3"),
                ("c3,d3,e3,f4,a5,d6,e6,d7,b8", "d3"),
                ("e2,c3,d3,d4,f4,f5,c6,d6,e6,g6","d3"),
                ("c3,d3,e3,d4,b5","d3"),
                
                // 7
                ("h1,c3,e3,h3,f4,c5,d6,f6,a8,c8","c3"),
                ("a1,c3,d3,c4,d4,c5,e5,c6,b7,c7","c3"),
                
                // 8
                ("a1,b1,c1,d1,g1,h1","a1"),
                ("a1,b1,c1,a2,b2,a3","a1")
            };

            // ------------------------------------------------------------------------------
            // ----------  TEST: Hiển thị patternShape dưới dạng board để kiểm tra ----------
            // ------------------------------------------------------------------------------
            var index = 1;
            foreach (var (item1, item2) in patternShapesString)
            {
                var board = new BitBoard();
                board.Clear();
                foreach (var notation in item1.Split(","))
                {
                    board.SetPieceAt(notation.ToBitMove(), Constant.Black);
                }
                board.SetPieceAt(item2.ToBitMove(), Constant.White);
                
                Console.WriteLine("\n- Pattern Shape {0}:", index);
                board.Display();
            
                index++;
            }

            // ------------------------------
            // ---------- Huan luyen --------
            // ------------------------------
        }
    }
}