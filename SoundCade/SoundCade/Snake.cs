namespace SoundCade
{
    struct Point : IEquatable<Point>
    {
        public int X;
        public int Y;

        public Point(int x, int y) { X = x; Y = y; }

        public bool Equals(Point other) => X == other.X && Y == other.Y;
        public override bool Equals(object? obj) => obj is Point p && Equals(p);
        public override int GetHashCode() => (X * 397) ^ Y;
        public static bool operator ==(Point a, Point b) => a.Equals(b);
        public static bool operator !=(Point a, Point b) => !a.Equals(b);
        public override string ToString() => $"({X},{Y})";
    }

    public class Snake
    {
        const int defaultWidth = 40;
        const int defaultHeight = 20;

        public static void PlayGame()
        {
            bool playAgain = true;
            var rand = new Random();


            while (playAgain)
            {
                ShowMenu();
                int score = RunGame(rand);
                playAgain = ShowGameEnd(score);
            }
        }

        private static void ShowMenu()
        {
            Console.CursorVisible = true;
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkCyan;


            string[] lines =
            {
            "█████ █   █  ███  █  █  █████",
            "█     ██  █ █   █ █ █   █    ",
            "█████ █ █ █ █████ ██ █  ████ ",
            "    █ █  ██ █   █ █  █  █    ",
            "█████ █   █ █   █ █   █ █████",
            "",
            "Esc to quit while playing",
            "",
            "Press any key to start..."
            };


            int windowWidth = Console.WindowWidth;
            int windowHeight = Console.WindowHeight;
            int startRow = Math.Max(0, windowHeight / 2 - lines.Length / 2);


            for (int i = 0; i < lines.Length; i++)
            {
                int startCol = Math.Max(0, (windowWidth - lines[i].Length) / 2);
                Console.SetCursorPosition(startCol, startRow + i);
                Console.WriteLine(lines[i]);
            }

            Console.CursorVisible = false;
            Console.ReadKey(true);
            Console.ResetColor();
        }

        private static int RunGame(Random rand)
        {
            
            int width = Math.Min(defaultWidth, Math.Max(10, Console.WindowWidth - 2));
            int height = Math.Min(defaultHeight, Math.Max(5, Console.WindowHeight - 5));
            

            int offsetX = (Console.WindowWidth - (width + 2)) / 2;
            int offsetY = (Console.WindowHeight - (height + 3)) / 2;

            Console.CursorVisible = false;

            var snake = new List<Point>()
                {
                    new Point(width/2, height/2),
                    new Point(width/2 - 1, height/2),
                    new Point(width/2 - 2, height/2),
                    new Point(width/2 - 3, height/2)
                };

            int dx = 1, dy = 0;
            int score = 0;
            int speed = 150;

            Point food;
            do { food = new Point(rand.Next(width), rand.Next(height)); } while (snake.Contains(food));


            DrawBorder(width, height, offsetX, offsetY);
            DrawCell(food.X, food.Y, '*', offsetX, offsetY);
            foreach (var p in snake) DrawCell(p.X, p.Y, '█', offsetX, offsetY);

            bool running = true;
            while (running)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKey key;
                    do
                    {
                        key = Console.ReadKey(true).Key;
                    } while (Console.KeyAvailable);

                    switch (key)
                    {
                        case ConsoleKey.UpArrow:
                        case ConsoleKey.W:
                            if (dy != 1) { dx = 0; dy = -1; }
                            break;
                        case ConsoleKey.DownArrow:
                        case ConsoleKey.S:
                            if (dy != -1) { dx = 0; dy = 1; }
                            break;
                        case ConsoleKey.LeftArrow:
                        case ConsoleKey.A:
                            if (dx != 1) { dx = -1; dy = 0; }
                            break;
                        case ConsoleKey.RightArrow:
                        case ConsoleKey.D:
                            if (dx != -1) { dx = 1; dy = 0; }
                            break;
                        case ConsoleKey.Escape:
                            running = false;
                            break;
                    }
                }

                var head = snake[0];
                var newHead = new Point(head.X + dx, head.Y + dy);

                if (newHead.X < 0 || newHead.X >= width || newHead.Y < 0 || newHead.Y >= height || snake.Contains(newHead))
                {
                    running = false;
                    break;
                }

                snake.Insert(0, newHead);

                bool ateFood = newHead.Equals(food);
                if (ateFood)
                {
                    score++;
                    do { food = new Point(rand.Next(width), rand.Next(height)); } while (snake.Contains(food));
                    if (score % 3 == 0 && speed > 40) speed -= 10;
                    DrawCell(food.X, food.Y, '*', offsetX, offsetY);
                }
                else
                {
                    var tail = snake[snake.Count - 1];
                    DrawCell(tail.X, tail.Y, ' ', offsetX, offsetY);
                    snake.RemoveAt(snake.Count - 1);
                }

                if (snake.Count > 1) DrawCell(snake[1].X, snake[1].Y, '█', offsetX, offsetY);
                DrawCell(newHead.X, newHead.Y, '█', offsetX, offsetY);

                Console.SetCursorPosition(offsetX, offsetY + height + 2);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"Score: {score} Speed: {speed}ms");

                Thread.Sleep(speed);
            }
            return score;
        }

        private static bool ShowGameEnd(int score)
        {
            Console.CursorVisible = true;
            Console.Clear();

            string[] lines =
            {
            " ████  ███  █   █ █████      ███  █   █ █████ ████   █",
            "█     █   █ ██ ██ █         █   █ █   █ █     █   █  █",
            "█  ██ █████ █ █ █ ████      █   █  █ █  ████  ████   █",
            "█   █ █   █ █   █ █         █   █  █ █  █     █   █  ",
            " ███  █   █ █   █ █████      ███    █   █████ █   █  █",
            "",
            $"Final score: {score}",
            "Play again? (Y/N)"
            };

            int windowWidth = Console.WindowWidth;
            int windowHeight = Console.WindowHeight;
            int startRow = Math.Max(0, windowHeight / 2 - lines.Length / 2);


            for (int i = 0; i < lines.Length; i++)
            {
                int startCol = Math.Max(0, (windowWidth - lines[i].Length) / 2);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.SetCursorPosition(startCol, startRow + i);
                Console.WriteLine(lines[i]);
            }

            Console.CursorVisible = false;

            ConsoleKey k;
            do { k = Console.ReadKey(true).Key; } while (k != ConsoleKey.Y && k != ConsoleKey.N && k != ConsoleKey.Escape);


            return (k == ConsoleKey.Y);
        }
        private static void DrawBorder(int width, int height, int offsetX, int offsetY)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.SetCursorPosition(offsetX, offsetY);
            Console.Write('╔' + new string('═', width) + '╗');
            for (int row = 0; row < height; row++)
            {
                Console.SetCursorPosition(offsetX, offsetY + row + 1);
                Console.Write('║' + new string(' ', width) + '║');
            }
            Console.SetCursorPosition(offsetX, offsetY + height + 1);
            Console.Write('╚' + new string('═', width) + '╝');
            Console.ResetColor();
        }

        private static void DrawCell(int x, int y, char c, int offsetX, int offsetY)
        {
            Console.SetCursorPosition(x + 1 + offsetX, y + 1 + offsetY);
            if (c == '█') Console.ForegroundColor = ConsoleColor.DarkGreen; // Snake
            else if (c == '*') Console.ForegroundColor = ConsoleColor.Red; // Apple
            else Console.ForegroundColor = ConsoleColor.Gray; // Just incase :eyes:
            Console.Write(c);
            Console.ResetColor();
        }
    }
}

