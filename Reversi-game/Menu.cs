using System;
using Reversi_mcts;
using Reversi_game.PlayMode;
using Reversi_game.PlayMode.SocketIo;
using Reversi_mcts.MachineLearning;
using Reversi_mcts.Utils;

namespace Reversi_game
{
    public static class Menu
    {
        // Những biến được dùng ở nhiều sub-menu sẽ được để global ở đây
        private static string _gameRecordFilePath = @"E:\\game-record.txt";
        private static string _saveTrainedFilePath = @"E:\\trained";

        private static string _ggfFilePath = @"E:\\game-record.ggf";
        private static string _saveConvertedFilePath = @"E:\\game-record-converted.txt";

        public static void MainMenu()
        {
            var userChoice = 0;
            while (true)
            {
                userChoice = GetUserChoice(
                    "Main Menu", new[]
                    {
                        $"BTMM",
                        "AI vs AI",
                        "Human vs AI",
                        "Socket io",
                        "Tools",
                        "Quit"
                    }, userChoice);

                if (userChoice == 0) BTMMMenu();
                else if (userChoice == 1) AiVsAiMenu();
                else if (userChoice == 2) HumanVsAiMenu();
                else if (userChoice == 3) SocketIoMenu();
                else if (userChoice == 4) ToolsMenu();
                else if (userChoice == 5)
                {
                    ConsoleUtil.WriteAndWaitKey("Good bye! I will miss you <3");
                    return;
                }
            }
        }

        private static void BTMMMenu()
        {
            var userChoice = 0;
            while (true)
            {
                userChoice = GetUserChoice(
                    "BTMM Menu", new[]
                    {
                        "<- Back",
                        BTMMAlgorithm.IsModelReady() ? "* Status: READY" : "* Status: NOT READY!!",
                        "Train",
                        "Load",
                        "Read Log"
                    }, userChoice);

                if (userChoice == 0) return;
                if (userChoice == 1) CheckModelReady();
                else if (userChoice == 2) TrainMenu();
                else if (userChoice == 3) LoadMenu();
                else ConsoleUtil.WriteAndWaitKey(GameLogger.ReadLog());
            }
        }

        private static void TrainMenu()
        {
            var userChoice = 0;
            while (true)
            {
                userChoice = GetUserChoice(
                    "Train BTMM Menu", new[]
                    {
                        "<- Back",
                        $"Game records: {_gameRecordFilePath}",
                        $"Save to: {_saveTrainedFilePath}",
                        "Train now!"
                    }, userChoice);

                if (userChoice == 0) return;
                if (userChoice == 1)
                    _gameRecordFilePath = (string) ConsoleUtil.Prompt("Game record", _gameRecordFilePath);
                else if (userChoice == 2)
                    _saveTrainedFilePath = (string) ConsoleUtil.Prompt("Save to", _saveTrainedFilePath);
                else
                {
                    try
                    {
                        BTMMAlgorithm.TrainGameRecord(_gameRecordFilePath, _saveTrainedFilePath);
                        ConsoleUtil.WriteAndWaitKey("> Train End.");
                    }
                    catch (Exception e)
                    {
                        ConsoleUtil.WriteAndWaitKey($"[!] ERROR: {e}");
                    }
                }
            }
        }

        private static void LoadMenu()
        {
            var userChoice = 0;
            while (true)
            {
                userChoice = GetUserChoice(
                    "Load BTMM Menu", new[]
                    {
                        "<- Back",
                        $"Trained file: {_saveTrainedFilePath}",
                        "Load now!"
                    }, userChoice);

                if (userChoice == 0) return;
                if (userChoice == 1)
                    _saveTrainedFilePath = (string) ConsoleUtil.Prompt("Trained data file", _saveTrainedFilePath);
                else
                {
                    try
                    {
                        BTMMAlgorithm.LoadTrainedData(_saveTrainedFilePath);
                        ConsoleUtil.WriteAndWaitKey("> Load Done.");
                    }
                    catch (Exception e)
                    {
                        ConsoleUtil.WriteAndWaitKey($"[!] ERROR: {e}");
                    }
                }
            }
        }

