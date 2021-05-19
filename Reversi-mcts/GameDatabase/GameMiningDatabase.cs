using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Reversi_mcts.GameDatabase
{
    public static class GameMiningDatabase
    {
        private static bool _isInitialized = false;

        public static int GameCount = 0;
        public static int GameMiss = 0;
        
        public static List<List<ulong>> ParsedGamesMoves = new List<List<ulong>>();
        public static List<List<List<ulong>>> ParsedGamesLegalMoves = new List<List<List<ulong>>>();

        public static void EnsureInitialized()
        {
            if (_isInitialized) return;
            Initialize();
            _isInitialized = true;
        }

        private static void Initialize()
        {
            //---------------------------------------------------------
            // game record file
            //---------------------------------------------------------
            const string filePath = "E:\\game-record.txt";

            //--------------------------------------------
            // Initialize database
            //--------------------------------------------
            try
            {
                var parser = new GameRecordParser();
                parser.Parse(filePath);
                ParsedGamesMoves = parser.ParsedGamesMoves;
                ParsedGamesLegalMoves = parser.ParsedGamesLegalMoves;
                GameCount = parser.GameCount;
                GameMiss = parser.GameMiss;
            }
            catch (Exception caught)
            {
                Console.WriteLine("Error: {0}", caught);
                Process.GetCurrentProcess().Kill();
            }

            Console.WriteLine("\n> Parsed {0} games. Miss {1} games.", GameCount, GameMiss);
        }
    }
}