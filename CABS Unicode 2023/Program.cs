using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.Threading;
using System.Drawing;
using System.Drawing.Printing;
using static System.Net.WebRequestMethods;
using System.Net.WebSockets;
using System.Dynamic;
using System.Runtime.Serialization;

namespace SpaceGame
{
    class Program
    {
        static string filePath;
        static int consoleWidth = 80;
        static int consoleHeight = 25;
        static int spaceshipWidth = 3;
        static int spaceshipHeight = 3;
        static int spaceshipX;
        static int spaceshipY;
        static List<int> meteorXPositions;
        static List<int> meteorYPositions;
        static List<int> starXPositions;
        static List<int> starYPositions;
        static Random random;
        static bool gameOver;
        private const int SPI_SETKEYBOARDDELAY = 0x0017;
        private const int SPI_SETKEYBOARDSPEED = 0x000B;
        private const int SPIF_SENDCHANGE = 0x0002;
        static int delay = 100;
        static int meteorPercentage = 35;
        static int starPercentage = 75;
        static string unicode = "\uD83D\uDE80";
        static string obstacle = "\u2730";
        static int score = 0;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SystemParametersInfo(int uAction, int uParam, ref int lpvParam, int fuWinIni);

        public static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            PrintGame();
            Getdifficulty();
            GetMusicLevel();
            PlayMusic(filePath);
            LaunchScreen();
            Console.Clear();

            int keyboardDelay = 0;
            SystemParametersInfo(SPI_SETKEYBOARDDELAY, keyboardDelay, ref keyboardDelay, SPIF_SENDCHANGE);
            int speed = 31;
            SystemParametersInfo(SPI_SETKEYBOARDSPEED, speed, ref speed, SPIF_SENDCHANGE);
            InitializeGame();
            while (!gameOver)
            {
                score++;

                HandleInput();
                Update();
                Render();
                System.Threading.Thread.Sleep(delay);
            }
            Console.SetCursorPosition(0, consoleHeight + 1);
            GameOverScreen();
        }
        static void PlayMusic(string filpath)
        {
            SoundPlayer soundPlayer = new SoundPlayer(filpath);
            soundPlayer.PlayLooping();
        }
        static void InitializeGame()
        {
            spaceshipX = consoleWidth / 2 - spaceshipWidth / 2;
            spaceshipY = consoleHeight - spaceshipHeight - 1;
            meteorXPositions = new List<int>();
            meteorYPositions = new List<int>();
            starXPositions = new List<int>();
            starYPositions = new List<int>();
            random = new Random();
            gameOver = false;
        }


