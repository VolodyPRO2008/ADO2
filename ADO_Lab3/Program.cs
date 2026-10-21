using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AdoNetLibrary
{
    class Program
    {
        private static IConfiguration _config;
        private static string _libraryCs;
        private static string _library2Cs;

        private const string SqlProviderName = "Microsoft.Data.SqlClient";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding  = System.Text.Encoding.UTF8;

            _config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _libraryCs  = _config.GetConnectionString("Library");
            _library2Cs = _config.GetConnectionString("Library2");

            DbProviderFactories.RegisterFactory(
                SqlProviderName,
                Microsoft.Data.SqlClient.SqlClientFactory.Instance);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. Список поставщиков данных");
                Console.WriteLine("2. Запрос через DbProviderFactory");
                Console.WriteLine("3. DataViewManager — фильтр и сортировка");
                Console.WriteLine("4. Успешная транзакция");
                Console.WriteLine("5. Транзакция с откатом");
                Console.WriteLine("0. Выход");
                Console.Write("\n> ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1": ShowProviders(); break;
                        case "2": RunQueryViaFactory(); break;
                        case "3": DemoDataViewManager(); break;
                        case "4": SuccessfulTransaction(); break;
                        case "5": FailedTransaction(); break;
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

        static void ShowProviders()
        {
            DataTable providers = DbProviderFactories.GetFactoryClasses();
            Console.WriteLine("Зарегистрированные поставщики\n");
            Console.WriteLine($"{"InvariantName",-40} | Description");
            Console.WriteLine(new string('-', 90));
            foreach (DataRow row in providers.Rows)
            {
                Console.WriteLine($"{row["InvariantName"],-40} | {row["Description"]}");
            }
            Console.WriteLine("\nВсего найдено: " + providers.Rows.Count);
            Console.WriteLine("\nПримечание: в .NET Core список пуст, пока вы не");
            Console.WriteLine("зарегистрируете провайдеры вручную через");
            Console.WriteLine("DbProviderFactories.RegisterFactory(). Мы это сделали в Main.");
        }

        static void RunQueryViaFactory()
        {
            Console.WriteLine("Выберите базу данных:");
            Console.WriteLine("  1 — Library  (основная)");
            Console.WriteLine("  2 — Library2");
            Console.Write("> ");
            string choice = Console.ReadLine()?.Trim();

            string cs = choice switch
            {
                "1" => _libraryCs,
                "2" => _library2Cs,
                _   => null
            };

            if (cs == null)
            {
                Console.WriteLine("Неверный выбор.");
                return;
            }

            DbProviderFactory factory = DbProviderFactories.GetFactory(SqlProviderName);

            using DbConnection conn = factory.CreateConnection();
            conn.ConnectionString = cs;

            using DbCommand cmd = factory.CreateCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT * FROM Books";

            DbDataAdapter adapter = factory.CreateDataAdapter();
            adapter.SelectCommand = cmd;

            DataTable dt = new DataTable();
            conn.Open();
            adapter.Fill(dt);
            conn.Close();

            Console.WriteLine("\nРезультат SELECT * FROM Books\n");
            PrintDataTable(dt);
        }

        static void DemoDataViewManager()
        {
            DataSet set = new DataSet();
            using (var conn = new SqlConnection(_libraryCs))
            {
                var adapter = new SqlDataAdapter("SELECT * FROM Authors", conn);
                adapter.Fill(set, "Authors");
            }

            Console.WriteLine("Исходные данные\n");
            PrintDataTable(set.Tables["Authors"]);

            DataViewManager dvm = new DataViewManager(set);
            dvm.DataViewSettings["Authors"].RowFilter = "Id < 100";
            dvm.DataViewSettings["Authors"].Sort      = "LastName ASC";

            DataView dv = dvm.CreateDataView(set.Tables["Authors"]);
            Console.WriteLine("\nПосле фильтра (Id < 100) и сортировки (LastName ASC)\n");
            PrintDataView(dv);

            Console.WriteLine("\nИсходные данные после фильтрации\n");
            PrintDataTable(set.Tables["Authors"]);
        }

        static void SuccessfulTransaction()
        {
            using (var c = new SqlConnection(_libraryCs))
            {
                c.Open();
                new SqlCommand(
                    "IF OBJECT_ID('tmp3') IS NOT NULL DROP TABLE tmp3", c)
                    .ExecuteNonQuery();
            }

            using var conn = new SqlConnection(_libraryCs);
            SqlTransaction tran = null;
            try
            {
                conn.Open();
                tran = conn.BeginTransaction();

                SqlCommand comm = conn.CreateCommand();
                comm.Transaction = tran;

                comm.CommandText = "CREATE TABLE tmp3 (id INT IDENTITY PRIMARY KEY, f1 VARCHAR(20), f2 INT)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Создана таблица tmp3.");

                comm.CommandText = "INSERT INTO tmp3(f1, f2) VALUES('Hello', 100)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Вставлена строка 1.");

                comm.CommandText = "INSERT INTO tmp3(f1, f2) VALUES('World', 200)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Вставлена строка 2.");

                tran.Commit();
                Console.WriteLine("\nТранзакция зафиксирована.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                tran?.Rollback();
                Console.WriteLine("Транзакция отклонена.");
            }

            using (var c = new SqlConnection(_libraryCs))
            {
                c.Open();
                int count = (int)new SqlCommand("SELECT COUNT(*) FROM tmp3", c).ExecuteScalar();
                Console.WriteLine($"\nПроверка: строк в tmp3 = {count} (ожидается 2).");
            }

            using (var c = new SqlConnection(_libraryCs))
            {
                c.Open();
                new SqlCommand("DROP TABLE tmp3", c).ExecuteNonQuery();
                Console.WriteLine("Таблица tmp3 удалена.");
            }
        }

        static void FailedTransaction()
        {
            using (var c = new SqlConnection(_libraryCs))
            {
                c.Open();
                new SqlCommand(
                    "IF OBJECT_ID('tmp3') IS NOT NULL DROP TABLE tmp3", c)
                    .ExecuteNonQuery();
            }

            using var conn = new SqlConnection(_libraryCs);
            SqlTransaction tran = null;
            try
            {
                conn.Open();
                tran = conn.BeginTransaction();

                SqlCommand comm = conn.CreateCommand();
                comm.Transaction = tran;

                comm.CommandText = "CREATE TABLE tmp3 (id INT IDENTITY PRIMARY KEY, f1 VARCHAR(20), f2 INT)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Создана таблица tmp3.");

                comm.CommandText = "INSERT INTO tmp3(f1, f2) VALUES('Hello', 100)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Вставлена строка 1.");

                comm.CommandText = "INSERT INTO tmp4(f1, f2) VALUES('World', 200)";
                comm.ExecuteNonQuery();
                Console.WriteLine("Эта строка НЕ должна выполниться.");

                tran.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nОшибка: " + ex.Message);
                tran?.Rollback();
                Console.WriteLine("Транзакция откачена — все шаги отменены.");
            }

            using (var c = new SqlConnection(_libraryCs))
            {
                c.Open();
                int count = (int)new SqlCommand(
                    "SELECT COUNT(*) FROM sys.tables WHERE name='tmp3'", c)
                    .ExecuteScalar();
                Console.WriteLine($"\nПроверка: таблиц tmp3 в БД = {count} (ожидается 0).");
            }
        }

        static void PrintDataTable(DataTable dt)
        {
            if (dt.Rows.Count == 0)
            {
                Console.WriteLine("(нет данных)");
                return;
            }

            foreach (DataColumn col in dt.Columns)
                Console.Write(col.ColumnName.PadRight(15) + "| ");
            Console.WriteLine();
            Console.WriteLine(new string('-', 17 * dt.Columns.Count));

            foreach (DataRow row in dt.Rows)
            {
                foreach (var v in row.ItemArray)
                    Console.Write((v?.ToString() ?? "").PadRight(15) + "| ");
                Console.WriteLine();
            }
        }

        static void PrintDataView(DataView dv)
        {
            foreach (DataColumn col in dv.Table.Columns)
                Console.Write(col.ColumnName.PadRight(15) + "| ");
            Console.WriteLine();
            Console.WriteLine(new string('-', 17 * dv.Table.Columns.Count));

            foreach (DataRowView row in dv)
            {
                foreach (var v in row.Row.ItemArray)
                    Console.Write((v?.ToString() ?? "").PadRight(15) + "| ");
                Console.WriteLine();
            }
        }
    }
}