        private static void AiVsAiMenu()
        {
            var totalRounds = 1;
            var blackAlgorithm = Algorithm.Mcts;
            var blackTimeout = 500;
            var whiteAlgorithm = Algorithm.Mcts;
            var whiteTimeout = 500;
            var userChoice = 0;
            while (true)
            {
                var mode = totalRounds == 1 ? "Log every moves" : "Only log round's record";
                userChoice = GetUserChoice(
                    "AI vs AI Menu", new[]
                    {
                        "<- Back",
                        $"+ Rounds         : {totalRounds} ({mode})",
                        $"+ Black algorithm: {blackAlgorithm}",
                        $"+ Black timeout  : {blackTimeout} (ms)",
                        $"+ White algorithm: {whiteAlgorithm}",
                        $"+ White timeout  : {whiteTimeout} (ms)",
                        "Fight Now!"
                    }, userChoice);

                if (userChoice == 0) return;
                if (userChoice == 1)
                {
                    Console.Write("> Rounds: ");
                    if (!int.TryParse(Console.ReadLine(), out totalRounds) || totalRounds <= 0)
                    {
                        totalRounds = 1;
                        ConsoleUtil.WriteAndWaitKey("> Invalid. Rounds must be int > 0.");
                    }
                }
                else if (userChoice == 2)
                {
                    Console.Write("> Black algorithm (Mcts, Mcts1, Mcts2): ");
                    blackAlgorithm = GetAlgorithm(Console.ReadLine());
                }
                else if (userChoice == 3)
                {
                    Console.Write("> Black timeout (ms): ");
                    if (!int.TryParse(Console.ReadLine(), out blackTimeout) || blackTimeout <= 0)
                    {
                        blackTimeout = 500;
                        ConsoleUtil.WriteAndWaitKey("> Invalid. Timeout must be int > 0.");
                    }
                }
                else if (userChoice == 4)
                {
                    Console.Write("> White algorithm (Mcts, Mcts1, Mcts2): ");
                    whiteAlgorithm = GetAlgorithm(Console.ReadLine());
                }
                else if (userChoice == 5)
                {
                    Console.Write("> White timeout (ms): ");
                    if (!int.TryParse(Console.ReadLine(), out whiteTimeout) || whiteTimeout <= 0)
                    {
                        whiteTimeout = 500;
                        ConsoleUtil.WriteAndWaitKey("> Invalid. Timeout must be int > 0.");
                    }
                }
                else
                {
                    if (totalRounds == 1) SelfPlay.OneRound(blackTimeout, blackAlgorithm, whiteTimeout, whiteAlgorithm);
                    else SelfPlay.MultiRounds(totalRounds, blackTimeout, blackAlgorithm, whiteTimeout, whiteAlgorithm);
                    ConsoleUtil.WriteAndWaitKey("> Fight End.");
                }
            }
        }

        private static void HumanVsAiMenu()
        {
            var aiAlgorithm = Algorithm.Mcts;
            var aiTimeout = 500;
            var humanColor = Constant.Black;
            var userChoice = 0;
            while (true)
            {
                userChoice = GetUserChoice(
                    "Human vs AI Menu", new[]
                    {
                        "<- Back",
                        $"+ Human color : {humanColor}",
                        $"+ AI algorithm: {aiAlgorithm}",
                        $"+ AI timeout  : {aiTimeout} (ms)",
                        "Play Now!"
                    }, userChoice);

                if (userChoice == 0) return;
                if (userChoice == 1)
                {
                    Console.Write("> Human color (0:black/1:white): ");
                    if (!byte.TryParse(Console.ReadLine(), out humanColor))
                    {
                        humanColor = 0;
                        ConsoleUtil.WriteAndWaitKey("> Invalid. Color must be 0(black) or 1(white).");
                    }
                }
                else if (userChoice == 2)
                {
                    Console.Write("> AI algorithm (Mcts, Mcts1, Mcts2): ");
                    aiAlgorithm = GetAlgorithm(Console.ReadLine());
                }
                else if (userChoice == 3)
                {
                    Console.Write("> AI timeout: ");
                    if (!int.TryParse(Console.ReadLine(), out aiTimeout) || aiTimeout <= 0)
                    {
                        aiTimeout = 500;
                        ConsoleUtil.WriteAndWaitKey("> Invalid. Timeout must be int > 0.");
                    }
                }
                else
                {
                    HumanVsAi.NewGame(Constant.White, aiTimeout, aiAlgorithm);
                    ConsoleUtil.WriteAndWaitKey("> Human vs AI End.");
                }
            }
        }

