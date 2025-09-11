namespace SoundCade
{
    public class Pong
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        const int defaultWidth = 50;
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

            int paddleSize = 4;
            int leftPaddleY = height / 2 - paddleSize / 2;
            int rightPaddleY = height / 2 - paddleSize / 2;
            int leftX = 1;
            int rightX = width - 2;

            int ballX = width / 2;
            int ballY = height / 2;
            int ballDx = rand.Next(0, 2) == 0 ? -1 : 1;
            int ballDy = rand.Next(0, 2) == 0 ? -1 : 1;

            int scoreLeft = 0;
            int scoreRight = 0;

            DrawBorder(width, height, offsetX, offsetY);

            for (int y = 0; y < height; y++)
            {
                DrawCell(leftX, y, (y >= leftPaddleY && y < leftPaddleY + paddleSize) ? '█' : ' ', offsetX, offsetY);
                DrawCell(rightX, y, (y >= rightPaddleY && y < rightPaddleY + paddleSize) ? '█' : ' ', offsetX, offsetY);
            }

            DrawCell(ballX, ballY, 'O', offsetX, offsetY);

            bool running = true;
            HashSet<ConsoleKey> heldKeys = new();
            int delay = 50;

            while (running)
            {
                while (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    heldKeys.Add(key);
                }

                if (heldKeys.Contains(ConsoleKey.W) && leftPaddleY > 0) leftPaddleY--;
                if (heldKeys.Contains(ConsoleKey.S) && leftPaddleY + paddleSize < height) leftPaddleY++;
                if (heldKeys.Contains(ConsoleKey.UpArrow) && rightPaddleY > 0) rightPaddleY--;
                if (heldKeys.Contains(ConsoleKey.DownArrow) && rightPaddleY + paddleSize < height) rightPaddleY++;

                if (heldKeys.Contains(ConsoleKey.Escape)) return 0;

                heldKeys.RemoveWhere(k => (GetAsyncKeyState((int)k) & 0x8000) == 0);

                DrawCell(ballX, ballY, ' ', offsetX, offsetY);

                ballX += ballDx;
                ballY += ballDy;

                if (ballY <= 0 || ballY >= height - 1) ballDy *= -1;

                if (ballX == leftX + 1 && ballY >= leftPaddleY && ballY < leftPaddleY + paddleSize) ballDx = 1;
                if (ballX == rightX - 1 && ballY >= rightPaddleY && ballY < rightPaddleY + paddleSize) ballDx = -1;

                if (ballX <= 0)
                {
                    scoreRight++;
                    ballX = width / 2;
                    ballY = height / 2;
                    ballDx = 1;
                }
                else if (ballX >= width - 1)
                {
                    scoreLeft++;
                    ballX = width / 2;
                    ballY = height / 2;
                    ballDx = -1;
                }

                for (int y = 0; y < height; y++)
                {
                    DrawCell(leftX, y, (y >= leftPaddleY && y < leftPaddleY + paddleSize) ? '█' : ' ', offsetX, offsetY);
                    DrawCell(rightX, y, (y >= rightPaddleY && y < rightPaddleY + paddleSize) ? '█' : ' ', offsetX, offsetY);
                }

                DrawCell(ballX, ballY, 'O', offsetX, offsetY);

                Console.SetCursorPosition(offsetX, offsetY - 1);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"Player 1: {scoreLeft}   Player 2: {scoreRight}   ");
                Console.ResetColor();

                int winScore = 5;
                if (scoreLeft == winScore || scoreRight == winScore)
                {
                    return scoreLeft == winScore ? 1 : 2;
                }

                Thread.Sleep(delay);
            }

            return 0;
        }

        private static bool ShowGameEnd(int winner)
        {
            Console.CursorVisible = true;
            Console.Clear();

            string[] lines = [];
            switch (winner)
            {
                case 0:
                default:
                    lines =
                    [
                    " ████  ███  █   █ █████      ███  █   █ █████ ████   █",
                    "█     █   █ ██ ██ █         █   █ █   █ █     █   █  █",
                    "█  ██ █████ █ █ █ ████      █   █  █ █  ████  ████   █",
                    "█   █ █   █ █   █ █         █   █  █ █  █     █   █   ",
                    " ███  █   █ █   █ █████      ███    █   █████ █   █  █",
                    "",
                    "Play Again? (Y/N)"
                    ];
                    break;

                case 1:
                    lines =
                    [
                        "████  █      ███  █   █ █████ ████     ███  █   █ █████    █   █ █████ █   █ █████  █",
                        "█   █ █     █   █  █ █  █     █   █   █   █ ██  █ █        █   █   █   ██  █ █      █",
                        "████  █     █████   █   ████  ████    █   █ █ █ █ ████     █ █ █   █   █ █ █ █████  █",
                        "█     █     █   █   █   █     █   █   █   █ █  ██ █        ██ ██   █   █  ██     █   ",
                        "█     █████ █   █   █   █████ █   █    ███  █   █ █████    █   █ █████ █   █ █████  █",
                        "",
                        "Play Again? (Y/N)"
                    ];
                    break;

                case 2:
                    lines =
                    [
                        "████  █      ███  █   █ █████ ████    █████ █   █  ███    █   █ █████ █   █ █████  █",
                        "█   █ █     █   █  █ █  █     █   █     █   █   █ █   █   █   █   █   ██  █ █      █",
                        "████  █     █████   █   ████  ████      █   █ █ █ █   █   █ █ █   █   █ █ █ █████  █",
                        "█     █     █   █   █   █     █   █     █   ██ ██ █   █   ██ ██   █   █  ██     █   ",
                        "█     █████ █   █   █   █████ █   █     █   █   █  ███    █   █ █████ █   █ █████  █",
                        "",
                        "Play Again? (Y/N)"
                    ];
                    break;
            }

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
            if (c == '█') Console.ForegroundColor = ConsoleColor.DarkCyan; // Paddle
            else if (c == 'O') Console.ForegroundColor = ConsoleColor.White; // Ball
            else Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(c);
            Console.ResetColor();
        }
    }
}
