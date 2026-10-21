using System;
using System.Linq;
using GamesData;
using GamesModels;

namespace GamesApp
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("\tИгры");
                Console.WriteLine("1. Все игры");
                Console.WriteLine("2. Поиск по названию игры");
                Console.WriteLine("3. Поиск по студии");
                Console.WriteLine("4. Поиск по студии и названию");
                Console.WriteLine("5. Поиск по стилю");
                Console.WriteLine("6. Поиск по году релиза");
                Console.WriteLine("7. Однопользовательские игры");
                Console.WriteLine("8. Многопользовательские игры");
                Console.WriteLine("9. Игра с максимальными продажами");
                Console.WriteLine("10. Игра с минимальными продажами");
                Console.WriteLine("11. Топ-3 продаваемых игр");
                Console.WriteLine("12. Топ-3 непродаваемых игр");
                Console.WriteLine("13. Добавить игру");
                Console.WriteLine("14. Изменить игру");
                Console.WriteLine("15. Удалить игру");
                Console.WriteLine("0. Выход");
                Console.Write("Выбор: ");

                switch (Console.ReadLine())
                {
                    case "1": ShowAll(); break;
                    case "2": SearchByTitle(); break;
                    case "3": SearchByStudio(); break;
                    case "4": SearchByStudioAndTitle(); break;
                    case "5": SearchByGenre(); break;
                    case "6": SearchByYear(); break;
                    case "7": ShowSingleplayer(); break;
                    case "8": ShowMultiplayer(); break;
                    case "9": ShowMaxSold(); break;
                    case "10": ShowMinSold(); break;
                    case "11": ShowTop3Sold(); break;
                    case "12": ShowTop3NotSold(); break;
                    case "13": AddGame(); break;
                    case "14": UpdateGame(); break;
                    case "15": DeleteGame(); break;
                    case "0": return;
                }
                Console.WriteLine("\nНажмите любую клавишу...");
                Console.ReadKey();
            }
        }

        static void ShowAll()
        {
            using var db = new GamesContext();
            foreach (var g in db.Games.ToList())
                Console.WriteLine($"{g.Id}. {g.Title} | {g.Studio} | {g.Genre} | {g.ReleaseDate:yyyy-MM-dd} | {g.GameMode} | Продано: {g.CopiesSold}");
        }

        static void SearchByTitle()
        {
            Console.Write("Название игры: ");
            string title = Console.ReadLine();
            using var db = new GamesContext();
            var games = db.Games.Where(g => g.Title.Contains(title)).ToList();
            PrintGames(games);
        }

        static void SearchByStudio()
        {
            Console.Write("Студия: ");
            string studio = Console.ReadLine();
            using var db = new GamesContext();
            var games = db.Games.Where(g => g.Studio.Contains(studio)).ToList();
            PrintGames(games);
        }

        static void SearchByStudioAndTitle()
        {
            Console.Write("Студия: ");
            string studio = Console.ReadLine();
            Console.Write("Название: ");
            string title = Console.ReadLine();
            using var db = new GamesContext();
            var games = db.Games.Where(g => g.Studio.Contains(studio) && g.Title.Contains(title)).ToList();
            PrintGames(games);
        }

        static void SearchByGenre()
        {
            Console.Write("Стиль игры: ");
            string genre = Console.ReadLine();
            using var db = new GamesContext();
            var games = db.Games.Where(g => g.Genre.Contains(genre)).ToList();
            PrintGames(games);
        }

        static void SearchByYear()
        {
            Console.Write("Год релиза: ");
            int year = int.Parse(Console.ReadLine());
            using var db = new GamesContext();
            var games = db.Games.Where(g => g.ReleaseDate.Year == year).ToList();
            PrintGames(games);
        }

        static void ShowSingleplayer()
        {
            using var db = new GamesContext();
            var games = db.Games.Where(g => g.GameMode == "Singleplayer").ToList();
            PrintGames(games);
        }

        static void ShowMultiplayer()
        {
            using var db = new GamesContext();
            var games = db.Games.Where(g => g.GameMode == "Multiplayer").ToList();
            PrintGames(games);
        }

        static void ShowMaxSold()
        {
            using var db = new GamesContext();
            var game = db.Games.OrderByDescending(g => g.CopiesSold).FirstOrDefault();
            if (game != null)
                Console.WriteLine($"{game.Title} — {game.CopiesSold} копий");
        }

        static void ShowMinSold()
        {
            using var db = new GamesContext();
            var game = db.Games.OrderBy(g => g.CopiesSold).FirstOrDefault();
            if (game != null)
                Console.WriteLine($"{game.Title} — {game.CopiesSold} копий");
        }

        static void ShowTop3Sold()
        {
            using var db = new GamesContext();
            var games = db.Games.OrderByDescending(g => g.CopiesSold).Take(3).ToList();
            PrintGames(games);
        }

        static void ShowTop3NotSold()
        {
            using var db = new GamesContext();
            var games = db.Games.OrderBy(g => g.CopiesSold).Take(3).ToList();
            PrintGames(games);
        }

        static void AddGame()
        {
            Console.Write("Название: ");
            string title = Console.ReadLine();
            Console.Write("Студия: ");
            string studio = Console.ReadLine();

            using var db = new GamesContext();
            if (db.Games.Any(g => g.Title == title && g.Studio == studio))
            {
                Console.WriteLine("Такая игра уже существует!");
                return;
            }

            Console.Write("Стиль: ");
            string genre = Console.ReadLine();
            Console.Write("Дата релиза (yyyy-MM-dd): ");
            DateTime date = DateTime.Parse(Console.ReadLine());
            Console.Write("Режим (Singleplayer/Multiplayer): ");
            string mode = Console.ReadLine();
            Console.Write("Продано копий: ");
            int sold = int.Parse(Console.ReadLine());

            var game = new Game
            {
                Title = title,
                Studio = studio,
                Genre = genre,
                ReleaseDate = date,
                GameMode = mode,
                CopiesSold = sold
            };
            db.Games.Add(game);
            db.SaveChanges();
            Console.WriteLine("Игра добавлена.");
        }

        static void UpdateGame()
        {
            Console.Write("Название игры для изменения: ");
            string title = Console.ReadLine();
            using var db = new GamesContext();
            var game = db.Games.FirstOrDefault(g => g.Title == title);
            if (game == null)
            {
                Console.WriteLine("Игра не найдена.");
                return;
            }

            Console.WriteLine("Что изменить? (1-название, 2-студия, 3-стиль, 4-дата, 5-режим, 6-продажи)");
            string choice = Console.ReadLine();
            Console.Write("Новое значение: ");
            string value = Console.ReadLine();

            switch (choice)
            {
                case "1": game.Title = value; break;
                case "2": game.Studio = value; break;
                case "3": game.Genre = value; break;
                case "4": game.ReleaseDate = DateTime.Parse(value); break;
                case "5": game.GameMode = value; break;
                case "6": game.CopiesSold = int.Parse(value); break;
            }
            db.SaveChanges();
            Console.WriteLine("Данные обновлены.");
        }

        static void DeleteGame()
        {
            Console.Write("Название игры: ");
            string title = Console.ReadLine();
            Console.Write("Студия: ");
            string studio = Console.ReadLine();

            using var db = new GamesContext();
            var game = db.Games.FirstOrDefault(g => g.Title == title && g.Studio == studio);
            if (game == null)
            {
                Console.WriteLine("Игра не найдена.");
                return;
            }

            Console.Write($"Вы уверены, что хотите удалить '{game.Title}'? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                db.Games.Remove(game);
                db.SaveChanges();
                Console.WriteLine("Игра удалена.");
            }
            else
            {
                Console.WriteLine("Удаление отменено.");
            }
        }

        static void PrintGames(System.Collections.Generic.List<Game> games)
        {
            if (games.Count == 0)
            {
                Console.WriteLine("Ничего не найдено.");
                return;
            }
            foreach (var g in games)
                Console.WriteLine($"{g.Title} | {g.Studio} | {g.Genre} | {g.ReleaseDate:yyyy-MM-dd} | {g.GameMode} | Продано: {g.CopiesSold}");
        }
    }
}