        static void HandleInput()
        {
            while (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.A or ConsoleKey.LeftArrow:
                        spaceshipX = Math.Max(0, spaceshipX - 1);
                        break;
                    case ConsoleKey.D or ConsoleKey.RightArrow:
                        spaceshipX = Math.Min(consoleWidth - spaceshipWidth, spaceshipX + 1);
                        break;
                    case ConsoleKey.Escape:
                        gameOver = true;
                        break;
                    case ConsoleKey.Enter:
                        Console.ReadLine();
                        break;
                }
            }
        }
        static void GameOverScreen()
        {
            Console.Clear();
            Console.WriteLine("\n\n\n\n\n\n                           ██████╗  █████╗ ███╗   ███╗███████╗     ██████╗ ██╗   ██╗███████╗██████╗ ");
            Console.WriteLine("                          ██╔════╝ ██╔══██╗████╗ ████║██╔════╝    ██╔═══██╗██║   ██║██╔════╝██╔══██╗");
            Console.WriteLine("                          ██║  ███╗███████║██╔████╔██║█████╗      ██║   ██║██║   ██║█████╗  ██████╔╝");
            Console.WriteLine("                          ██║   ██║██╔══██║██║╚██╔╝██║██╔══╝      ██║   ██║╚██╗ ██╔╝██╔══╝  ██╔══██╗");
            Console.WriteLine("                          ╚██████╔╝██║  ██║██║ ╚═╝ ██║███████╗    ╚██████╔╝ ╚████╔╝ ███████╗██║  ██║");
            Console.WriteLine("                           ╚═════╝ ╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝     ╚═════╝   ╚═══╝  ╚══════╝╚═╝  ╚═╝");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"                                   Du hast verloren. Dein Score liegt bei {score} Punkten");


        }
        static void Update()
        {
            for (int i = 0; i < meteorXPositions.Count; i++)
            {
                meteorYPositions[i]++;
                if (meteorYPositions[i] >= consoleHeight)
                {
                    meteorXPositions.RemoveAt(i);
                    meteorYPositions.RemoveAt(i);
                    i--;
                }
                else if (meteorYPositions[i] >= spaceshipY && meteorYPositions[i] < spaceshipY + spaceshipHeight &&
                         (meteorXPositions[i] == spaceshipX || meteorXPositions[i] + 1 == spaceshipX || meteorXPositions[i] + 2 == spaceshipX))
                {
                    gameOver = true; // Collision with meteor
                }
            }

            for (int i = 0; i < starXPositions.Count; i++)
            {
                starYPositions[i]++;
                if (starYPositions[i] == spaceshipY && starXPositions[i] >= spaceshipX && starXPositions[i] < spaceshipX + spaceshipWidth)
                {
                    gameOver = true; // Collision with star
                }
                if (starYPositions[i] >= consoleHeight)
                {
                    starXPositions.RemoveAt(i);
                    starYPositions.RemoveAt(i);
                    i--;
                }
            }

            if (random.Next(0, 100) < meteorPercentage && meteorYPositions.Count < 3) // % chance of meteor
            {
                int meteorX = random.Next(0, consoleWidth - 2);
                meteorXPositions.Add(meteorX);
                meteorYPositions.Add(0);
                meteorXPositions.Add(meteorX + 1);
                meteorYPositions.Add(0);
                meteorXPositions.Add(meteorX + 2);
                meteorYPositions.Add(0);
            }

            if (random.Next(0, 100) < starPercentage) // % chance of star
            {
                int starX = random.Next(0, consoleWidth);
                starXPositions.Add(starX);
                starYPositions.Add(0);
            }
        }
        static void LaunchScreen()
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.Clear();
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n");
            Console.WriteLine();
            Console.WriteLine("                                  Mit A und D oder Pfeiltaste Rakete bewegen");
            Console.WriteLine();
            Console.Write("                                              Press [enter] to start...");
            Console.ReadKey();

            Console.ForegroundColor = ConsoleColor.White;
        }
        static void Render()
        {
            StringBuilder screen = new StringBuilder();

            for (int i = 0; i < consoleHeight; i++)
            {
                for (int j = 0; j < consoleWidth; j++)
                {
                    if (i == spaceshipY && j >= spaceshipX && j < spaceshipX + spaceshipWidth)
                    {
                        screen.Append(unicode); // Spaceship symbol
                    }
                    else if (meteorYPositions.Contains(i) && (meteorXPositions[meteorYPositions.IndexOf(i)] == j || meteorXPositions[meteorYPositions.IndexOf(i)] + 1 == j || meteorXPositions[meteorYPositions.IndexOf(i)] + 2 == j))
                    {
                        screen.Append("✯"); // Meteor symbol
                    }
                    else if (starYPositions.Contains(i) && starXPositions[starYPositions.IndexOf(i)] == j)
                    {
                        screen.Append(obstacle); // Star symbol
                    }
                    else
                    {
                        screen.Append(" ");
                    }
                }
                screen.AppendLine();
            }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(screen.ToString());
        }
        static void PrintGame()
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("\n\n\n                 □□□□□□□□□     □□□□□□□□     □□□□□□□    □□     □□   □□□□□□□□□□   □□□□□□□□□□□□");
            Console.WriteLine("                 □□      □□   □        □   □       □   □□    □□    □□                □□");
            Console.WriteLine("                 □□      □□   □        □   □           □□  □□      □□                □□");
            Console.WriteLine("                 □□□□□□□□□    □        □   □           □□□□□□      □□□□□□□□□□        □□");
            Console.WriteLine("                 □□    □□     □        □   □           □□□□□□      □□□□□□□□□□        □□");
            Console.WriteLine("                 □□     □□    □        □   □           □□   □□     □□                □□");
            Console.WriteLine("                 □□      □□   □        □   □       □   □□    □□    □□                □□");
            Console.WriteLine("                 □□       □□   □□□□□□□□     □□□□□□□    □□     □□   □□□□□□□□□□        □□");
            Console.WriteLine();
            Console.WriteLine("                            □□□□□□□□□          □□        □□□       □□□   □□□□□□□□□□");
            Console.WriteLine("                           □□       □□        □□□□       □□□□     □□□□   □□ ");
            Console.WriteLine("                           □□                □□  □□      □□ □□   □□ □□   □□");
            Console.WriteLine("                           □□    □□□□       □□□□□□□□     □□  □□ □□  □□   □□□□□□□□□□");
            Console.WriteLine("                           □□       □□     □□      □□    □□   □□□   □□   □□");
            Console.WriteLine("                           □□       □□    □□        □□   □□         □□   □□");
            Console.WriteLine("                            □□□□□□□□□    □□          □□  □□         □□   □□□□□□□□□□");
            Console.WriteLine();
            Console.WriteLine("                                          by Haider Sebastian");
            Console.WriteLine("");
            Console.WriteLine("                                          Weiter = Leertaste");

            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadKey();
        }
        enum music
        {
            Musik1,
            Musik2,
            Musik3
        }

        static void GetMusicLevel()
        {
            music choosenMusic = music.Musik1;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n\n\n\n\n\n\n\n\n\n                                               Musiklevel auswählen:");
                Console.WriteLine();

                foreach (music music in Enum.GetValues(typeof(music)))
                {
                    if (music == choosenMusic)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"                                                      {music}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"                                                      {music}");
                    }

                    if (choosenMusic == music.Musik1)
                    {
                        filePath = "musikDatei.wav";
                    }
                    else if (choosenMusic == music.Musik2)
                    {
                        filePath = "Geometry Dash Level 2.wav";
                    }
                    else if (choosenMusic == music.Musik3)
                    {
                        filePath = "DRY Out.wav";
                    }
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        choosenMusic = DekrementiereMusicLevel(choosenMusic);
                        break;
                    case ConsoleKey.DownArrow:
                        choosenMusic = InkrementiereMusicLevel(choosenMusic);
                        break;
                    case ConsoleKey.Enter:
                        Console.ReadKey();
                        return;
                }
            }
        }
        static music InkrementiereMusicLevel(music music)
        {
            int musicIndex = (int)music;
            musicIndex = (musicIndex + 1) % Enum.GetNames(typeof(music)).Length;
            return (music)musicIndex;
        }

        static music DekrementiereMusicLevel(music music)
        {
            int musicIndex = (int)music;
            musicIndex = (musicIndex - 1 + Enum.GetNames(typeof(music)).Length) % Enum.GetNames(typeof(music)).Length;
            return (music)musicIndex;
        }
        static void Getdifficulty()
        {
            difficultyLevel choosenMod = difficultyLevel.Einfach;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n\n\n\n\n\n\n\n\n\n                                           Schwierigkeitsmodus auswählen:");
                Console.WriteLine();

                foreach (difficultyLevel modus in Enum.GetValues(typeof(difficultyLevel)))
                {
                    if (modus == choosenMod)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"                                                      {modus}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"                                                      {modus}");
                    }

                    if (choosenMod == difficultyLevel.Schwer)
                    {
                        delay = 33;
                    }
                    else if (choosenMod == difficultyLevel.Einfach)
                    {
                        delay = 100;
                    }
                    else if (choosenMod == difficultyLevel.Mittel)
                    {
                        delay = 50;
                    }
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        choosenMod = DekrementiereSchwierigkeitsmodus(choosenMod);
                        break;
                    case ConsoleKey.DownArrow:
                        choosenMod = InkrementiereSchwierigkeitsmodus(choosenMod);
                        break;
                    case ConsoleKey.Enter:
                        Console.ReadKey();
                        return;
                }
            }
        }
        static difficultyLevel InkrementiereSchwierigkeitsmodus(difficultyLevel modus)
        {
            int modusIndex = (int)modus;
            modusIndex = (modusIndex + 1) % Enum.GetNames(typeof(difficultyLevel)).Length;
            return (difficultyLevel)modusIndex;
        }

        static difficultyLevel DekrementiereSchwierigkeitsmodus(difficultyLevel modus)
        {
            int modusIndex = (int)modus;
            modusIndex = (modusIndex - 1 + Enum.GetNames(typeof(difficultyLevel)).Length) % Enum.GetNames(typeof(difficultyLevel)).Length;
            return (difficultyLevel)modusIndex;
        }

        enum difficultyLevel
        {
            Einfach,
            Mittel,
            Schwer
        }
    }
}
