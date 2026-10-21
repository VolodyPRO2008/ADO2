using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AdoNetLibrary
{
    class Program
    {
        private static string _connString;
        private static SqlConnection _conn;

        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _connString = config.GetConnectionString("ConnString");
            _conn = new SqlConnection(_connString);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("\tСклад\t(состояние: " + _conn.State + ")");
                Console.WriteLine("1.  Подключиться к БД");
                Console.WriteLine("2.  Отключиться от БД");
                Console.WriteLine("3.  Показать все товары");
                Console.WriteLine("4.  Показать все типы товаров");
                Console.WriteLine("5.  Показать всех поставщиков");
                Console.WriteLine("6.  Товар с максимальным количеством");
                Console.WriteLine("7.  Товар с минимальным количеством");
                Console.WriteLine("8.  Товар с минимальной себестоимостью");
                Console.WriteLine("9.  Товар с максимальной себестоимостью");
                Console.WriteLine("10. Товары заданной категории");
                Console.WriteLine("11. Товары заданного поставщика");
                Console.WriteLine("12. Самый старый товар на складе");
                Console.WriteLine("13. Среднее количество товаров по каждому типу");
                Console.WriteLine("0.  Выход");
                Console.Write("\nВыберите пункт: ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1": Connect(); break;
                        case "2": Disconnect(); break;
                        case "3": ShowAllProducts(); break;
                        case "4": ShowCategories(); break;
                        case "5": ShowSuppliers(); break;
                        case "6": ShowExtremeProduct("Quantity", "MAX"); break;
                        case "7": ShowExtremeProduct("Quantity", "MIN"); break;
                        case "8": ShowExtremeProduct("Cost", "MIN"); break;
                        case "9": ShowExtremeProduct("Cost", "MAX"); break;
                        case "10": ShowProductsByCategory(); break;
                        case "11": ShowProductsBySupplier(); break;
                        case "12": ShowOldestProduct(); break;
                        case "13": ShowAverageQuantityByCategory(); break;
                        case "0": return;
                        default: Console.WriteLine("Неизвестный пункт."); break;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Ошибка SQL: {ex.Message}");
                }

                Console.WriteLine("\nНажмите любую клавишу...");
                Console.ReadKey();
            }
        }

        private static void EnsureOpen()
        {
            if (_conn.State != ConnectionState.Open)
            {
                Console.WriteLine("Сначала подключитесь к БД.");
                throw new InvalidOperationException("Соединение закрыто");
            }
        }

        static void Connect()
        {
            try
            {
                if (_conn.State == ConnectionState.Open)
                {
                    Console.WriteLine("Уже подключено.");
                    return;
                }
                _conn.Open();
                Console.WriteLine("Подключение к БД установлено успешно.");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Не удалось подключиться: {ex.Message}");
            }
        }

        static void Disconnect()
        {
            if (_conn.State == ConnectionState.Open)
            {
                _conn.Close();
                Console.WriteLine("Соединение закрыто.");
            }
            else
            {
                Console.WriteLine("Соединение и так закрыто.");
            }
        }

        static void ShowAllProducts()
        {
            EnsureOpen();
            string sql = @"
                SELECT p.Id, p.Name, c.Name AS Category, s.Name AS Supplier,
                       p.Quantity, p.Cost, p.DeliveryDate
                FROM Products p
                JOIN Categories c ON p.CategoryId = c.Id
                JOIN Suppliers  s ON p.SupplierId = s.Id
                ORDER BY p.Id";

            using var cmd = new SqlCommand(sql, _conn);
            using var rdr = cmd.ExecuteReader();
            Console.WriteLine($"{"Id",-4} {"Название",-22} {"Категория",-18} {"Поставщик",-20} {"Кол-во",6} {"Цена",10} {"Дата",12}");
            while (rdr.Read())
            {
                Console.WriteLine($"{rdr.GetInt32(0),-4} {rdr.GetString(1),-22} {rdr.GetString(2),-18} {rdr.GetString(3),-20} {rdr.GetInt32(4),6} {rdr.GetDecimal(5),10:F2} {rdr.GetDateTime(6):yyyy-MM-dd}");
            }
        }

        static void ShowCategories()
        {
            EnsureOpen();
            using var cmd = new SqlCommand("SELECT Id, Name FROM Categories", _conn);
            using var rdr = cmd.ExecuteReader();
            Console.WriteLine("Типы товаров:");
            while (rdr.Read())
                Console.WriteLine($"  {rdr.GetInt32(0)}. {rdr.GetString(1)}");
        }

        static void ShowSuppliers()
        {
            EnsureOpen();
            using var cmd = new SqlCommand("SELECT Id, Name FROM Suppliers", _conn);
            using var rdr = cmd.ExecuteReader();
            Console.WriteLine("Поставщики:");
            while (rdr.Read())
                Console.WriteLine($"  {rdr.GetInt32(0)}. {rdr.GetString(1)}");
        }

        static void ShowExtremeProduct(string column, string agg)
        {
            EnsureOpen();
            string sql = $@"
                SELECT TOP 1 p.Name, c.Name AS Category, s.Name AS Supplier,
                       p.Quantity, p.Cost, p.DeliveryDate
                FROM Products p
                JOIN Categories c ON p.CategoryId = c.Id
                JOIN Suppliers  s ON p.SupplierId = s.Id
                ORDER BY p.{column} {(agg == "MAX" ? "DESC" : "ASC")}";

            using var cmd = new SqlCommand(sql, _conn);
            using var rdr = cmd.ExecuteReader();
            if (rdr.Read())
            {
                string label = column == "Quantity"
                    ? (agg == "MAX" ? "с максимальным количеством" : "с минимальным количеством")
                    : (agg == "MAX" ? "с максимальной себестоимостью" : "с минимальной себестоимостью");

                Console.WriteLine($"Товар {label}:");
                Console.WriteLine($"  {rdr.GetString(0)} | Категория: {rdr.GetString(1)} | Поставщик: {rdr.GetString(2)}");
                Console.WriteLine($"  Кол-во: {rdr.GetInt32(3)} | Цена: {rdr.GetDecimal(4):F2} | Дата: {rdr.GetDateTime(5):yyyy-MM-dd}");
            }
        }

        static void ShowProductsByCategory()
        {
            EnsureOpen();
            Console.Write("Введите Id категории: ");
            int id = int.Parse(Console.ReadLine());

            using var cmd = new SqlCommand(
                "SELECT Name, Quantity, Cost FROM Products WHERE CategoryId = @id", _conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var rdr = cmd.ExecuteReader();
            Console.WriteLine($"Товары категории Id={id}:");
            bool any = false;
            while (rdr.Read())
            {
                any = true;
                Console.WriteLine($"  {rdr.GetString(0)} | Кол-во: {rdr.GetInt32(1)} | Цена: {rdr.GetDecimal(2):F2}");
            }
            if (!any) Console.WriteLine("  (пусто)");
        }

        static void ShowProductsBySupplier()
        {
            EnsureOpen();
            Console.Write("Введите Id поставщика: ");
            int id = int.Parse(Console.ReadLine());

            using var cmd = new SqlCommand(
                "SELECT Name, Quantity, Cost FROM Products WHERE SupplierId = @id", _conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var rdr = cmd.ExecuteReader();
            Console.WriteLine($"Товары поставщика Id={id}:");
            bool any = false;
            while (rdr.Read())
            {
                any = true;
                Console.WriteLine($"  {rdr.GetString(0)} | Кол-во: {rdr.GetInt32(1)} | Цена: {rdr.GetDecimal(2):F2}");
            }
            if (!any) Console.WriteLine("  (пусто)");
        }

        static void ShowOldestProduct()
        {
            EnsureOpen();
            string sql = @"
                SELECT TOP 1 p.Name, p.DeliveryDate, c.Name
                FROM Products p
                JOIN Categories c ON p.CategoryId = c.Id
                ORDER BY p.DeliveryDate ASC";

            using var cmd = new SqlCommand(sql, _conn);
            using var rdr = cmd.ExecuteReader();
            if (rdr.Read())
                Console.WriteLine($"Самый старый товар: {rdr.GetString(0)} (поставлен {rdr.GetDateTime(1):yyyy-MM-dd}, категория: {rdr.GetString(2)})");
        }

        static void ShowAverageQuantityByCategory()
        {
            EnsureOpen();
            string sql = @"
                SELECT c.Name, AVG(CAST(p.Quantity AS FLOAT)) AS AvgQty
                FROM Categories c
                LEFT JOIN Products p ON p.CategoryId = c.Id
                GROUP BY c.Name
                ORDER BY c.Name";

            using var cmd = new SqlCommand(sql, _conn);
            using var rdr = cmd.ExecuteReader();
            Console.WriteLine("Среднее количество товаров по типу:");
            while (rdr.Read())
            {
                string name = rdr.GetString(0);
                double avg = rdr.IsDBNull(1) ? 0 : rdr.GetDouble(1);
                Console.WriteLine($"  {name,-20} → {avg:F2}");
            }
        }
    }
}