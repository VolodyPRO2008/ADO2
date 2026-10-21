using System;
using System.IO;
using System.Linq;
using System.Text;

namespace CountriesApp
{
    class Program
    {
        static string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CountriesDB;Integrated Security=True;";

        static CountriesDataContext db = null;

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ГЛАВНОЕ МЕНЮ ===");
                Console.WriteLine("1. Подключиться к базе данных");
                Console.WriteLine("2. Отключиться от базы данных");
                Console.WriteLine("3. Отчёты (Задания 2, 3, 4)");
                Console.WriteLine("0. Выход");
                Console.Write("Выбор: ");

                switch (Console.ReadLine())
                {
                    case "1": Connect(); break;
                    case "2": Disconnect(); break;
                    case "3":
                        if (db == null) Console.WriteLine("Сначала подключитесь к базе!");
                        else ReportsMenu();
                        break;
                    case "0": return;
                }
                Console.WriteLine("\nНажмите любую клавишу...");
                Console.ReadKey();
            }
        }

        static void Connect()
        {
            try
            {
                db = new CountriesDataContext(connectionString);
                db.Connection.Open();
                Console.WriteLine("Успешно подключено к базе данных");
            }
            catch (Exception ex)
            {
                db = null;
                Console.WriteLine("Ошибка подключения: " + ex.Message);
            }
        }

        static void Disconnect()
        {
            if (db != null)
            {
                db.Connection.Close();
                db.Dispose();
                db = null;
                Console.WriteLine("Отключено от базы данных.");
            }
            else
            {
                Console.WriteLine("Вы не подключены.");
            }
        }

        static void ReportsMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. Полная информация о странах");
                Console.WriteLine("2. Частичная информация о странах");
                Console.WriteLine("3. Информация о конкретной стране");
                Console.WriteLine("4. Города конкретной страны");
                Console.WriteLine("5. Страны, начинающиеся с буквы");
                Console.WriteLine("6. Столицы, начинающиеся с буквы");
                Console.WriteLine("7. Топ-3 столиц с наименьшим населением");
                Console.WriteLine("8. Топ-3 стран с наименьшим населением");
                Console.WriteLine("9. Среднее население столиц по частям света");
                Console.WriteLine("10. Топ-3 стран по каждой части света (мин. население)");
                Console.WriteLine("11. Топ-3 стран по каждой части света (макс. население)");
                Console.WriteLine("12. Среднее население конкретной страны");
                Console.WriteLine("13. Город с наименьшим населением в стране");
                Console.WriteLine("0. Назад");
                Console.Write("Выбор: ");

                var choice = Console.ReadLine();
                if (choice == "0") return;

                Console.Write("Вывести на экран (1) или в файл (2)? ");
                bool toFile = Console.ReadLine() == "2";

                try
                {
                    string report = GenerateReport(choice);
                    if (toFile) SaveToFile(report);
                    else Console.WriteLine(report);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: " + ex.Message);
                }

                Console.WriteLine("\nНажмите любую клавишу...");
                Console.ReadKey();
            }
        }

        static string GenerateReport(string choice)
        {
            var sb = new StringBuilder();

            switch (choice)
            {
                case "1":
                    foreach (var c in db.Countries)
                    {
                        var pop = db.Populations.FirstOrDefault(p => p.CountryId == c.Id)?.Total ?? 0;
                        sb.AppendLine($"{c.Id}. {c.Name} | Часть света: {c.PartOfWorld} | Площадь: {c.Area} | Население: {pop}");
                    }
                    break;

                case "2":
                    Console.Write("Сколько полей вывести (1 - только имя, 2 - имя и часть света)? ");
                    int fields = int.Parse(Console.ReadLine());
                    foreach (var c in db.Countries)
                    {
                        if (fields == 1) sb.AppendLine(c.Name);
                        else sb.AppendLine($"{c.Name} ({c.PartOfWorld})");
                    }
                    break;

                case "3":
                    Console.Write("Введите название страны: ");
                    string name = Console.ReadLine();
                    var country = db.Countries.FirstOrDefault(c => c.Name == name);
                    if (country != null)
                        sb.AppendLine($"{country.Name}, часть света: {country.PartOfWorld}, площадь: {country.Area}");
                    else sb.AppendLine("Страна не найдена.");
                    break;

                case "4":
                    Console.Write("Введите название страны: ");
                    string cName = Console.ReadLine();
                    var countryCities = db.Countries.Where(c => c.Name == cName)
                        .Join(db.Cities, c => c.Id, city => city.CountryId, (c, city) => city).ToList();
                    if (countryCities.Any())
                        foreach (var city in countryCities)
                            sb.AppendLine($"{city.Name} (население: {city.Population})");
                    else sb.AppendLine("Города не найдены.");
                    break;

                case "5":
                    Console.Write("Введите букву: ");
                    string letter = Console.ReadLine();
                    foreach (var c in db.Countries.Where(c => c.Name.StartsWith(letter)))
                        sb.AppendLine(c.Name);
                    break;

                case "6":
                    Console.Write("Введите букву: ");
                    string capLetter = Console.ReadLine();
                    foreach (var c in db.Capitals.Where(c => c.Name.StartsWith(capLetter)))
                        sb.AppendLine(c.Name);
                    break;

                case "7":
                    foreach (var c in db.Capitals.OrderBy(c => c.Population).Take(3))
                        sb.AppendLine($"{c.Name} - {c.Population}");
                    break;

                case "8":
                    var minCountries = db.Countries
                        .Join(db.Populations, c => c.Id, p => p.CountryId, (c, p) => new { c.Name, p.Total })
                        .OrderBy(x => x.Total).Take(3);
                    foreach (var c in minCountries)
                        sb.AppendLine($"{c.Name} - {c.Total}");
                    break;

                case "9":
                    var avgCapitals = db.Countries
                        .Join(db.Capitals, c => c.Id, cap => cap.CountryId, (c, cap) => new { c.PartOfWorld, cap.Population })
                        .GroupBy(x => x.PartOfWorld)
                        .Select(g => new { Part = g.Key, Avg = g.Average(x => x.Population) });
                    foreach (var a in avgCapitals)
                        sb.AppendLine($"Часть света: {a.Part} — среднее население столиц: {a.Avg:F0}");
                    break;

                case "10":
                    var minByPart = db.Countries
                        .Join(db.Populations, c => c.Id, p => p.CountryId, (c, p) => new { c.Name, c.PartOfWorld, p.Total })
                        .GroupBy(x => x.PartOfWorld)
                        .SelectMany(g => g.OrderBy(x => x.Total).Take(3));
                    foreach (var item in minByPart)
                        sb.AppendLine($"[{item.PartOfWorld}] {item.Name} - {item.Total}");
                    break;

                case "11":
                    var maxByPart = db.Countries
                        .Join(db.Populations, c => c.Id, p => p.CountryId, (c, p) => new { c.Name, c.PartOfWorld, p.Total })
                        .GroupBy(x => x.PartOfWorld)
                        .SelectMany(g => g.OrderByDescending(x => x.Total).Take(3));
                    foreach (var item in maxByPart)
                        sb.AppendLine($"[{item.PartOfWorld}] {item.Name} - {item.Total}");
                    break;

                case "12":
                    Console.Write("Введите название страны: ");
                    string targetCountry = Console.ReadLine();
                    var avgCities = db.Countries.Where(c => c.Name == targetCountry)
                        .Join(db.Cities, c => c.Id, city => city.CountryId, (c, city) => city.Population);
                    if (avgCities.Any())
                        sb.AppendLine($"Среднее население городов в {targetCountry}: {avgCities.Average():F0}");
                    else sb.AppendLine("Данных нет.");
                    break;

                case "13":
                    Console.Write("Введите название страны: ");
                    string minCityCountry = Console.ReadLine();
                    var minCity = db.Countries.Where(c => c.Name == minCityCountry)
                        .Join(db.Cities, c => c.Id, city => city.CountryId, (c, city) => city)
                        .OrderBy(city => city.Population).FirstOrDefault();
                    if (minCity != null)
                        sb.AppendLine($"Город с мин. населением: {minCity.Name} ({minCity.Population})");
                    else sb.AppendLine("Города не найдены.");
                    break;

                default:
                    sb.AppendLine("Неизвестный отчет.");
                    break;
            }

            return sb.ToString();
        }

        static void SaveToFile(string content)
        {
            string fileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            File.WriteAllText(fileName, content);
            Console.WriteLine($"Отчёт сохранён в файл: {fileName}");
        }
    }
}