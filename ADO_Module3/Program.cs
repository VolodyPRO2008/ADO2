using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace AdoModule3
{
    class Program
    {
        private static IConfiguration _config = null!;
        private static string _providerName = "Microsoft.Data.SqlClient";
        private static string _connString = null!;

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding  = System.Text.Encoding.UTF8;

            _config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            DbProviderFactories.RegisterFactory(
                "Microsoft.Data.SqlClient",
                Microsoft.Data.SqlClient.SqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory(
                "Microsoft.Data.Sqlite",
                Microsoft.Data.Sqlite.SqliteFactory.Instance);

            _connString = _config.GetConnectionString("SqlServer")!;

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"\tОценки студентов [провайдер: {_providerName}]");
                Console.WriteLine("1. Показать все оценки");
                Console.WriteLine("2. Показать всех студентов");
                Console.WriteLine("3. Добавить оценку");
                Console.WriteLine("4. Обновить оценку");
                Console.WriteLine("5. Удалить оценку");
                Console.WriteLine("6. Сменить СУБД");
                Console.WriteLine("0. Выход");
                Console.Write("\n> ");

                string? choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1": await ShowGradesAsync(); break;
                        case "2": await ShowStudentsAsync(); break;

                        case "3": await AddGradeAsync(); break;
                        case "4": await UpdateGradeAsync(); break;

                        case "5": await DeleteGradeAsync(); break;

                        case "6": SwitchProvider(); break;

                        case "0": return;
                        default: Console.WriteLine("Нет такого пункта."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: " + ex.Message);
                }

                Console.WriteLine("\nНажмите Enter...");
                Console.ReadLine();
            }
        }

        private static async Task<DbConnection> OpenConnectionAsync()
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(_providerName);
            DbConnection conn = factory.CreateConnection()!;
            conn.ConnectionString = _connString;
            await conn.OpenAsync();
            return conn;
        }

        static async Task ShowGradesAsync()
        {
            var sw = Stopwatch.StartNew();

            await using var conn = await OpenConnectionAsync();

            string sql = @"
                SELECT g.Id, s.FullName, sub.Name AS Subject, g.Grade, g.GradeDate
                FROM Grades g
                JOIN Students s ON g.StudentId = s.Id
                JOIN Subjects sub ON g.SubjectId = sub.Id
                ORDER BY g.Id";

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            await using var reader = await cmd.ExecuteReaderAsync();

            Console.WriteLine($"{"Id",-4} {"Студент",-20} {"Предмет",-20} {"Оценка",6} {"Дата",12}");
            Console.WriteLine(new string('-', 70));

            while (await reader.ReadAsync())
            {
                Console.WriteLine(
                    $"{reader.GetInt32(0),-4} " +
                    $"{reader.GetString(1),-20} " +
                    $"{reader.GetString(2),-20} " +
                    $"{reader.GetInt32(3),6} " +
                    $"{reader.GetDateTime(4):yyyy-MM-dd}");
            }

            sw.Stop();
            Console.WriteLine($"\nВыполнено за {sw.Elapsed.TotalSeconds:F4} сек.");
        }

        static async Task ShowStudentsAsync()
        {
            var sw = Stopwatch.StartNew();

            await using var conn = await OpenConnectionAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, FullName, GroupName FROM Students ORDER BY Id";

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                Console.WriteLine($"{reader.GetInt32(0),-4} {reader.GetString(1),-20} {reader.GetString(2)}");
            }

            sw.Stop();
            Console.WriteLine($"\nВыполнено за {sw.Elapsed.TotalSeconds:F4} сек.");
        }

        static async Task AddGradeAsync()
        {
            Console.Write("Id студента: ");   int studentId = int.Parse(Console.ReadLine()!);
            Console.Write("Id предмета: ");   int subjectId = int.Parse(Console.ReadLine()!);
            Console.Write("Оценка: ");        int grade     = int.Parse(Console.ReadLine()!);

            var sw = Stopwatch.StartNew();

            await using var conn = await OpenConnectionAsync();
            await using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                INSERT INTO Grades (StudentId, SubjectId, Grade, GradeDate)
                VALUES (@sid, @subid, @g, @d)";

            AddParameter(cmd, "@sid",   studentId);
            AddParameter(cmd, "@subid", subjectId);
            AddParameter(cmd, "@g",     grade);
            AddParameter(cmd, "@d",     DateTime.Today);

            int rows = await cmd.ExecuteNonQueryAsync();
            sw.Stop();

            Console.WriteLine($"Добавлено строк: {rows}. {sw.Elapsed.TotalSeconds:F4} сек.");
        }

        static async Task UpdateGradeAsync()
        {
            Console.Write("Id записи для обновления: "); int id = int.Parse(Console.ReadLine()!);
            Console.Write("Новая оценка: ");              int grade = int.Parse(Console.ReadLine()!);

            var sw = Stopwatch.StartNew();

            await using var conn = await OpenConnectionAsync();
            await using var cmd = conn.CreateCommand();

            cmd.CommandText = "UPDATE Grades SET Grade = @g WHERE Id = @id";
            AddParameter(cmd, "@g",  grade);
            AddParameter(cmd, "@id", id);

            int rows = await cmd.ExecuteNonQueryAsync();
            sw.Stop();

            Console.WriteLine($"Обновлено строк: {rows}. {sw.Elapsed.TotalSeconds:F4} сек.");
        }

        static async Task DeleteGradeAsync()
        {
            Console.Write("Id записи для удаления: "); int id = int.Parse(Console.ReadLine()!);

            var sw = Stopwatch.StartNew();

            await using var conn = await OpenConnectionAsync();
            await using var cmd = conn.CreateCommand();

            cmd.CommandText = "DELETE FROM Grades WHERE Id = @id";
            AddParameter(cmd, "@id", id);

            int rows = await cmd.ExecuteNonQueryAsync();
            sw.Stop();

            Console.WriteLine($"Удалено строк: {rows}. {sw.Elapsed.TotalSeconds:F4} сек.");
        }

        static void SwitchProvider()
        {
            Console.WriteLine("Доступные СУБД:");
            Console.WriteLine("  1 — SQL Server");
            Console.WriteLine("  2 — SQLite");
            Console.Write("> ");
            string? c = Console.ReadLine();

            if (c == "1")
            {
                _providerName = "Microsoft.Data.SqlClient";
                _connString = _config.GetConnectionString("SqlServer")!;
                Console.WriteLine("Переключено на SQL Server.");
            }
            else if (c == "2")
            {
                _providerName = "Microsoft.Data.Sqlite";
                _connString = _config.GetConnectionString("Sqlite")!;
                Console.WriteLine("Переключено на SQLite.");
                Console.WriteLine("Убедитесь, что файл studentgrades.db создан.");
            }
            else
            {
                Console.WriteLine("Отмена.");
            }
        }

        private static void AddParameter(DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }
    }
}