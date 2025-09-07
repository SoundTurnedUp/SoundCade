namespace SoundCade
{
    using System;

    class Program
    {
        public static void Main(string[] args)
        {
            Console.CursorVisible = true;
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkCyan;

            string[] title =
            {
            "█████  ███  █   █ █   █ ████   ████  ███  ████  █████",
            "█     █   █ █   █ ██  █ █   █ █     █   █ █   █ █    ",
            "█████ █   █ █   █ █ █ █ █   █ █     █████ █   █ ████ ",
            "    █ █   █ █   █ █  ██ █   █ █     █   █ █   █ █    ",
            "█████  ███  █████ █   █ ████   ████ █   █ ████  █████"
            };

            string[] menuItems = { "SNAKE", "PONG" };

            int windowWidth = Console.WindowWidth;
            int windowHeight = Console.WindowHeight;
            int startRow = Math.Max(0, windowHeight / 2 - (title.Length + menuItems.Length + 4) / 2);

            foreach (var line in title)
            {
                int startCol = Math.Max(0, (windowWidth - line.Length) / 2);
                Console.SetCursorPosition(startCol, startRow++);
                Console.WriteLine(line);
            }

            Console.ResetColor();

            int selectedIndex = 0;
            bool running = true;

            while (running)
            {
                for (int i = 0; i < menuItems.Length + 3; i++)
                {
                    Console.SetCursorPosition(0, startRow + i);
                    Console.Write(new string(' ', windowWidth));
                }

                string header = "== GAMES AVAILABLE ==";
                int headerCol = (windowWidth - header.Length) / 2;
                Console.SetCursorPosition(headerCol, startRow);
                Console.WriteLine(header);

                string[] display = menuItems.Select(m => "= " + m).ToArray();
                int maxLen = display.Max(s => s.Length);
                int itemsStartCol = Math.Max(0, (windowWidth - maxLen) / 2);

                for (int i = 0; i < display.Length; i++)
                {
                    Console.SetCursorPosition(itemsStartCol, startRow + 2 + i);


                    string text = display[i].PadRight(maxLen);


                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkCyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        Console.ResetColor();
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                    }


                    Console.WriteLine(text);
                }

                Console.ResetColor();

                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex - 1 + menuItems.Length) % menuItems.Length;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex + 1) % menuItems.Length;
                        break;
                    case ConsoleKey.Enter:
                        if (menuItems[selectedIndex] == "SNAKE")
                        {
                            Console.Beep(6000, 400);
                            Snake.PlayGame();
                        }
                        else if (menuItems[selectedIndex] == "PONG")
                        {
                            Console.Beep(6000, 400);
                            Pong.PlayGame();
                        }
                        running = false;
                        break;
                }
            }
        }
    }
}