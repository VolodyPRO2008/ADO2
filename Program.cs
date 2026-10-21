using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FruitsApp
{
    class Program
    {
        static async Task Main()
        {
            var configs = new Dictionary<string, (string Provider, string ConnStr)>
            {
                ["1"] = ("Microsoft.Data.SqlClient", "Server=(localdb)\\MSSQLLocalDB;Database=FruitsDB;Integrated Security=True;TrustServerCertificate=True;"),
                ["2"] = ("Microsoft.Data.Sqlite", "Data Source=fruits.db")
            };

            while (true)
            {
                Console.WriteLine("1-SQL Server, 2-SQLite, 0-выход");
                var c = Console.ReadLine();
                if (c == "0") break;
                if (!configs.ContainsKey(c)) continue;

                var prov = configs[c].Provider;
                var cs = configs[c].ConnStr;
                var repo = new Repository(prov, cs);
                await repo.EnsureDbAsync();
                await repo.EnsureTableAsync();

                bool back = false;
                while (!back)
                {
                    Console.WriteLine("1: чтение, 2:добавить, 3: обновить, 4: удалить, 0: назад");
                    var op = Console.ReadLine();
                    try
                    {
                        var sw = Stopwatch.StartNew();
                        switch (op)
                        {
                            case "1":
                                var items = await repo.GetAllAsync();
                                sw.Stop();
                                foreach (var i in items) Console.WriteLine(i.Id + " " + i.Name + " " + i.Type + " " + i.Calories);
                                break;
                            case "2":
                                Console.Write("Name: "); var n = Console.ReadLine();
                                Console.Write("Type: "); var t = Console.ReadLine();
                                Console.Write("Cal: "); var cal = int.Parse(Console.ReadLine());
                                await repo.InsertAsync(n, t, cal);
                                sw.Stop();
                                break;
                            case "3":
                                Console.Write("Id: "); var id = int.Parse(Console.ReadLine());
                                Console.Write("Name: "); var n2 = Console.ReadLine();
                                Console.Write("Cal: "); var cal2 = int.Parse(Console.ReadLine());
                                await repo.UpdateAsync(id, n2, cal2);
                                sw.Stop();
                                break;
                            case "4":
                                Console.Write("Id: "); var id2 = int.Parse(Console.ReadLine());
                                await repo.DeleteAsync(id2);
                                sw.Stop();
                                break;
                            case "0":
                                back = true;
                                continue;
                            default:
                                continue;
                        }
                        Console.WriteLine("Время: " + sw.Elapsed.TotalSeconds.ToString("F4") + " сек.");
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
            }
        }
    }

    public class Item
    {
        public int Id;
        public string Name = "";
        public string Type = "";
        public int Calories;
    }

    public class Repository
    {
        private readonly DbProviderFactory _f;
        private readonly string _cs;
        private readonly bool _isSqlServer;

        public Repository(string p, string cs)
        {
            _f = DbProviderFactories.GetFactory(p);
            _cs = cs;
            _isSqlServer = p == "Microsoft.Data.SqlClient";
        }

        private async Task<DbConnection> OpenAsync()
        {
            var c = _f.CreateConnection();
            c.ConnectionString = _cs;
            await c.OpenAsync();
            return c;
        }

        public async Task EnsureDbAsync()
        {
            if (!_isSqlServer) return;

            var master = _cs.Replace("Database=FruitsDB", "Database=master");
            using (var c = new SqlConnection(master))
            {
                await c.OpenAsync();
                using (var cmd = c.CreateCommand())
                {
                    cmd.CommandText = "IF DB_ID('FruitsDB') IS NULL CREATE DATABASE FruitsDB;";
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task EnsureTableAsync()
        {
            using (var c = await OpenAsync())
            using (var cmd = _f.CreateCommand())
            {
                cmd.Connection = c;
                cmd.CommandText = _isSqlServer
                    ? "IF OBJECT_ID('FruitsAndVegetables','U') IS NULL CREATE TABLE FruitsAndVegetables (Id INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(100) NOT NULL, Type NVARCHAR(20) NOT NULL, Calories INT NOT NULL);"
                    : "CREATE TABLE IF NOT EXISTS FruitsAndVegetables (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Type TEXT NOT NULL, Calories INTEGER NOT NULL);";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Item>> GetAllAsync()
        {
            var list = new List<Item>();
            using (var c = await OpenAsync())
            using (var cmd = _f.CreateCommand())
            {
                cmd.Connection = c;
                cmd.CommandText = "SELECT Id, Name, Type, Calories FROM FruitsAndVegetables";
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        list.Add(new Item
                        {
                            Id = r.GetInt32(0),
                            Name = r.GetString(1),
                            Type = r.GetString(2),
                            Calories = r.GetInt32(3)
                        });
                    }
                }
            }
            return list;
        }

        public async Task InsertAsync(string n, string t, int cal)
        {
            using (var c = await OpenAsync())
            using (var cmd = _f.CreateCommand())
            {
                cmd.Connection = c;
                cmd.CommandText = "INSERT INTO FruitsAndVegetables (Name, Type, Calories) VALUES (@n,@t,@c)";
                P(cmd, "@n", n); P(cmd, "@t", t); P(cmd, "@c", cal);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(int id, string n, int cal)
        {
            using (var c = await OpenAsync())
            using (var cmd = _f.CreateCommand())
            {
                cmd.Connection = c;
                cmd.CommandText = "UPDATE FruitsAndVegetables SET Name=@n, Calories=@c WHERE Id=@id";
                P(cmd, "@n", n); P(cmd, "@c", cal); P(cmd, "@id", id);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var c = await OpenAsync())
            using (var cmd = _f.CreateCommand())
            {
                cmd.Connection = c;
                cmd.CommandText = "DELETE FROM FruitsAndVegetables WHERE Id=@id";
                P(cmd, "@id", id);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private void P(DbCommand cmd, string n, object v)
        {
            var p = _f.CreateParameter();
            p.ParameterName = n;
            p.Value = v;
            cmd.Parameters.Add(p);
        }
    }
}