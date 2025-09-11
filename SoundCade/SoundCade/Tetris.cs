namespace SoundCade
{
    public class Tetris
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        const int defaultWidth = 30;
        const int defaultHeight = 40;

        private static readonly Dictionary<string, (int[,], ConsoleColor)> Tetrominoes = new()
        {
            { "I", (new int[,] { {1,1,1,1} }, ConsoleColor.Cyan) },
            { "O", (new int[,] { {1,1},{1,1} }, ConsoleColor.Yellow) },
            { "T", (new int[,] { {0,1,0},{1,1,1} }, ConsoleColor.Magenta) },
            { "S", (new int[,] { {0,1,1},{1,1,0} }, ConsoleColor.Green) },
            { "Z", (new int[,] { {1,1,0},{0,1,1} }, ConsoleColor.Red) },
            { "J", (new int[,] { {1,0,0},{1,1,1} }, ConsoleColor.Blue) },
            { "L", (new int[,] { {0,0,1},{1,1,1} }, ConsoleColor.DarkYellow) },
        };

        public static void PlayGame()
        {
            bool playAgain = true;
            var rand = new Random();


            while (playAgain)
            {
                ShowMenu();
                int score = RunGame(rand);
                BackgroundMusic.StopMusic();
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
            [
               "█████ █████ █████ ████  █████ █████",
               "  █   █       █   █   █   █   █    ",
               "  █   ████    █   ████    █   █████",
               "  █   █       █   █   █   █       █",
               "  █   █████   █   █   █ █████ █████",
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

            Console.CursorVisible = false;
            Console.ReadKey(true);
            Console.ResetColor();
        }

        private static int RunGame(Random rand)
        {
            int width = Math.Min(defaultWidth, Console.WindowWidth - 2);
            int height = Math.Min(defaultHeight, Console.WindowHeight - 5);

            int offsetX = (Console.WindowWidth - (width + 2)) / 2;
            int offsetY = (Console.WindowHeight - (height + 3)) / 2;

            Console.CursorVisible = false;

            int[,] board = new int[height, width];
            ConsoleColor[,] colorBoard = new ConsoleColor[height, width];
            int score = 0;
            int linesCleared = 0;
            int speed = 500;

            Console.Clear();
            DrawBorder(width, height, offsetX, offsetY);

            BackgroundMusic.StartTetrisMusic();

            Tetromino current = SpawnRandomTetromino(rand, width);

            bool running = true;
            DateTime lastFall = DateTime.Now;

            HashSet<ConsoleKey> heldKeys = new();
            Dictionary<ConsoleKey, DateTime> nextRepeat = new();

            int initialDelay = 150;
            int repeatRate = 50;  

            while (running)
            {
                // input stuff
                while (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);
                    var key = keyInfo.Key;

                    if (!heldKeys.Contains(key))
                    {
                        heldKeys.Add(key);
                        nextRepeat[key] = DateTime.Now.AddMilliseconds(initialDelay);

                        HandleKeyPress(board, current, key, ref running);
                    }
                }

                foreach (var key in heldKeys.ToList())
                {
                    if ((GetAsyncKeyState((int)key) & 0x8000) == 0)
                    {
                        heldKeys.Remove(key);
                        nextRepeat.Remove(key);
                    }
                    else if (DateTime.Now >= nextRepeat[key])
                    {
                        HandleKeyPress(board, current, key, ref running);
                        nextRepeat[key] = DateTime.Now.AddMilliseconds(repeatRate);
                    }
                }

                // Gravity
                if ((DateTime.Now - lastFall).TotalMilliseconds >= speed)
                {
                    lastFall = DateTime.Now;

                    if (!Collides(board, current, 0, 1))
                    {
                        current.Y++;
                    }
                    else
                    {
                        Merge(board, colorBoard, current);

                        int cleared = ClearLines(board, colorBoard);
                        if (cleared > 0)
                        {
                            linesCleared += cleared;
                            score += cleared switch { 1 => 100, 2 => 300, 3 => 500, 4 => 800, _ => 0 };

                            if (linesCleared % 10 == 0 && speed > 100)
                                speed -= 50;
                        }

                        current = SpawnRandomTetromino(rand, width);
                        if (Collides(board, current, 0, 0))
                            running = false;
                    }
                }

                DrawBoard(board, colorBoard, current, width, height, offsetX, offsetY, score, linesCleared, speed);

                Thread.Sleep(30);
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

        private static Tetromino SpawnRandomTetromino(Random rand, int boardWidth)
        {
            var keys = new List<string>(Tetrominoes.Keys);
            string shape = keys[rand.Next(keys.Count)];
            var (matrix, color) = Tetrominoes[shape];

            int shapeWidth = matrix.GetLength(1);
            int startX = boardWidth / 2 - shapeWidth / 2;
            return new Tetromino(matrix, color, startX, -GetTopPadding(matrix));
        }

        private static int GetTopPadding(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < matrix.GetLength(1); c++)
                    if (matrix[r, c] == 1) return r;
            }
            return 0;
        }

        private static bool Collides(int[,] board, Tetromino block, int dx, int dy)
        {
            int boardH = board.GetLength(0);
            int boardW = board.GetLength(1);

            for (int y = 0; y < block.Shape.GetLength(0); y++)
            {
                for (int x = 0; x < block.Shape.GetLength(1); x++)
                {
                    if (block.Shape[y, x] == 0) continue;

                    int newX = block.X + x + dx;
                    int newY = block.Y + y + dy;

                    if (newX < 0 || newX >= boardW) return true;
                    if (newY >= boardH) return true;
                    if (newY >= 0 && board[newY, newX] == 1) return true;
                }
            }
            return false;
        }

        private static void Merge(int[,] board, ConsoleColor[,] colorBoard, Tetromino block)
        {
            for (int y = 0; y < block.Shape.GetLength(0); y++)
            {
                for (int x = 0; x < block.Shape.GetLength(1); x++)
                {
                    if (block.Shape[y, x] == 1)
                    {
                        int by = block.Y + y;
                        int bx = block.X + x;
                        if (by >= 0 && by < board.GetLength(0) && bx >= 0 && bx < board.GetLength(1))
                        {
                            board[by, bx] = 1;
                            colorBoard[by, bx] = block.Color;
                        }
                    }
                }
            }
        }

        private static int ClearLines(int[,] board, ConsoleColor[,] colorBoard)
        {
            int width = board.GetLength(1);
            int height = board.GetLength(0);
            int cleared = 0;

            for (int y = height - 1; y >= 0; y--)
            {
                bool full = true;
                for (int x = 0; x < width; x++)
                {
                    if (board[y, x] == 0) { full = false; break; }
                }

                if (full)
                {
                    cleared++;
                    for (int yy = y; yy > 0; yy--)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            board[yy, x] = board[yy - 1, x];
                            colorBoard[yy, x] = colorBoard[yy - 1, x];
                        }
                    }
                    for (int x = 0; x < width; x++)
                    {
                        board[0, x] = 0;
                        colorBoard[0, x] = default;
                    }
                    y++; 
                }
            }
            return cleared;
        }

        private static void DrawBoard(int[,] board, ConsoleColor[,] colourBoard, Tetromino current, int width, int height, int offsetX, int offsetY, int score, int lines, int speed)
        {
            for (int y = 0; y < height; y++)
            {
                Console.SetCursorPosition(offsetX + 1, offsetY + 1 + y);
                for (int x = 0; x < width; x++)
                {
                    if (TryGetBlockColour(board, colourBoard, current, x, y, out var c))
                    {
                        Console.ForegroundColor = c;
                        Console.Write('█');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
            }

            Console.SetCursorPosition(offsetX, offsetY + height + 2);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"Score: {score}  Lines: {lines}");
            Console.ResetColor();
        }

        private static bool TryGetBlockColour(int[,] board, ConsoleColor[,] colourBoard, Tetromino current, int x, int y, out ConsoleColor colour)
        {
            for (int cy = 0; cy < current.Shape.GetLength(0); cy++)
            {
                for (int cx = 0; cx < current.Shape.GetLength(1); cx++)
                {
                    if (current.Shape[cy, cx] == 1)
                    {
                        int px = current.X + cx;
                        int py = current.Y + cy;
                        if (px == x && py == y)
                        {
                            colour = current.Color;
                            return true;
                        }
                    }
                }
            }

            if (y >= 0 && y < board.GetLength(0) && x >= 0 && x < board.GetLength(1) && board[y, x] == 1)
            {
                colour = colourBoard[y, x];
                return true;
            }

            colour = default;
            return false;
        }

        private static void HandleKeyPress(int[,] board, Tetromino current, ConsoleKey key, ref bool running)
        {
            switch (key)
            {
                case ConsoleKey.LeftArrow:
                    if (!Collides(board, current, -1, 0)) current.X--;
                    break;
                case ConsoleKey.RightArrow:
                    if (!Collides(board, current, 1, 0)) current.X++;
                    break;
                case ConsoleKey.DownArrow:
                    if (!Collides(board, current, 0, 1)) current.Y++;
                    break;
                case ConsoleKey.UpArrow:
                    current.Rotate();
                    if (Collides(board, current, 0, 0)) current.RotateBack();
                    break;
                case ConsoleKey.Spacebar:
                    while (!Collides(board, current, 0, 1)) current.Y++;
                    break;
                case ConsoleKey.Escape:
                    running = false;
                    break;
            }
        }
    }

    public class Tetromino
    {
        public int[,] Shape { get; private set; }
        public ConsoleColor Color { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Tetromino(int[,] shape, ConsoleColor color, int startX, int startY)
        {
            Shape = (int[,])shape.Clone();
            Color = color;
            X = startX;
            Y = startY;
        }

        public void Rotate()
        {
            int rows = Shape.GetLength(0);
            int cols = Shape.GetLength(1);
            int[,] rotated = new int[cols, rows];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    rotated[c, rows - 1 - r] = Shape[r, c];
            Shape = rotated;
        }

        public void RotateBack()
        {
            Rotate();
            Rotate();
            Rotate();
        }
    }
}
