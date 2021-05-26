using System;
using System.IO;
using System.Text;

namespace Reversi_mcts.Utils
{
    public static class GameLogger
    {
        private const string FileName = "Train-log.txt";
        private static readonly StringBuilder StringBuilder = new StringBuilder();

        private static string GetDateTime()
        {
            // https://stackoverflow.com/a/1027274/11898496
            return DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        public static void WriteBeginTrain(int gameCount)
        {
            StringBuilder.Length = 0;
            StringBuilder.AppendLine($"\nTrain: {gameCount.ToString()} games - DateTime: {GetDateTime()}");
            StringBuilder.AppendLine("TestPhase|LogLikelihood|AverageProbability");
            WriteFile();
        }

        public static void WriteFinishTrain()
        {
            StringBuilder.Length = 0;
            StringBuilder.AppendLine($"\nEnd Train - DateTime: {GetDateTime()}");
            WriteFile();
        }

        public static void WriteMleLog(int loop, double likelihood, double averProb)
        {
            StringBuilder.Length = 0;
            StringBuilder.AppendFormat("\n{0}|{1:0.000000000000000}|{2:0.000000000000000}", loop, likelihood, averProb);
            WriteFile();
        }

        public static void WriteFile()
        {
            if (StringBuilder.Length == 0)
                return;

            var writer = new StreamWriter(FileName, true, Encoding.ASCII);
            writer.Write(StringBuilder.ToString());
            writer.Close();
            StringBuilder.Length = 0;
        }

        public static string ReadLog()
        {
            var file = new FileInfo(FileName);
            if (!file.Exists)
                return "";

            var reader = new StreamReader(file.FullName, Encoding.ASCII);
            return reader.ReadToEnd() + StringBuilder;
        }
    }
}