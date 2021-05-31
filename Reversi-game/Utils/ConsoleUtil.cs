using System;

namespace Reversi_mcts.Utils
{
    public static class ConsoleUtil
    {
        public static void WriteAndWaitKey(object o)
        {
            Console.WriteLine(o);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static object Prompt(string text, object defaultValue)
        {
            Console.WriteLine($"> Current is: {defaultValue}");
            Console.Write($"> {text} (let empty to use current): ");
            var userInout = Console.ReadLine();
            return userInout == string.Empty ? defaultValue : userInout;
        }

        // https://stackoverflow.com/a/8946847/11898496
        public static void ClearCurrentConsoleLine()
        {
            var currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}