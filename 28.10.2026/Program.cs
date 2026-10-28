using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace MailingListApp
{
    public class CountryStats
    {
        public string Country { get; set; }
        public int CustomerCount { get; set; }
        public int CityCount { get; set; }
    }

    public class CityStats
    {
        public string City { get; set; }
        public int CustomerCount { get; set; }
    }

    public class Promotion
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    class Program
    {
        // Строка подключения (Замени на свою, если используешь LocalDB)
        static string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=MailingListDB;Trusted_Connection=True;";

        static async Task Main(string[] args)
        {
            Console.WriteLine(" СПИСОК РАССЫЛКИ: РАБОТА С DAPPER \n");

            //  ЗАДАНИЕ 1 
            Console.WriteLine(" Задание 1: Статистика ");
            await GetCustomersPerCity();
            await GetCustomersPerCountry();
            await GetCitiesPerCountry();
            await GetAverageCitiesPerCountry();

            //  ЗАДАНИЕ 2 
            Console.WriteLine("\n Задание 2: Выборки по связям ");
            await GetCategoriesForCustomersInCountry("Russia");
            await GetPromotionsForCategoryBetweenDates("Electronics", new DateTime(2023, 1, 1), new DateTime(2023, 12, 31));
            await GetPromotionsForCustomer(1); // 1 - ID покупателя

            //  ЗАДАНИЕ 3 
            Console.WriteLine("\n Задание 3: Топы и лидеры ");
            await GetTop3CountriesByCustomers();
            await GetBestCountryByCustomers();
            await GetTop3CitiesByCustomers();
            await GetBestCityByCustomers();

            Console.ReadLine();
        }


        // ЗАДАНИЕ 1


        static async Task GetCustomersPerCity()
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sql = @"
                    SELECT c.Name AS City, COUNT(cu.Id) AS CustomerCount
                    FROM Cities c
                    LEFT JOIN Customers cu ON c.Id = cu.CityId
                    GROUP BY c.Name
                    ORDER BY CustomerCount DESC;";

                var result = await db.QueryAsync<CityStats>(sql);
                Console.WriteLine("\n1. Количество покупателей в каждом городе:");
                foreach (var item in result)
                    Console.WriteLine($"{item.City}: {item.CustomerCount}");
            }
        }

        static async Task GetCustomersPerCountry()
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sql = @"
                    SELECT co.Name AS Country, COUNT(cu.Id) AS CustomerCount
                    FROM Countries co
                    LEFT JOIN Cities c ON co.Id = c.CountryId
                    LEFT JOIN Customers cu ON c.Id = cu.CityId
                    GROUP BY co.Name
                    ORDER BY CustomerCount DESC;";

                var result = await db.QueryAsync<CountryStats>(sql);
                Console.WriteLine("\n2. Количество покупателей в каждой стране:");
                foreach (var item in result)
                    Console.WriteLine($"{item.Country}: {item.CustomerCount}");
            }
        }

        static async Task GetCitiesPerCountry()
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sql = @"
                    SELECT co.Name AS Country, COUNT(c.Id) AS CityCount
                    FROM Countries co
                    LEFT JOIN Cities c ON co.Id = c.CountryId
                    GROUP BY co.Name
                    ORDER BY CityCount DESC;";

                var result = await db.QueryAsync<CountryStats>(sql);
                Console.WriteLine("\n3. Количество городов в каждой стране:");
                foreach (var item in result)
                    Console.WriteLine($"{item.Country}: {item.CityCount}");
            }
        }

        static async Task GetAverageCitiesPerCountry()
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                // Считаем среднее: всего городов делим на количество стран
                string sql = @"
                    SELECT AVG(Cast(CityCount as Float)) 
                    FROM (
                        SELECT COUNT(c.Id) AS CityCount
                        FROM Countries co
                        LEFT JOIN Cities c ON co.Id = c.CountryId
                        GROUP BY co.Id
                    ) AS SubQuery;";

                var avg = await db.ExecuteScalarAsync<double>(sql);
                Console.WriteLine($"\n4. Среднее количество городов по всем странам: {Math.Round(avg, 2)}");
            }
        }


        // ЗАДАНИЕ 2


        static async Task GetCategoriesForCustomersInCountry(string countryName)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sql = @"
                    SELECT DISTINCT cat.Name AS Category
                    FROM Categories cat
                    JOIN CustomerCategories cc ON cat.Id = cc.CategoryId
                    JOIN Customers cu ON cc.CustomerId = cu.Id
                    JOIN Cities c ON cu.CityId = c.Id
                    JOIN Countries co ON c.CountryId = co.Id
                    WHERE co.Name = @CountryName;";

                var result = await db.QueryAsync<Category>(sql, new { CountryName = countryName });
                Console.WriteLine($"\n1. Разделы, которыми интересуются покупатели из страны {countryName}:");
                foreach (var item in result)
                    Console.WriteLine($"- {item.Name}");
            }
        }

        static async Task GetPromotionsForCategoryBetweenDates(string categoryName, DateTime start, DateTime end)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sql = @"
                    SELECT p.Title, p.StartDate, p.EndDate
                    FROM Promotions p
                    JOIN Categories cat ON p.CategoryId = cat.Id
                    WHERE cat.Name = @CategoryName 
                      AND p.StartDate >= @StartDate 
                      AND p.EndDate <= @EndDate;";

                var result = await db.QueryAsync<Promotion>(sql, new { CategoryName = categoryName, StartDate = start, EndDate = end });
                Console.WriteLine($"\n2. Акционные товары раздела '{categoryName}' с {start.ToShortDateString()} по {end.ToShortDateString()}:");
                foreach (var item in result)
                    Console.WriteLine($"- {item.Title} ({item.StartDate.ToShortDateString()} - {item.EndDate.ToShortDateString()})");
            }
        }

        static async Task GetPromotionsForCustomer(int customerId)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sql = @"
                    SELECT p.Title, p.StartDate, p.EndDate
                    FROM Promotions p
                    JOIN Categories cat ON p.CategoryId = cat.Id
                    JOIN CustomerCategories cc ON cat.Id = cc.CategoryId
                    WHERE cc.CustomerId = @CustomerId;";

                var result = await db.QueryAsync<Promotion>(sql, new { CustomerId = customerId });
                Console.WriteLine($"\n3. Акционные товары для покупателя с ID {customerId}:");
                foreach (var item in result)
                    Console.WriteLine($"- {item.Title}");
            }
        }


        // ЗАДАНИЕ 3


        static async Task GetTop3CountriesByCustomers()
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sql = @"
                    SELECT TOP 3 co.Name AS Country, COUNT(cu.Id) AS CustomerCount
                    FROM Countries co
                    LEFT JOIN Cities c ON co.Id = c.CountryId
                    LEFT JOIN Customers cu ON c.Id = cu.CityId
                    GROUP BY co.Name
                    ORDER BY CustomerCount DESC;";

                var result = await db.QueryAsync<CountryStats>(sql);
                Console.WriteLine("\n1. Топ-3 стран по количеству покупателей:");
                int rank = 1;
                foreach (var item in result)
                    Console.WriteLine($"{rank++}. {item.Country} ({item.CustomerCount} чел.)");
            }
        }

        static async Task GetBestCountryByCustomers()
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sql = @"
                    SELECT TOP 1 co.Name AS Country, COUNT(cu.Id) AS CustomerCount
                    FROM Countries co
                    LEFT JOIN Cities c ON co.Id = c.CountryId
                    LEFT JOIN Customers cu ON c.Id = cu.CityId
                    GROUP BY co.Name
                    ORDER BY CustomerCount DESC;";

                var result = await db.QueryFirstOrDefaultAsync<CountryStats>(sql);
                Console.WriteLine($"\n2. Лучшая страна по количеству покупателей: {result?.Country} ({result?.CustomerCount} чел.)");
            }
        }

        static async Task GetTop3CitiesByCustomers()
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sql = @"
                    SELECT TOP 3 c.Name AS City, COUNT(cu.Id) AS CustomerCount
                    FROM Cities c
                    LEFT JOIN Customers cu ON c.Id = cu.CityId
                    GROUP BY c.Name
                    ORDER BY CustomerCount DESC;";

                var result = await db.QueryAsync<CityStats>(sql);
                Console.WriteLine("\n3. Топ-3 городов по количеству покупателей:");
                int rank = 1;
                foreach (var item in result)
                    Console.WriteLine($"{rank++}. {item.City} ({item.CustomerCount} чел.)");
            }
        }

        static async Task GetBestCityByCustomers()
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sql = @"
                    SELECT TOP 1 c.Name AS City, COUNT(cu.Id) AS CustomerCount
                    FROM Cities c
                    LEFT JOIN Customers cu ON c.Id = cu.CityId
                    GROUP BY c.Name
                    ORDER BY CustomerCount DESC;";

                var result = await db.QueryFirstOrDefaultAsync<CityStats>(sql);
                Console.WriteLine($"\n4. Лучший город по количеству покупателей: {result?.City} ({result?.CustomerCount} чел.)");
            }
        }
    }

}