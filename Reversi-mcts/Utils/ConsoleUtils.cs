using System;
using System.Threading;

namespace Reversi_mcts.Utils
{
    static class ConsoleUtils
    {
        private const char Block = '■';
        private const string Back = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";
        private const string Twirl = "-\\|/";

        public static void RunTest()
        {
            for (var i = 0; i <= 100; ++i)
            {
                DrawTextProgressBar(i, 100);
                Thread.Sleep(50);
            }
        }
        
        // https://stackoverflow.com/q/24918768/11898496
        private static void DrawTextProgressBar(int progress, int total)
        {
            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i < onechunk * progress; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw unfilled part
            for (int i = position; i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }
    }
}