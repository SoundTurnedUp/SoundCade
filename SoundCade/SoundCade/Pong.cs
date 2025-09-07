using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundCade
{
    public class Pong
    {
        public static void PlayGame()
        {
            const int defaultWidth = 40;
            const int defaultHeight = 20;
            var rand = new Random();

            bool playAgain = true;

            while (playAgain)
            {
                Console.CursorVisible = true;
                Console.Clear();
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.DarkCyan;

                string[] lines =
                [
                    "████   ███  █   █  ████ ",
                    "█   █ █   █ ██  █ █     ",
                    "████  █   █ █ █ █ █  ██ ",
                    "█     █   █ █  ██ █   █ ",
                    "█      ███  █   █  ███  ",
                    "",
                    "Esc to quit while playing",
                    "",
                    "Press any key to start..."
                ];

                int windowWidth = Console.WindowWidth;
                int windowHeight = Console.WindowHeight;
                int startRow = Math.Max(0, windowHeight / 2 - lines.Length / 2);

                for (int i = 0; i < lines.Length; i++)
                {
                    int startCol = Math.Max(0, (windowWidth - lines[i].Length) / 2);
                    Console.SetCursorPosition(startCol, startRow + i);
                    Console.WriteLine(lines[i]);
                }

                Console.ReadKey(true);
                Console.ResetColor();

                int width = Math.Min(defaultWidth, Math.Max(10, Console.WindowWidth - 2));
                int height = Math.Min(defaultHeight, Math.Max(5, Console.WindowHeight - 5));

                int offsetX = (Console.WindowWidth - (width + 2)) / 2;
                int offsetY = (Console.WindowHeight - (height + 3)) / 2;

                Console.CursorVisible = false;


                Console.Beep(6000, 400);
            }
        }
    }
}
