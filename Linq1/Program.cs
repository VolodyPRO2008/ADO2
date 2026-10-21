using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LinqLab
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Task1_QuerySyntax();
            Task2_MethodSyntax();
            Task3_Agreement();
            Task4_Xml();

            Console.WriteLine("\nНажмите Enter для выхода...");
            Console.ReadLine();
        }

        private static List<Student> GetStudents()
        {
            return new List<Student>
            {
                new Student { Name = "Алексей",  Group = "ИТ-41", Age = 20, GPA = 4.2, PassedCredits = 40 },
                new Student { Name = "Борис",    Group = "ИТ-41", Age = 19, GPA = 3.1, PassedCredits = 35 },
                new Student { Name = "Виктория", Group = "ПИ-42", Age = 21, GPA = 4.8, PassedCredits = 45 },
                new Student { Name = "Григорий", Group = "ИБ-43", Age = 20, GPA = 2.9, PassedCredits = 20 },
                new Student { Name = "Дарья",    Group = "ИТ-41", Age = 22, GPA = 4.5, PassedCredits = 42 },
                new Student { Name = "Елена",    Group = "ПИ-42", Age = 20, GPA = 3.8, PassedCredits = 38 },
                new Student { Name = "Жанна",    Group = "ИБ-43", Age = 23, GPA = 4.9, PassedCredits = 48 },
                new Student { Name = "Игорь",    Group = "ИТ-41", Age = 19, GPA = 2.5, PassedCredits = 15 }
            };
        }

        static void Task1_QuerySyntax()
        {
            Console.WriteLine("\tЗадание 1: Студенты ИТ-41 с баллом > 3.5");
            Console.WriteLine($"{"Имя",-10} | {"Группа",-6} | {"Возраст",-7} | {"Балл",-4} | {"Зачеты",-6}");

            var students = GetStudents();

            var query =
                from s in students
                where s.Group == "ИТ-41" && s.GPA > 3.5
                orderby s.Age
                select s;

            foreach (var s in query)
                Console.WriteLine(s);
        }

        static void Task2_MethodSyntax()
        {
            Console.WriteLine("\n\tЗадание 2: Стипендии");

            var students = GetStudents();

            var query = students
                .Where(s => s.GPA >= 4.0)
                .OrderByDescending(s => s.GPA)
                .Select(s => new
                {
                    FullName    = s.Name,
                    Scholarship = s.GPA >= 4.7 ? "Ректорская"
                                : s.GPA >= 4.5 ? "Повышенная"
                                : s.GPA >= 4.0 ? "Стандартная"
                                : "Нет"
                });

            foreach (var item in query)
                Console.WriteLine($"{item.FullName}: {item.Scholarship}");
        }

        static void Task3_Agreement()
        {
            Console.WriteLine("\n\tЗадание 3: Статистика");

            var students = GetStudents();

            var groups = students
                .GroupBy(s => s.Group)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Group = g.Key,
                    Avg   = g.Average(s => s.GPA)
                });

            foreach (var g in groups)
                Console.WriteLine($"Группа {g.Group}: Средний балл {g.Avg:F2}");

            var oldest = students
                .Where(s => s.PassedCredits == 45)
                .OrderByDescending(s => s.Age)
                .FirstOrDefault();

            Console.WriteLine();
            if (oldest != null)
                Console.WriteLine($"Студент с 45 зачетами: {oldest.Name} ({oldest.Age} год)");
            else
                Console.WriteLine("Студента с 45 зачетами не найдено.");
        }
        static void Task4_Xml()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "uni.xml");

            XDocument doc = new XDocument(
                new XElement("university",
                    new XElement("course",
                        new XAttribute("name", "Программирование"),
                        new XElement("student", new XAttribute("name", "Кирилл"),  new XAttribute("gpa", "3.9")),
                        new XElement("student", new XAttribute("name", "Лариса"),  new XAttribute("gpa", "4.7")),
                        new XElement("student", new XAttribute("name", "Михаил"),  new XAttribute("gpa", "3.2")),
                        new XElement("student", new XAttribute("name", "Наталья"), new XAttribute("gpa", "4.1"))
                    )
                )
            );
            doc.Save(path);
            Console.WriteLine($"\nXML сохранён: {path}");

            XDocument loaded = XDocument.Load(path);

            var topStudents =
                from s in loaded.Descendants("student")
                let gpa = double.Parse(s.Attribute("gpa").Value,
                                        System.Globalization.CultureInfo.InvariantCulture)
                where gpa > 4.0
                select s.Attribute("name").Value;

            Console.WriteLine("\tЗадание 4: Отличникиd");
            foreach (var name in topStudents)
                Console.WriteLine(name);
        }
    }
}