        private static void SocketIoMenu()
        {
            var serverAddress = "http://localhost";
            var serverPort = 3000;
            var clientName = "reversi-mcts-200ms";
            var algorithm = Algorithm.Mcts1;
            var timeout = 200;

            var userChoice = 0;
            while (true)
            {
                userChoice = GetUserChoice(
                    "Socket IO Menu", new[]
                    {
                        "<- Back",
                        $"+ Server Address  : {serverAddress}",
                        $"+ Server Port  : {serverPort}",
                        $"+ Client name: {clientName}",
                        $"+ Algorithm  : {algorithm}",
                        $"+ Timeout    : {timeout} (ms)",
                        "Connect Now!"
                    }, userChoice);

                if (userChoice == 0) return;
                if (userChoice == 1)
                    serverAddress = (string) ConsoleUtil.Prompt("Server Address", serverAddress);
                else if (userChoice == 2)
                {
                    Console.Write("> Timeout (ms): ");
                    if (!int.TryParse(Console.ReadLine(), out serverPort))
                    {
                        serverPort = 3000;
                        ConsoleUtil.WriteAndWaitKey("> Invalid. Port be int.");
                    }
                }
                else if (userChoice == 3)
                    clientName = (string) ConsoleUtil.Prompt("Client name", clientName);
                else if (userChoice == 4)
                {
                    Console.Write("> Algorithm (Mcts, Mcts1, Mcts2): ");
                    algorithm = GetAlgorithm(Console.ReadLine());
                }
                else if (userChoice == 5)
                {
                    Console.Write("> Timeout (ms): ");
                    if (!int.TryParse(Console.ReadLine(), out timeout))
                    {
                        timeout = 500;
                        ConsoleUtil.WriteAndWaitKey("> Invalid. Timeout must be int.");
                    }
                }
                else if (userChoice == 6)
                {
                    new SocketClient($"{serverAddress}:{serverPort}/", clientName, algorithm, timeout).Connect();
                    ConsoleUtil.WriteAndWaitKey("> Fight through Socket IO End.");
                }
            }
        }

        private static void ToolsMenu()
        {
            var userChoice = 0;
            while (true)
            {
                userChoice = GetUserChoice(
                    "Tools Menu", new[]
                    {
                        "<- Back",
                        "Convert GGF to GameRecord",
                    }, userChoice);

                if (userChoice == 0) return;
                if (userChoice == 1) ConvertGgfMenu();
            }
        }

        private static void ConvertGgfMenu()
        {
            var userChoice = 0;
            while (true)
            {
                userChoice = GetUserChoice(
                    "Convert GGF Menu", new[]
                    {
                        "<- Back",
                        $"GGF file: {_ggfFilePath}",
                        $"Save to : {_saveConvertedFilePath}",
                        "Convert Now!"
                    }, userChoice);

                if (userChoice == 0) return;
                if (userChoice == 1)
                {
                    var tempPath = (string) ConsoleUtil.Prompt("GGF file", _ggfFilePath);
                    var validExtension = FileUtils.CheckFileExtension(tempPath, ".ggf");
                    if (validExtension) _ggfFilePath = tempPath;
                    else ConsoleUtil.WriteAndWaitKey("[!] GGF file extension must be '.ggf'");
                }
                else if (userChoice == 2)
                {
                    var tempPath = (string) ConsoleUtil.Prompt("Save to", _saveConvertedFilePath);
                    var validExtension = FileUtils.CheckFileExtension(tempPath, ".txt");
                    if (validExtension) _saveConvertedFilePath = tempPath;
                    else ConsoleUtil.WriteAndWaitKey("[!] Game-record file extension must be '.txt'");
                }
                else
                {
                    try
                    {
                        var parser = new GameRecordParser();
                        parser.Parse(_ggfFilePath);
                        parser.SaveParsedGame(_saveConvertedFilePath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[!] ERROR: {e}");
                    }

                    ConsoleUtil.WriteAndWaitKey("> Convert GGF End.");
                }
            }
        }

        private static Algorithm GetAlgorithm(string algorithmName)
        {
            var name = algorithmName.ToLower();
            if (name == "mcts") return Algorithm.Mcts;
            if (name == "mcts1")
            {
                if (CheckModelReady()) return Algorithm.Mcts1;
            }
            else if (name == "mcts2")
            {
                if (CheckModelReady()) return Algorithm.Mcts2;
            }
            else Console.Write($"> Invalid algorithm name '{algorithmName}'. ");

            ConsoleUtil.WriteAndWaitKey("> Algorithm will be set default to MCTS now.");
            return Algorithm.Mcts;
        }

        private static int GetUserChoice(string header, string[] choices, int startIndex = 0)
        {
            var userChoice = startIndex;
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"------------ {header} ------------");
                for (var i = 0; i < choices.Length; i++)
                    Console.WriteLine(userChoice == i ? $"{i} [ {choices[i]} ]" : $"    {choices[i]}");

                Console.WriteLine("---------------------------------");
                switch (Console.ReadKey(false).Key)
                {
                    case ConsoleKey.UpArrow:
                        if (userChoice > 0) userChoice--;
                        break;
                    case ConsoleKey.DownArrow:
                        if (userChoice < choices.Length - 1) userChoice++;
                        break;
                    case ConsoleKey.Enter:
                        return userChoice;
                    default:
                        ConsoleUtil.WriteAndWaitKey(" <*> HELP: Press Arrow-keys to move. Enter-key to select");
                        break;
                }
            }
        }

        private static bool CheckModelReady()
        {
            if (BTMMAlgorithm.IsModelReady()) return true;
            Console.WriteLine("[!] You have to Train or Load trained data to be able to use BTMM Algorithm.");
            ConsoleUtil.WriteAndWaitKey("> Go to tab BTMM in MainMenu to Config BTMM.");
            return false;
        }
    }
}