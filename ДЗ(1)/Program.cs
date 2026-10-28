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
                Console.WriteLine(" ЗАДАНИЕ 2: ВЫБОРКА ");
                Console.WriteLine("1. Разница забитых и пропущенных голов");
                Console.WriteLine("2. Полная информация о матче (по ID)");
                Console.WriteLine("3. Матчи на конкретную дату");
                Console.WriteLine("4. Все матчи конкретной команды");
                Console.WriteLine("5. Игроки, забившие в конкретную дату");
                Console.WriteLine(" ЗАДАНИЕ 3: РЕДАКТИРОВАНИЕ ");
                Console.WriteLine("6. Добавить новый матч");
                Console.WriteLine("7. Изменить существующий матч");
                Console.WriteLine("8. Удалить матч");
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
                        case "0": return;
                        default: Console.WriteLine("Неверный ввод."); break;
                    }
                }
            }
        }

        //  ЗАДАНИЕ 2 

        static void ShowGoalDifference(AppDbContext db)
        {
            Console.WriteLine(" Разница забитых и пропущенных ");
            var teams = db.Teams.ToList();
            foreach (var team in teams)
            {
                var scored = db.Matches.Where(m => m.Team1Id == team.Id).Sum(m => m.Team1Goals) +
                             db.Matches.Where(m => m.Team2Id == team.Id).Sum(m => m.Team2Goals);

                var conceded = db.Matches.Where(m => m.Team1Id == team.Id).Sum(m => m.Team2Goals) +
                               db.Matches.Where(m => m.Team2Id == team.Id).Sum(m => m.Team1Goals);

                Console.WriteLine($"{team.Name}: Забито {scored}, Пропущено {conceded} | Разница: {scored - conceded}");
            }
        }

        static void ShowMatchInfo(AppDbContext db)
        {
            Console.Write("Введите ID матча: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;

            var match = db.Matches.Include(m => m.Team1).Include(m => m.Team2).Include(m => m.Goals).ThenInclude(g => g.Player)
                .FirstOrDefault(m => m.Id == id);

            if (match == null) { Console.WriteLine("Матч не найден."); return; }

            Console.WriteLine($"\nДата: {match.MatchDate.ToShortDateString()}");
            Console.WriteLine($"{match.Team1.Name} {match.Team1Goals} : {match.Team2Goals} {match.Team2.Name}");
            Console.WriteLine("Голы:");
            if (!match.Goals.Any()) Console.WriteLine("- Голов не было");
            foreach (var goal in match.Goals)
                Console.WriteLine($"- {goal.Player.FullName} на {goal.Minute} минуте");
        }

        static void ShowMatchesByDate(AppDbContext db)
        {
            Console.Write("Введите дату (гггг-мм-дд): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime date)) return;

            var matches = db.Matches.Include(m => m.Team1).Include(m => m.Team2)
                .Where(m => m.MatchDate.Date == date.Date).ToList();

            if (!matches.Any()) { Console.WriteLine("Матчей нет."); return; }
            foreach (var m in matches)
                Console.WriteLine($"{m.Team1.Name} {m.Team1Goals} : {m.Team2Goals} {m.Team2.Name}");
        }

        static void ShowMatchesByTeam(AppDbContext db)
        {
            Console.Write("Введите название команды (например, Real Madrid): ");
            string tName = Console.ReadLine()?.ToLower();

            var matches = db.Matches.Include(m => m.Team1).Include(m => m.Team2)
                .Where(m => m.Team1.Name.ToLower() == tName || m.Team2.Name.ToLower() == tName).ToList();

            if (!matches.Any()) { Console.WriteLine("Матчей нет."); return; }
            foreach (var m in matches)
                Console.WriteLine($"{m.MatchDate.ToShortDateString()}: {m.Team1.Name} {m.Team1Goals} : {m.Team2Goals} {m.Team2.Name}");
        }

        static void ShowScorersByDate(AppDbContext db)
        {
            Console.Write("Введите дату (гггг-мм-дд): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime date)) return;

            var scorers = db.Goals.Include(g => g.Player).ThenInclude(p => p.Team)
                .Where(g => g.Match.MatchDate.Date == date.Date)
                .Select(g => new { g.Player.FullName, Team = g.Player.Team.Name })
                .ToList();

            if (!scorers.Any()) { Console.WriteLine("В эту дату никто не забивал."); return; }
            foreach (var s in scorers)
                Console.WriteLine($"{s.FullName} ({s.Team})");
        }

        // ЗАДАНИЕ 3

        static void AddMatch(AppDbContext db)
        {
            Console.WriteLine("Доступные команды: " + string.Join(", ", db.Teams.Select(t => t.Name)));
            Console.Write("Команда 1: "); string t1 = Console.ReadLine();
            Console.Write("Команда 2: "); string t2 = Console.ReadLine();
            Console.Write("Дата (гггг-мм-дд): "); DateTime date = DateTime.Parse(Console.ReadLine());

            // Проверка на дубликат (Задание 3)
            bool exists = db.Matches.Include(m => m.Team1).Include(m => m.Team2)
                .Any(m => m.Team1.Name == t1 && m.Team2.Name == t2 && m.MatchDate.Date == date.Date);

            if (exists) { Console.WriteLine("ОШИБКА: Такой матч уже существует!"); return; }

            var team1 = db.Teams.FirstOrDefault(t => t.Name == t1);
            var team2 = db.Teams.FirstOrDefault(t => t.Name == t2);
            if (team1 == null || team2 == null) { Console.WriteLine("Команда не найдена."); return; }

            Console.Write($"Голы {team1.Name}: "); int g1 = int.Parse(Console.ReadLine());
            Console.Write($"Голы {team2.Name}: "); int g2 = int.Parse(Console.ReadLine());

            db.Matches.Add(new Match { Team1Id = team1.Id, Team2Id = team2.Id, Team1Goals = g1, Team2Goals = g2, MatchDate = date });
            db.SaveChanges();
            Console.WriteLine("Матч добавлен!");
        }

        static void EditMatch(AppDbContext db)
        {
            Console.Write("ID матча: "); int id = int.Parse(Console.ReadLine());
            var match = db.Matches.Find(id);
            if (match == null) { Console.WriteLine("Не найден."); return; }

            Console.WriteLine("1 - Голы 1 | 2 - Голы 2 | 3 - Дата");
            string c = Console.ReadLine();
            if (c == "1") { Console.Write("Новые голы 1: "); match.Team1Goals = int.Parse(Console.ReadLine()); }
            if (c == "2") { Console.Write("Новые голы 2: "); match.Team2Goals = int.Parse(Console.ReadLine()); }
            if (c == "3") { Console.Write("Новая дата: "); match.MatchDate = DateTime.Parse(Console.ReadLine()); }

            db.SaveChanges();
            Console.WriteLine("Обновлено!");
        }

        static void DeleteMatch(AppDbContext db)
        {
            Console.Write("Команда 1: "); string t1 = Console.ReadLine();
            Console.Write("Команда 2: "); string t2 = Console.ReadLine();
            Console.Write("Дата (гггг-мм-дд): "); DateTime date = DateTime.Parse(Console.ReadLine());

            var match = db.Matches.Include(m => m.Team1).Include(m => m.Team2)
                .FirstOrDefault(m => m.Team1.Name == t1 && m.Team2.Name == t2 && m.MatchDate.Date == date.Date);

            if (match == null) { Console.WriteLine("Матч не найден."); return; }

            Console.Write($"Точно удалить матч {t1} - {t2}? (y/n): ");
            if (Console.ReadLine().ToLower() == "y")
            {
                db.Matches.Remove(match);
                db.SaveChanges();
                Console.WriteLine("Удалено.");
            }
        }

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