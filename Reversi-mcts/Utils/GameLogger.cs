using System;
using System.IO;
using System.Text;

namespace Reversi_mcts.Utils
{
    public static class GameLogger
    {
        private const string FileName = "Train-log.txt";
        private static readonly StringBuilder StringBuilder = new StringBuilder();

        public static void BeginTrain(int gameCount)
        {
            // https://stackoverflow.com/a/1027274/11898496
            var time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            StringBuilder.Length = 0;
            StringBuilder.Append($"Train: {gameCount.ToString()} games - DateTime: {time}");
            StringBuilder.Append("TestPhase|LogLikelihood|AverageProbability");
        }

        public static void EndTrain()
        {
            var time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            StringBuilder.Length = 0;
            StringBuilder.Append($"End Train - DateTime: {time}");
        }

        public static void BeginTest(int loop, double likelihood, double averProb)
        {
            StringBuilder.Length = 0;
            StringBuilder.AppendFormat("{0}|{1:0.000000000000000}|{2:0.000000000000000}", loop, likelihood, averProb);
        }

        public static void WriteFile()
        {
            if (StringBuilder.Length == 0)
                return;

            var writer = new StreamWriter(FileName, true, Encoding.ASCII);
            writer.WriteLine(StringBuilder.ToString());
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