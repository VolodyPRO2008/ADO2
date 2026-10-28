using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;

namespace MailingListApp
{
    // ==========================================
    // КЛАССЫ-МОДЕЛИ (POCO)
    // ==========================================
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Promotion
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    // ==========================================
    // ОСНОВНОЙ КЛАСС ПРОГРАММЫ
    // ==========================================
    class Program
    {
        static string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=MailingListDB;Trusted_Connection=True;TrustServerCertificate=True;";
        static IDbConnection? db = null;

        static void Main(string[] args)
        {
            if (!ConnectToDatabase()) return;

            while (true)
            {
                Console.WriteLine("\n СПИСОК РАССЫЛКИ ");
                Console.WriteLine(" Задание 3 ");
                Console.WriteLine("1. Все покупатели");
                Console.WriteLine("2. Email всех покупателей");
                Console.WriteLine("3. Список разделов");
                Console.WriteLine("4. Список акционных товаров");
                Console.WriteLine("5. Все города");
                Console.WriteLine("6. Все страны");
                Console.WriteLine(" Задание 4 ");
                Console.WriteLine("7. Покупатели из конкретного города");
                Console.WriteLine("8. Покупатели из конкретной страны");
                Console.WriteLine("9. Акции для конкретной страны");
                Console.WriteLine("0. Выход");
                Console.Write("Выбор: ");

                string? choice = Console.ReadLine();
                Console.Clear();

                switch (choice)
                {
                    case "1": ShowAllCustomers(); break;
                    case "2": ShowAllEmails(); break;
                    case "3": ShowCategories(); break;
                    case "4": ShowPromotions(); break;
                    case "5": ShowCities(); break;
                    case "6": ShowCountries(); break;
                    case "7": ShowCustomersByCity(); break;
                    case "8": ShowCustomersByCountry(); break;
                    case "9": ShowPromotionsByCountry(); break;
                    case "0": Disconnect(); return;
                    default: Console.WriteLine("Неверный ввод."); break;
                }
            }
        }

        static bool ConnectToDatabase()
        {
            try
            {
                db = new SqlConnection(connectionString);
                db.Open();
                Console.WriteLine("✅ Успешное подключение к базе данных 'Список рассылки'!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка подключения: {ex.Message}");
                return false;
            }
        }

        static void Disconnect()
        {
            if (db != null && db.State == ConnectionState.Open)
            {
                db.Close();
                Console.WriteLine("Отключено от базы данных. До свидания!");
            }
        }

        //  ЗАДАНИЕ 3 

        static void ShowAllCustomers()
        {
            string sql = @"
                SELECT c.Id, c.FullName, c.BirthDate, c.Gender, c.Email, 
                       ci.Name AS CityName, co.Name AS CountryName
                FROM Customers c
                JOIN Cities ci ON c.CityId = ci.Id
                JOIN Countries co ON ci.CountryId = co.Id";

            // Добавили ! в конце, чтобы убрать предупреждение
            var customers = db!.Query<Customer>(sql);
            Console.WriteLine("\nВсе покупатели:");
            foreach (var c in customers)
                Console.WriteLine($"{c.Id}. {c.FullName} | {c.Gender} | {c.BirthDate.ToShortDateString()} | {c.Email} | {c.CityName}, {c.CountryName}");
        }

        static void ShowAllEmails()
        {
            string sql = "SELECT Email FROM Customers";
            var emails = db!.Query<string>(sql);
            Console.WriteLine("\nEmail всех покупателей:");
            foreach (var email in emails) Console.WriteLine(email);
        }

        static void ShowCategories()
        {
            var categories = db!.Query<Category>("SELECT * FROM Categories");
            Console.WriteLine("\nСписок разделов:");
            foreach (var cat in categories) Console.WriteLine($"{cat.Id}. {cat.Name}");
        }

        static void ShowPromotions()
        {
            string sql = @"
                SELECT p.Id, p.Title, cat.Name AS CategoryName, co.Name AS CountryName, p.StartDate, p.EndDate
                FROM Promotions p
                JOIN Categories cat ON p.CategoryId = cat.Id
                JOIN Countries co ON p.CountryId = co.Id";

            var promos = db!.Query<Promotion>(sql);
            Console.WriteLine("\nСписок акционных товаров:");
            foreach (var p in promos)
                Console.WriteLine($"{p.Title} | Раздел: {p.CategoryName} | Страна: {p.CountryName} | {p.StartDate.ToShortDateString()} - {p.EndDate.ToShortDateString()}");
        }

        static void ShowCities()
        {
            var cities = db!.Query<City>("SELECT * FROM Cities");
            Console.WriteLine("\nВсе города:");
            foreach (var city in cities) Console.WriteLine($"{city.Id}. {city.Name}");
        }

        static void ShowCountries()
        {
            var countries = db!.Query<Country>("SELECT * FROM Countries");
            Console.WriteLine("\nВсе страны:");
            foreach (var country in countries) Console.WriteLine($"{country.Id}. {country.Name}");
        }

        //  ЗАДАНИЕ 4 

        static void ShowCustomersByCity()
        {
            Console.Write("Введите название города: ");
            string? city = Console.ReadLine();

            string sql = @"
                SELECT c.Id, c.FullName, c.Email, ci.Name AS CityName
                FROM Customers c
                JOIN Cities ci ON c.CityId = ci.Id
                WHERE ci.Name = @CityName";

            // Вот тут добавляем city!, чтобы убрать ошибку CS8604
            var customers = db!.Query<Customer>(sql, new { CityName = city! });

            Console.WriteLine($"\nПокупатели из города {city}:");
            if (!customers.Any()) Console.WriteLine("Никого не найдено.");
            foreach (var c in customers) Console.WriteLine($"{c.FullName} ({c.Email})");
        }

        static void ShowCustomersByCountry()
        {
            Console.Write("Введите название страны: ");
            string? country = Console.ReadLine();

            string sql = @"
                SELECT c.Id, c.FullName, c.Email, co.Name AS CountryName
                FROM Customers c
                JOIN Cities ci ON c.CityId = ci.Id
                JOIN Countries co ON ci.CountryId = co.Id
                WHERE co.Name = @CountryName";

            var customers = db!.Query<Customer>(sql, new { CountryName = country! });

            Console.WriteLine($"\nПокупатели из страны {country}:");
            if (!customers.Any()) Console.WriteLine("Никого не найдено.");
            foreach (var c in customers) Console.WriteLine($"{c.FullName} ({c.Email})");
        }

        static void ShowPromotionsByCountry()
        {
            Console.Write("Введите название страны: ");
            string? country = Console.ReadLine();

            string sql = @"
                SELECT p.Title, cat.Name AS CategoryName, p.StartDate, p.EndDate
                FROM Promotions p
                JOIN Categories cat ON p.CategoryId = cat.Id
                JOIN Countries co ON p.CountryId = co.Id
                WHERE co.Name = @CountryName";

            var promos = db!.Query<Promotion>(sql, new { CountryName = country! });

            Console.WriteLine($"\nАкции для страны {country}:");
            if (!promos.Any()) Console.WriteLine("Акций не найдено.");
            foreach (var p in promos) Console.WriteLine($"{p.Title} (Раздел: {p.CategoryName}) до {p.EndDate.ToShortDateString()}");
        }
    }
}