using FurnitureFactory.Areas.FurnitureFactory.Models;

namespace FurnitureFactory.Areas.FurnitureFactory.Data;

public static class DbInitializer
{
    public static void Initialize(AcmeDataContext db)
    {
        db.Database.EnsureCreated();

        if (db.OrderDetails.Any()) return; // База данных инициализирована

        const int customersNumber = 100;
        const int employeesNumber = 100;
        const int furnituresNumber = 100;
        const int ordersNumber = 10000;
        const int orderDetailsNumber = 10000;

        Random randObj = new(1);

        string[] personFirstVoc =
        {
            "Dmitry", "Eugene", "Daniel", "Denis", "Vasily", "Olga", "Natalia", "Ivan", "Svetlana", "Pavel", "Anna",
            "Sergey", "Maria", "Andrey", "Yulia", "Alexey", "Ekaterina", "Mikhail", "Yana"
        };
        string[] personLastVoc =
        {
            "Valuev", "Ivanov", "Bondarev", "Demidenko", "Ermolin", "Kuznetsov", "Popov", "Sokolov", "Mironov",
            "Lebedev", "Novikov", "Petrov", "Volkov", "Solovyov", "Morozov", "Fedorov", "Sergeev", "Kozlov", "Egorov"
        };
        string[] personMiddleVoc =
        {
            "Osipovich", "Alexandrovich", "Dmitrievich", "Abkhazievich", "Sergeevich", "Vladimirovich", "Nikolaevich",
            "Igorevich", "Petrovich", "Olegovich", "Anatolyevich", "Yuryevich", "Vitalievich", "Yaroslavovich",
            "Valentinovich", "Borisovich", "Ivanovich", "Dmitrievich", "Vladovich"
        };
        string[] materialVoc =
        {
            "Steel", "Platinum", "Aluminum", "PET", "Cast Iron", "Aluminum", "Steel", "Copper", "Bronze", "Titanium",
            "Wood", "Glass", "Concrete", "Silver", "Gold", "Brass", "Ceramic", "Rubber", "Plastic"
        };
        string[] positionVoc =
        {
            "Administrator", "Mechanic", "Supervisor", "Salesperson", "Janitor", "Engineer", "Manager", "Developer",
            "Analyst", "Designer", "Technician", "Consultant", "Specialist", "Coordinator", "Inspector", "Assistant",
            "Operator", "Driver", "Cashier", "Waiter"
        };
        string[] educationVoc =
        {
            "Secondary", "Higher", "None", "Technical", "Bachelor's", "Master's", "Ph.D.", "Vocational", "Diploma",
            "Certificate", "Associate", "Graduate", "Postgraduate", "Professional", "Specialized", "Doctorate",
            "Undergraduate", "Advanced", "Self-Taught", "Apprenticeship"
        };


        for (var i = 1; i <= customersNumber; i++)
        {
            var customer = new Customer
            {
                CompanyName = $"Company {i}",
                RepresentativeLastName = personLastVoc[randObj.Next(personLastVoc.Length)],
                RepresentativeFirstName = personFirstVoc[randObj.Next(personFirstVoc.Length)],
                RepresentativeMiddleName = personMiddleVoc[randObj.Next(personMiddleVoc.Length)],
                PhoneNumber = $"123-456-789{i.ToString().PadLeft(2, '0')}",
                Address = $"Address {i}"
            };
            db.Customers.Add(customer);
        }

        db.SaveChanges();

        for (var i = 1; i <= employeesNumber; i++)
        {
            var employee = new Employee
            {
                LastName = personLastVoc[randObj.Next(personLastVoc.Length)],
                FirstName = personFirstVoc[randObj.Next(personFirstVoc.Length)],
                MiddleName = personMiddleVoc[randObj.Next(personMiddleVoc.Length)],
                Position = positionVoc[randObj.Next(positionVoc.Length)],
                Education = educationVoc[randObj.Next(educationVoc.Length)]
            };
            db.Employees.Add(employee);
        }

        db.SaveChanges();

        for (var i = 1; i <= furnituresNumber; i++)
        {
            var furniture = new Furniture
            {
                FurnitureName = $"Furniture {i}",
                Description = $"Description {i}",
                MaterialType = materialVoc[randObj.Next(materialVoc.Length)],
                Price = randObj.Next(100, 1000),
                QuantityOnHand = randObj.Next(1, 100)
            };
            db.Furnitures.Add(furniture);
        }

        db.SaveChanges();

        for (var i = 1; i <= ordersNumber; i++)
        {
            var order = new Order
            {
                OrderDate = DateTime.Now.AddDays(-randObj.Next(1, 365)),
                CustomerId = randObj.Next(1, customersNumber),
                SpecialDiscount = randObj.Next(0, 25),
                IsCompleted = randObj.Next(0, 2) == 1,
                ResponsibleEmployeeId = randObj.Next(1, employeesNumber)
            };
            db.Orders.Add(order);
        }

        db.SaveChanges();

        for (var i = 1; i <= orderDetailsNumber; i++)
        {
            var orderDetail = new OrderDetail
            {
                OrderId = randObj.Next(1, ordersNumber),
                FurnitureId = randObj.Next(1, furnituresNumber),
                Quantity = randObj.Next(1, 10)
            };
            db.OrderDetails.Add(orderDetail);
        }

        db.SaveChanges();
    }
}