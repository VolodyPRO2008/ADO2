using System;
using System.Linq;

namespace CountriesApp
{
    class Program
    {
        static void Main()
        {
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CountriesDB;Integrated Security=True;";
            var db = new CountriesDataContext(connectionString);

            Console.WriteLine("Задание 1");
            var newCountry = new Country { Name = "Испания", PartOfWorld = "Европа", Area = 505990 };
            db.Countries.InsertOnSubmit(newCountry);
            db.SubmitChanges();

            var newCapital = new Capital { CountryId = newCountry.Id, Name = "Мадрид", Population = 3300000 };
            db.Capitals.InsertOnSubmit(newCapital);
            db.SubmitChanges();
            Console.WriteLine("Страна и столица добавлены.");

            Console.WriteLine("\nЗадание 2");
            var countryToUpdate = db.Countries.FirstOrDefault(c => c.Name == "Испания");
            if (countryToUpdate != null)
            {
                countryToUpdate.Area = 506000;
                db.SubmitChanges();
                Console.WriteLine("Площадь Испании обновлена.");
            }

            Console.WriteLine("\nЗадание 3");
            var countryToDelete = db.Countries.FirstOrDefault(c => c.Name == "Испания");
            if (countryToDelete != null)
            {
                var relatedCapitals = db.Capitals.Where(c => c.CountryId == countryToDelete.Id);
                db.Capitals.DeleteAllOnSubmit(relatedCapitals);

                db.Countries.DeleteOnSubmit(countryToDelete);
                db.SubmitChanges();
                Console.WriteLine("Испания и её столица удалены.");
            }

            Console.WriteLine("\nЗадание 4");

            Console.WriteLine("Топ-3 стран по населению:");
            foreach (var c in db.GetTop3CountriesByPopulation())
                Console.WriteLine($"{c.Name} - {c.Population}");

            Console.WriteLine("\nТоп-3 столиц по населению:");
            foreach (var c in db.GetTop3CapitalsByPopulation())
                Console.WriteLine($"{c.Name} - {c.Population}");

            var maxCountry = db.GetCountryWithMaxPopulation().FirstOrDefault();
            Console.WriteLine($"\nСтрана с макс. населением: {maxCountry?.Name} ({maxCountry?.Population})");

            var maxCity = db.GetCityWithMaxPopulation().FirstOrDefault();
            Console.WriteLine($"Город с макс. населением: {maxCity?.Name} ({maxCity?.Population})");
        }
    }
}