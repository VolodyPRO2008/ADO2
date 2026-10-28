using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace FootballChampionship
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
                if (!db.Teams.Any()) SeedData(db);
            }

            while (true)
            {
                Console.WriteLine("\n ЧЕМПИОНАТ ИСПАНИИ ПО ФУТБОЛУ ");
                Console.WriteLine("1. Показать разницу голов команд");
                Console.WriteLine("2. Полная информация о матче");
                Console.WriteLine("3. Все матчи на дату");
                Console.WriteLine("4. Все матчи команды");
                Console.WriteLine("5. Игроки, забившие в дату");
                Console.WriteLine("6. Добавить матч");
                Console.WriteLine("7. Изменить матч");
                Console.WriteLine("8. Удалить матч");
                Console.WriteLine();
                Console.WriteLine("9. Сгенерировать случайные матчи (Задание 1)");
                Console.WriteLine("10. Топ-3 бомбардира команды (Задание 2)");
                Console.WriteLine("11. Лучший бомбардир команды");
                Console.WriteLine("12. Топ-3 бомбардира всего чемпионата");
                Console.WriteLine("13. Лучший бомбардир чемпионата");
                Console.WriteLine("14. Топ-3 команд по забитым (Задание 3)");
                Console.WriteLine("15. Команда с наибольшим числом забитых");
                Console.WriteLine("16. Топ-3 команд по пропущенным (меньше всего)");
                Console.WriteLine("17. Команда с наименьшим числом пропущенных");
                Console.WriteLine("18. Топ-3 команд по очкам (Задание 4)");
                Console.WriteLine("19. Команда с наибольшим числом очков");
                Console.WriteLine("20. Топ-3 команд с наименьшим числом очков");
                Console.WriteLine("21. Команда с наименьшим числом очков");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите действие: ");

                string choice = Console.ReadLine();
                Console.Clear();

                using (var db = new AppDbContext())
                {
                    switch (choice)
                    {
                        case "1": ShowGoalDifference(db); break;
                        case "2": ShowMatchInfo(db); break;
                        case "3": ShowMatchesByDate(db); break;
                        case "4": ShowMatchesByTeam(db); break;
                        case "5": ShowScorersByDate(db); break;
                        case "6": AddMatch(db); break;
                        case "7": EditMatch(db); break;
                        case "8": DeleteMatch(db); break;
                        case "9": GenerateMatches(db); break;
                        case "10": ShowTop3BombardiersByTeam(db); break;
                        case "11": ShowBestBombardierByTeam(db); break;
                        case "12": ShowTop3Bombardiers(db); break;
                        case "13": ShowBestBombardier(db); break;
                        case "14": ShowTop3ScoredTeams(db); break;
                        case "15": ShowBestScoredTeam(db); break;
                        case "16": ShowTop3ConcededTeams(db); break;
                        case "17": ShowBestConcededTeam(db); break;
                        case "18": ShowTop3PointsTeams(db); break;
                        case "19": ShowBestPointsTeam(db); break;
                        case "20": ShowTop3WorstPointsTeams(db); break;
                        case "21": ShowWorstPointsTeam(db); break;
                        case "0": return;
                        default: Console.WriteLine("Неверный ввод."); break;
                    }
                }
            }
        }

        //  ПРОШЛЫЕ ЗАДАНИЯ (2 и 3) 
        static void ShowGoalDifference(AppDbContext db) { /* Твой старый код */ }
        static void ShowMatchInfo(AppDbContext db) { /* Твой старый код */ }
        static void ShowMatchesByDate(AppDbContext db) { /* Твой старый код */ }
        static void ShowMatchesByTeam(AppDbContext db) { /* Твой старый код */ }
        static void ShowScorersByDate(AppDbContext db) { /* Твой старый код */ }
        static void AddMatch(AppDbContext db) { /* Твой старый код */ }
        static void EditMatch(AppDbContext db) { /* Твой старый код */ }
        static void DeleteMatch(AppDbContext db) { /* Твой старый код */ }

        //  ЗАДАНИЕ 1 
        static void GenerateMatches(AppDbContext db)
        {
            Console.Write("Сколько случайных матчей сгенерировать? ");
            int count = int.Parse(Console.ReadLine());

            // Вызов хранимой процедуры
            db.Database.ExecuteSqlRaw($"EXEC GenerateRandomMatches @MatchCount = {count}");
            Console.WriteLine($"Успешно сгенерировано {count} матчей!");
        }

        //  ЗАДАНИЕ 2 
        static void ShowTop3BombardiersByTeam(AppDbContext db)
        {
            Console.Write("Введите название команды: ");
            string team = Console.ReadLine();
            var top3 = db.Bombardiers.Where(b => b.TeamName.ToLower() == team.ToLower())
                                     .OrderByDescending(b => b.Goals).Take(3).ToList();
            Console.WriteLine($"\nТоп-3 бомбардира команды {team}:");
            foreach (var b in top3) Console.WriteLine($"{b.FullName} - {b.Goals} голов");
        }

        static void ShowBestBombardierByTeam(AppDbContext db)
        {
            Console.Write("Введите название команды: ");
            string team = Console.ReadLine();
            var best = db.Bombardiers.Where(b => b.TeamName.ToLower() == team.ToLower())
                                     .OrderByDescending(b => b.Goals).FirstOrDefault();
            if (best != null) Console.WriteLine($"\nЛучший бомбардир {team}: {best.FullName} ({best.Goals})");
            else Console.WriteLine("Игроки не найдены.");
        }

        static void ShowTop3Bombardiers(AppDbContext db)
        {
            var top3 = db.Bombardiers.OrderByDescending(b => b.Goals).Take(3).ToList();
            Console.WriteLine("\nТоп-3 бомбардира чемпионата:");
            foreach (var b in top3) Console.WriteLine($"{b.FullName} ({b.TeamName}) - {b.Goals} голов");
        }

        static void ShowBestBombardier(AppDbContext db)
        {
            var best = db.Bombardiers.OrderByDescending(b => b.Goals).FirstOrDefault();
            if (best != null) Console.WriteLine($"\nЛучший бомбардир чемпионата: {best.FullName} ({best.TeamName}) - {best.Goals} голов");
        }

        //  ЗАДАНИЕ 3 
        static void ShowTop3ScoredTeams(AppDbContext db)
        {
            var top3 = db.TeamStats.OrderByDescending(t => t.TotalScored).Take(3).ToList();
            Console.WriteLine("\nТоп-3 команд по забитым голам:");
            foreach (var t in top3) Console.WriteLine($"{t.TeamName}: {t.TotalScored}");
        }

        static void ShowBestScoredTeam(AppDbContext db)
        {
            var best = db.TeamStats.OrderByDescending(t => t.TotalScored).FirstOrDefault();
            Console.WriteLine($"\nБольше всех забила: {best?.TeamName} ({best?.TotalScored})");
        }

        static void ShowTop3ConcededTeams(AppDbContext db)
        {
            var top3 = db.TeamStats.OrderBy(t => t.TotalConceded).Take(3).ToList(); // По возрастанию (меньше пропустили)
            Console.WriteLine("\nТоп-3 команд, пропустивших меньше всего:");
            foreach (var t in top3) Console.WriteLine($"{t.TeamName}: {t.TotalConceded}");
        }

        static void ShowBestConcededTeam(AppDbContext db)
        {
            var best = db.TeamStats.OrderBy(t => t.TotalConceded).FirstOrDefault();
            Console.WriteLine($"\nМеньше всех пропустила: {best?.TeamName} ({best?.TotalConceded})");
        }

        //  ЗАДАНИЕ 4 
        static void ShowTop3PointsTeams(AppDbContext db)
        {
            var top3 = db.TeamStats.OrderByDescending(t => t.TotalPoints).Take(3).ToList();
            Console.WriteLine("\nТоп-3 команд по очкам:");
            foreach (var t in top3) Console.WriteLine($"{t.TeamName}: {t.TotalPoints} очков");
        }

        static void ShowBestPointsTeam(AppDbContext db)
        {
            var best = db.TeamStats.OrderByDescending(t => t.TotalPoints).FirstOrDefault();
            Console.WriteLine($"\nБольше всех очков набрала: {best?.TeamName} ({best?.TotalPoints})");
        }

        static void ShowTop3WorstPointsTeams(AppDbContext db)
        {
            var top3 = db.TeamStats.OrderBy(t => t.TotalPoints).Take(3).ToList();
            Console.WriteLine("\nТоп-3 команд по МЕНЬШЕМУ количеству очков:");
            foreach (var t in top3) Console.WriteLine($"{t.TeamName}: {t.TotalPoints} очков");
        }

        static void ShowWorstPointsTeam(AppDbContext db)
        {
            var worst = db.TeamStats.OrderBy(t => t.TotalPoints).FirstOrDefault();
            Console.WriteLine($"\nМеньше всех очков набрала: {worst?.TeamName} ({worst?.TotalPoints})");
        }

        //  ТЕСТОВЫЕ ДАННЫЕ 
        static void SeedData(AppDbContext db)
        {
            var real = new Team { Name = "Real Madrid" };
            var barca = new Team { Name = "Barcelona" };
            db.Teams.AddRange(real, barca); db.SaveChanges();

            var players = new[]
            {
                new Player { FullName = "Vinícius Júnior", Country = "Brazil", Number = 7, Position = "FW", TeamId = real.Id },
                new Player { FullName = "Jude Bellingham", Country = "England", Number = 5, Position = "MF", TeamId = real.Id },
                new Player { FullName = "Robert Lewandowski", Country = "Poland", Number = 9, Position = "FW", TeamId = barca.Id },
                new Player { FullName = "Pedri", Country = "Spain", Number = 8, Position = "MF", TeamId = barca.Id }
            };
            db.Players.AddRange(players); db.SaveChanges();

            var match = new Match { Team1Id = real.Id, Team2Id = barca.Id, Team1Goals = 2, Team2Goals = 1, MatchDate = DateTime.Now.AddDays(-5) };
            db.Matches.Add(match); db.SaveChanges();

            db.Goals.AddRange(
                new Goal { MatchId = match.Id, PlayerId = players[0].Id, Minute = 15 },
                new Goal { MatchId = match.Id, PlayerId = players[3].Id, Minute = 42 },
                new Goal { MatchId = match.Id, PlayerId = players[1].Id, Minute = 88 }
            );
            db.SaveChanges();
        }
    }
}