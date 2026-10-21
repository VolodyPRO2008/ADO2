using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AdoNetLab2
{
    internal class Program
    {
        static string ConnString;

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            ConnString = config.GetConnectionString("MyConnString");
            if (string.IsNullOrWhiteSpace(ConnString))
            {
                Console.WriteLine("Не найдена строка подключения MyConnString в appsettings.json");
                return;
            }

            Task1_ManualDataTable();
            Task2_FillDataSet();
            Task3_AutoUpdate();
            Task4_CustomUpdate();
            Task5_RowStateOrder();
            Task6_TableMappings();

            Console.WriteLine("\nГотово. Нажмите любую клавишу...");
            Console.ReadKey();
        }

        static void PrintTable(DataTable table, string title = "")
        {
            if (!string.IsNullOrEmpty(title))
                Console.WriteLine($"\n\t{title}");

            foreach (DataColumn col in table.Columns)
                Console.Write(col.ColumnName.PadRight(18));
            Console.WriteLine();
            Console.WriteLine(new string('-', table.Columns.Count * 18));

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    object val = row[col];
                    string s = (val == null || val == DBNull.Value) ? "NULL" : val.ToString();
                    if (s.Length > 16) s = s.Substring(0, 16) + "..";
                    Console.Write(s.PadRight(18));
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        static void Task1_ManualDataTable()
        {
            Console.WriteLine("\tЗадание 1. DataTable");

            DataTable table = new DataTable("Authors");
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("FirstName", typeof(string));
            table.Columns.Add("LastName", typeof(string));

            object[,] data =
            {
                { 1, "Isaac",    "Asimov"   },
                { 2, "Ray",      "Bradbury" },
                { 3, "Clifford", "Simak"    },
                { 4, "Roger",    "Zelazny"  }
            };

            for (int i = 0; i < data.GetLength(0); i++)
            {
                DataRow row = table.NewRow();
                row["Id"]        = data[i, 0];
                row["FirstName"] = data[i, 1];
                row["LastName"]  = data[i, 2];
                table.Rows.Add(row);
            }

            PrintTable(table, "Authors (manual)");

            Console.WriteLine("Доступ через row[0]         : " + table.Rows[0][0]);
            Console.WriteLine("Доступ через row[\"FirstName\"]: " + table.Rows[0]["FirstName"]);
        }

        static void Task2_FillDataSet()
        {
            Console.WriteLine("\tЗадание 2. DataSet + SqlDataAdapter");

            using SqlConnection conn = new SqlConnection(ConnString);
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Books", conn);

            DataSet ds = new DataSet();
            int rows = da.Fill(ds, "Books");

            Console.WriteLine($"Загружено строк: {rows}");
            Console.WriteLine($"Имя таблицы в DataSet: {ds.Tables[0].TableName}");

            PrintTable(ds.Tables["Books"], "Books из БД");

            int rows2 = da.Fill(ds, "Books");
            Console.WriteLine($"Повторный Fill вернул: {rows2} (строк в таблице: {ds.Tables["Books"].Rows.Count})");
        }

        static void Task3_AutoUpdate()
        {
            Console.WriteLine("\tЗадание 3. Update через SqlCommandBuilder");

            using SqlConnection conn = new SqlConnection(ConnString);
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Books", conn);

            SqlCommandBuilder cmdBuilder = new SqlCommandBuilder(da);

            DataSet ds = new DataSet();
            da.Fill(ds, "Books");

            Console.WriteLine("До локального изменения");
            PrintTable(ds.Tables["Books"], "Books");

            ds.Tables["Books"].Rows[0]["Price"] = 999;

            Console.WriteLine("После локального изменения (в памяти)");
            PrintTable(ds.Tables["Books"], "Books");

            Console.WriteLine("INSERT: " + cmdBuilder.GetInsertCommand().CommandText);
            Console.WriteLine("UPDATE: " + cmdBuilder.GetUpdateCommand().CommandText);
            Console.WriteLine("DELETE: " + cmdBuilder.GetDeleteCommand().CommandText);

            int affected = da.Update(ds, "Books");
            Console.WriteLine($"\nUpdate затронул строк: {affected}");

            DataSet ds2 = new DataSet();
            da.Fill(ds2, "Books");
            PrintTable(ds2.Tables["Books"], "Books после сохранения в БД");

            Console.WriteLine("Проблемный эксперимент: SELECT без Id");
            try
            {
                SqlDataAdapter bad = new SqlDataAdapter("SELECT Title, Price, Pages FROM Books", conn);
                SqlCommandBuilder badBuilder = new SqlCommandBuilder(bad);
                DataSet badDs = new DataSet();
                bad.Fill(badDs, "Books");
                badDs.Tables["Books"].Rows[0]["Price"] = 123;
                bad.Update(badDs, "Books");
                Console.WriteLine("Ошибка не возникла (неожиданно!)");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Тип: " + ex.GetType().Name);
                Console.WriteLine("Сообщение: " + ex.Message);
            }
        }

        static void Task4_CustomUpdate()
        {
            Console.WriteLine("\tЗадание 4. Свой UpdateCommand");

            using SqlConnection conn = new SqlConnection(ConnString);
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Books", conn);

            SqlCommand updateCmd = new SqlCommand(
                "UPDATE Books SET Price = @pPrice WHERE Id = @pId", conn);

            updateCmd.Parameters.Add(new SqlParameter("@pPrice", SqlDbType.Int));
            updateCmd.Parameters["@pPrice"].SourceVersion = DataRowVersion.Current;
            updateCmd.Parameters["@pPrice"].SourceColumn  = "Price";

            updateCmd.Parameters.Add(new SqlParameter("@pId", SqlDbType.Int));
            updateCmd.Parameters["@pId"].SourceVersion = DataRowVersion.Original;
            updateCmd.Parameters["@pId"].SourceColumn  = "Id";

            da.UpdateCommand = updateCmd;

            DataSet ds = new DataSet();
            da.Fill(ds, "Books");

            ds.Tables["Books"].Rows[1]["Price"] = 111;
            ds.Tables["Books"].Rows[2]["Price"] = 222;

            int affected = da.Update(ds, "Books");
            Console.WriteLine($"Update затронул строк: {affected}");

            DataSet ds2 = new DataSet();
            da.Fill(ds2, "Books");
            PrintTable(ds2.Tables["Books"], "Books после кастомного Update");
        }

        static void Task5_RowStateOrder()
        {
            Console.WriteLine("\tЗадание 5. Порядок синхронизации");

            using SqlConnection conn = new SqlConnection(ConnString);
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Books", conn);
            SqlCommandBuilder cb = new SqlCommandBuilder(da);

            DataSet ds = new DataSet();
            da.Fill(ds, "Books");
            DataTable books = ds.Tables["Books"];

            books.Rows[0].Delete();

            if (books.Rows.Count > 0)
                books.Rows[0]["Price"] = 777;

            DataRow newRow = books.NewRow();
            newRow["AuthorId"] = 1;
            newRow["Title"]    = "New Book";
            newRow["Price"]    = 500;
            newRow["Pages"]    = 100;
            books.Rows.Add(newRow);

            Console.WriteLine("Состояния строк до Update");
            foreach (DataRow row in books.Rows)
                Console.WriteLine($"  {row["Title"],20} -> {row.RowState}");

            da.Update(books.Select(null, null, DataViewRowState.Deleted));
            da.Update(books.Select(null, null, DataViewRowState.ModifiedCurrent));
            da.Update(books.Select(null, null, DataViewRowState.Added));

            DataSet ds2 = new DataSet();
            da.Fill(ds2, "Books");
            PrintTable(ds2.Tables["Books"], "Books после синхронизации");
        }

        static void Task6_TableMappings()
        {
            Console.WriteLine("\tЗадание 6. TableMappings");

            using SqlConnection conn = new SqlConnection(ConnString);

            string sql = "SELECT Id, FirstName, LastName FROM Authors; SELECT Id, Title, Price FROM Books;";

            SqlDataAdapter daNoMap = new SqlDataAdapter(sql, conn);
            DataSet dsNoMap = new DataSet();
            daNoMap.Fill(dsNoMap);
            Console.WriteLine("Без TableMappings имена таблиц:");
            foreach (DataTable t in dsNoMap.Tables)
                Console.WriteLine("  " + t.TableName);

            SqlDataAdapter da2 = new SqlDataAdapter(sql, conn);

            DataTableMapping dtmAuthors = da2.TableMappings.Add("Table", "Authors");
            dtmAuthors.ColumnMappings.Add("Id",        "Num");
            dtmAuthors.ColumnMappings.Add("FirstName", "Name");
            dtmAuthors.ColumnMappings.Add("LastName",  "Surname");

            da2.TableMappings.Add("Table1", "Books");

            DataSet ds = new DataSet();
            da2.Fill(ds);

            Console.WriteLine("\nС TableMappings имена таблиц:");
            foreach (DataTable t in ds.Tables)
                Console.WriteLine("  " + t.TableName);

            PrintTable(ds.Tables["Authors"], "Authors (переименованные колонки)");
            PrintTable(ds.Tables["Books"],   "Books");
        }
    }
}