using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CountriesLinq
{
    internal class Program
    {
        static string ConnString;

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            ConnString = config.GetConnectionString("MyConnString");
            if (string.IsNullOrWhiteSpace(ConnString))
            {
                Console.WriteLine("Не найдена строка MyConnString");
                return;
            }

            List<Country> countries = LoadCountries();
            List<City>    cities    = LoadCities();

            Task2_BasicQueries(countries, cities);
            Task3_FilterQueries(countries, cities);
            Task4_AggregationQueries(countries, cities);

            Console.WriteLine("\nНажмите Enter...");
            Console.ReadLine();
        }

        static List<Country> LoadCountries()
        {
            var list = new List<Country>();
            using var conn = new SqlConnection(ConnString);
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT Id, Name, Continent, AreaKm2, Population, CapitalId FROM Countries", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Country
                {
                    Id         = r.GetInt32(0),
                    Name       = r.GetString(1),
                    Continent  = r.GetString(2),
                    AreaKm2    = r.GetDouble(3),
                    Population = r.GetInt64(4),
                    CapitalId  = r.IsDBNull(5) ? (int?)null : r.GetInt32(5)
                });
            }
            return list;
        }

        static List<City> LoadCities()
        {
            var list = new List<City>();
            using var conn = new SqlConnection(ConnString);
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT Id, Name, Population, IsCapital FROM Cities", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new City
                {
                    Id         = r.GetInt32(0),
                    Name       = r.GetString(1),
                    Population = r.GetInt32(2),
                    IsCapital  = r.GetBoolean(3)
                });
            }
            return list;
        }

        static void PrintHeader(string title)
        {
            Console.WriteLine();
            Console.WriteLine("\ttitle");
        }

        static void Task2_BasicQueries(List<Country> countries, List<City> cities)
        {
            Console.WriteLine("\n\tЗадание 2");

            PrintHeader("Все страны");
            var all = countries
                .Select(c => new { c.Id, c.Name, c.Continent, c.AreaKm2, c.Population, c.CapitalId });
            foreach (var c in all)
                Console.WriteLine($"{c.Id,2}. {c.Name,-15} | {c.Continent,-10} | " +
                                  $"{c.AreaKm2,12:N0} км² | {c.Population,12:N0} чел | CapitalId={c.CapitalId}");

            PrintHeader("Названия стран");
            var names = countries
                .Select(c => c.Name)
                .OrderBy(n => n);
            foreach (var n in names) Console.WriteLine(" - " + n);

            PrintHeader("Названия столиц");
            var capitals = countries
                .Where(c => c.CapitalId.HasValue)
                .Join(cities, c => c.CapitalId, ci => ci.Id, (c, ci) => ci.Name)
                .OrderBy(n => n);
            foreach (var n in capitals) Console.WriteLine(" - " + n);

            string target = "Россия";
            PrintHeader($"Крупные города страны: {target}");
            var countryId = countries.First(c => c.Name == target).Id;
            string[] rusCities = { "Санкт-Петербург", "Новосибирск" };
            var bigCities = cities
                .Where(ci => !ci.IsCapital && rusCities.Contains(ci.Name))
                .OrderByDescending(ci => ci.Population)
                .Select(ci => new { ci.Name, ci.Population });
            foreach (var ci in bigCities)
                Console.WriteLine($" - {ci.Name}: {ci.Population:N0}");

            PrintHeader("Столицы с населением > 5 млн");
            var bigCapitals = cities
                .Where(ci => ci.IsCapital && ci.Population > 5_000_000)
                .OrderByDescending(ci => ci.Population)
                .Select(ci => new { ci.Name, ci.Population });
            foreach (var ci in bigCapitals)
                Console.WriteLine($" - {ci.Name}: {ci.Population:N0}");

            PrintHeader("Европейские страны");
            var european = countries
                .Where(c => c.Continent == "Европа")
                .Select(c => c.Name)
                .OrderBy(n => n);
            foreach (var n in european) Console.WriteLine(" - " + n);

            double minArea = 1_000_000;
            PrintHeader($"Страны с площадью > {minArea:N0} км²");
            var bigArea = countries
                .Where(c => c.AreaKm2 > minArea)
                .OrderByDescending(c => c.AreaKm2)
                .Select(c => new { c.Name, c.AreaKm2 });
            foreach (var c in bigArea)
                Console.WriteLine($" - {c.Name}: {c.AreaKm2:N0} км²");
        }

        static void Task3_FilterQueries(List<Country> countries, List<City> cities)
        {
            Console.WriteLine("\n\tЗадание 3");

            PrintHeader("Столицы, содержащие буквы 'а' и 'р'");
            var filtered = cities
                .Where(ci => ci.IsCapital)
                .Where(ci => ci.Name.ToLower().Contains('а') && ci.Name.ToLower().Contains('р'))
                .Select(ci => ci.Name)
                .OrderBy(n => n);
            foreach (var n in filtered) Console.WriteLine(" - " + n);

            PrintHeader("Столицы, начинающиеся на 'к'");
            var startK = cities
                .Where(ci => ci.IsCapital && ci.Name.StartsWith("К", StringComparison.OrdinalIgnoreCase))
                .Select(ci => ci.Name)
                .OrderBy(n => n);
            foreach (var n in startK) Console.WriteLine(" - " + n);

            double minArea = 300_000;
            double maxArea = 1_000_000;
            PrintHeader($"Страны с площадью {minArea:N0}–{maxArea:N0} км²");
            var byRange = countries
                .Where(c => c.AreaKm2 >= minArea && c.AreaKm2 <= maxArea)
                .OrderBy(c => c.AreaKm2)
                .Select(c => new { c.Name, c.AreaKm2 });
            foreach (var c in byRange)
                Console.WriteLine($" - {c.Name}: {c.AreaKm2:N0} км²");

            long minPop = 100_000_000;
            PrintHeader($"Страны с населением > {minPop:N0}");
            var byPop = countries
                .Where(c => c.Population > minPop)
                .OrderByDescending(c => c.Population)
                .Select(c => new { c.Name, c.Population });
            foreach (var c in byPop)
                Console.WriteLine($" - {c.Name}: {c.Population:N0}");
        }

        static void Task4_AggregationQueries(List<Country> countries, List<City> cities)
        {
            Console.WriteLine("\n\tЗадание 4");

            PrintHeader("Топ-5 стран по площади");
            countries
                .OrderByDescending(c => c.AreaKm2)
                .Take(5)
                .Select(c => new { c.Name, c.AreaKm2 })
                .ToList()
                .ForEach(c => Console.WriteLine($" - {c.Name}: {c.AreaKm2:N0} км²"));

            PrintHeader("Топ-5 столиц по населению");
            cities
                .Where(ci => ci.IsCapital)
                .OrderByDescending(ci => ci.Population)
                .Take(5)
                .Select(ci => new { ci.Name, ci.Population })
                .ToList()
                .ForEach(ci => Console.WriteLine($" - {ci.Name}: {ci.Population:N0}"));

            PrintHeader("Страна с самой большой площадью");
            var maxAreaCountry = countries.OrderByDescending(c => c.AreaKm2).First();
            Console.WriteLine($" - {maxAreaCountry.Name}: {maxAreaCountry.AreaKm2:N0} км²");

            PrintHeader("Столица с самым большим населением");
            var maxCapital = cities.Where(ci => ci.IsCapital)
                                   .OrderByDescending(ci => ci.Population)
                                   .First();
            Console.WriteLine($" - {maxCapital.Name}: {maxCapital.Population:N0}");

            PrintHeader("Самая маленькая страна в Европе по площади");
            var smallestEurope = countries
                .Where(c => c.Continent == "Европа")
                .OrderBy(c => c.AreaKm2)
                .First();
            Console.WriteLine($" - {smallestEurope.Name}: {smallestEurope.AreaKm2:N0} км²");

            PrintHeader("Средняя площадь стран Европы");
            var avgArea = countries
                .Where(c => c.Continent == "Европа")
                .Average(c => c.AreaKm2);
            Console.WriteLine($" - {avgArea:N0} км²");

            PrintHeader("Топ-3 города России по населению");
            string[] rusCities = { "Москва", "Санкт-Петербург", "Новосибирск" };
            cities
                .Where(ci => rusCities.Contains(ci.Name))
                .OrderByDescending(ci => ci.Population)
                .Take(3)
                .Select(ci => new { ci.Name, ci.Population })
                .ToList()
                .ForEach(ci => Console.WriteLine($" - {ci.Name}: {ci.Population:N0}"));

            PrintHeader("Общее количество стран");
            Console.WriteLine($" - {countries.Count}");

            PrintHeader("Часть света с максимумом стран");
            var topContinent = countries
                .GroupBy(c => c.Continent)
                .Select(g => new { Continent = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .First();
            Console.WriteLine($" - {topContinent.Continent}: {topContinent.Count} стран");

            PrintHeader("Стран по частям света");
            countries
                .GroupBy(c => c.Continent)
                .Select(g => new { Continent = g.Key, Count = g.Count() })
                .OrderBy(x => x.Continent)
                .ToList()
                .ForEach(x => Console.WriteLine($" - {x.Continent}: {x.Count}"));
        }
    }
}