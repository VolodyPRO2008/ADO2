using EF1;
using System;
using System.Data.Entity;
using System.Linq;

namespace CarRentalDbFirst
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ЛАБОРАТОРНАЯ РАБОТА: АВТОПАРК ===\n");

            Task1_AddInitialData();
            Task2_GetAllCars();
            Task3_FindCarByModel("Camry");
            Task4_GetCarById(1);
            Task5_GetCarsWithBrandInfo();
            Task6_AddNewCar("X5", 2023, 150.00m, 2);
            Task7_UpdateCarPrice(1, 200.00m);

            Console.ReadLine();
        }

        static void Task1_AddInitialData()
        {
            Console.WriteLine("Добавление начальных данных...");
            using (CarRentalDbEntities db = new CarRentalDbEntities())
            {
                if (!db.Brands.Any())
                {
                    var toyota = new Brand { Name = "Toyota", Country = "Япония" };
                    var bmw = new Brand { Name = "BMW", Country = "Германия" };

                    db.Brands.Add(toyota);
                    db.Brands.Add(bmw);
                    db.SaveChanges();

                    db.Cars.Add(new Car { Model = "Camry", ReleaseYear = 2020, PricePerDay = 50.00m, BrandId = toyota.Id });
                    db.Cars.Add(new Car { Model = "Corolla", ReleaseYear = 2019, PricePerDay = 40.00m, BrandId = toyota.Id });
                    db.Cars.Add(new Car { Model = "X5", ReleaseYear = 2022, PricePerDay = 100.00m, BrandId = bmw.Id });
                    db.SaveChanges();

                    Console.WriteLine("Данные успешно добавлены!");
                }
                else
                {
                    Console.WriteLine("База данных уже содержит записи.");
                }
            }
        }

        static void Task2_GetAllCars()
        {
            Console.WriteLine("\n--- ЗАДАНИЕ 1: Все автомобили по алфавиту ---");
            using (CarRentalDbEntities db = new CarRentalDbEntities())
            {
                var cars = db.Cars.OrderBy(c => c.Model).ToList();

                foreach (var car in cars)
                {
                    Console.WriteLine($"{car.Model} - {car.PricePerDay} руб/день");
                }
            }
        }

        static void Task3_FindCarByModel(string modelName)
        {
            Console.WriteLine($"\n--- ЗАДАНИЕ 2: Поиск машины '{modelName}' ---");
            using (CarRentalDbEntities db = new CarRentalDbEntities())
            {
                var car = db.Cars.FirstOrDefault(c => c.Model == modelName);

                if (car != null)
                {
                    Console.WriteLine($"Модель: {car.Model}, Год: {car.ReleaseYear}");
                }
                else
                {
                    Console.WriteLine("Машина не найдена");
                }
            }
        }

        static void Task4_GetCarById(int id)
        {
            Console.WriteLine($"\n--- ЗАДАНИЕ 3: Поиск по ID ({id}) ---");
            using (CarRentalDbEntities db = new CarRentalDbEntities())
            {
                var car = db.Cars.Find(id);

                if (car != null)
                {
                    Console.WriteLine($"Модель: {car.Model}");
                }
                else
                {
                    Console.WriteLine("Машина не найдена");
                }
            }
        }

        static void Task5_GetCarsWithBrandInfo()
        {
            Console.WriteLine("\n--- ЗАДАНИЕ 4: Автомобили с названием марки ---");
            using (CarRentalDbEntities db = new CarRentalDbEntities())
            {
                var cars = db.Cars.ToList();

                foreach (var car in cars)
                {
                    Console.WriteLine($"Модель: {car.Model} | Марка: {car.Brand.Name} | Страна: {car.Brand.Country}");
                }
            }
        }

        static void Task6_AddNewCar(string model, int year, decimal price, int brandId)
        {
            Console.WriteLine($"\n--- ЗАДАНИЕ 5: Добавление машины '{model}' ---");
            using (CarRentalDbEntities db = new CarRentalDbEntities())
            {
                var newCar = new Car
                {
                    Model = model,
                    ReleaseYear = year,
                    PricePerDay = price,
                    BrandId = brandId
                };

                db.Cars.Add(newCar);
                db.SaveChanges();
                Console.WriteLine("Машина успешно добавлена.");
            }
        }

        static void Task7_UpdateCarPrice(int carId, decimal newPrice)
        {
            Console.WriteLine($"\n--- ЗАДАНИЕ 6: Обновление цены машины (ID: {carId}) ---");
            using (CarRentalDbEntities db = new CarRentalDbEntities())
            {
                var car = db.Cars.Find(carId);

                if (car != null)
                {
                    car.PricePerDay = newPrice;
                    db.SaveChanges();
                    Console.WriteLine($"Цена обновлена на {newPrice}.");
                }
                else
                {
                    Console.WriteLine("Машина не найдена.");
                }
            }
        }
    }
}