using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AdoNetLibrary
{
    class Program
    {
        static void Main(string[] args)
        {
            Program pr = new Program();

            // Раскомментируйте нужные методы по очереди:
            pr.InsertQuery();          // добавление автора
            pr.ReadData();             // чтение авторов
            pr.ReadData2();            // пакетный запрос
            pr.ExecStoredProcedure();  // вызов хранимой процедуры
            pr.HomeWork();             // домашнее задание

            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        private SqlConnection conn;

        public Program()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            conn = new SqlConnection();
            conn.ConnectionString = config.GetConnectionString("MyConnString");
        }

        // Вставка автора
        public void InsertQuery()
        {
            try
            {
                conn.Open();
                string insertString = @"insert into Authors (FirstName, LastName) values ('Roger', 'Zelazny')";
                SqlCommand cmd = new SqlCommand(insertString, conn);
                cmd.ExecuteNonQuery();
                Console.WriteLine("Автор добавлен.");
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        // Чтение таблицы Authors
        public void ReadData()
        {
            SqlDataReader rdr = null;
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from Authors", conn);
                rdr = cmd.ExecuteReader();

                int line = 0;
                while (rdr.Read())
                {
                    if (line == 0)
                    {
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            Console.Write(rdr.GetName(i).ToString() + "\t");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine(rdr["Id"] + "\t" + rdr["FirstName"] + "\t" + rdr["LastName"]);
                    line++;
                }
                Console.WriteLine("Обработано записей: " + line);
            }
            finally
            {
                if (rdr != null) rdr.Close();
                if (conn != null) conn.Close();
            }
        }

        // Пакетная обработка двух запросов
        public void ReadData2()
        {
            SqlDataReader rdr = null;
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select * from Authors; select * from Books", conn);
                rdr = cmd.ExecuteReader();

                int line = 0;
                do
                {
                    while (rdr.Read())
                    {
                        if (line == 0)
                        {
                            for (int i = 0; i < rdr.FieldCount; i++)
                            {
                                Console.Write(rdr.GetName(i).ToString() + "\t");
                            }
                            Console.WriteLine();
                        }
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            Console.Write(rdr[i].ToString() + "\t");
                        }
                        Console.WriteLine();
                        line++;
                    }
                } while (rdr.NextResult());

                Console.WriteLine("Всего записей: " + line);
            }
            finally
            {
                if (rdr != null) rdr.Close();
                if (conn != null) conn.Close();
            }
        }

        // Вызов хранимой процедуры
        public void ExecStoredProcedure()
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("getBooksNumber", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@AuthorId", SqlDbType.Int).Value = 1;

            SqlParameter outputParam = new SqlParameter("@BookCount", SqlDbType.Int);
            outputParam.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();

            Console.WriteLine("Количество книг автора: " + cmd.Parameters["@BookCount"].Value.ToString());
            conn.Close();
        }

        // Домашнее задание: подсчёт суммы цен и страниц
        public void HomeWork()
        {
            conn.Open();

            // 1. Узнаём количество книг
            SqlCommand cmdCount = new SqlCommand("Select count(id) from Books", conn);
            int num = (int)cmdCount.ExecuteScalar();
            Console.WriteLine("Количество книг: " + num);

            // 2. Читаем все книги
            SqlCommand cmdBooks = new SqlCommand("Select * from Books", conn);
            SqlDataReader rdr = cmdBooks.ExecuteReader();

            int totalPrice = 0;
            int totalPages = 0;

            for (int i = 0; i < num; i++)
            {
                if (rdr.Read())
                {
                    int id       = rdr.GetInt32(0);
                    string title = rdr.GetString(1);
                    int authorId = rdr.GetInt32(2);
                    int price    = rdr.GetInt32(3);
                    int pages    = rdr.GetInt32(4);

                    totalPrice += price;
                    totalPages += pages;

                    Console.WriteLine($"Id={id}, Title={title}, AuthorId={authorId}, Price={price}, Pages={pages}");
                }
            }

            rdr.Close();
            conn.Close();

            Console.WriteLine($"Суммарная цена всех книг: {totalPrice}");
            Console.WriteLine($"Суммарное количество страниц: {totalPages}");
        }
    }
}
