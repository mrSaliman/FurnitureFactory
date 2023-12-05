using System.Data.Entity;
using FurnitureFactory.Areas.FurnitureFactory.Data;
using FurnitureFactory.Areas.FurnitureFactory.Models;
using Microsoft.Extensions.Caching.Memory;

namespace FurnitureFactory.Areas.FurnitureFactory.Services;

public class FurnitureFactoryCacheService
{
    private readonly IMemoryCache _cache;
    private readonly AcmeDataContext _context;

    public FurnitureFactoryCacheService(AcmeDataContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public IEnumerable<Order> GetOrders()
    {
        _cache.TryGetValue("Orders", out IEnumerable<Order>? orders);

        return orders ?? SetOrders();
    }

    public IEnumerable<Order> SetOrders()
    {
        var orders = _context.Orders.Include(o => o.ResponsibleEmployee).Include(o => o.Customer);
        var result = orders.ToList();
        _cache.Set("Orders", result,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(100000)));
        return result;
    }

    public IEnumerable<Furniture> GetFurnitures()
    {
        _cache.TryGetValue("Furnitures", out IEnumerable<Furniture>? furnitures);

        return furnitures ?? SetFurnitures();
    }

    public IEnumerable<Furniture> SetFurnitures()
    {
        var furnitures = _context.Furnitures
            .ToList();
        _cache.Set("Furnitures", furnitures,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(100000)));
        return furnitures;
    }

    public IEnumerable<Customer> GetCustomers()
    {
        _cache.TryGetValue("Customers", out IEnumerable<Customer>? customers);

        return customers ?? SetCustomers();
    }

    public IEnumerable<Customer> SetCustomers()
    {
        var customers = _context.Customers
            .ToList();
        _cache.Set("Customers", customers,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(100000)));
        return customers;
    }

    public IEnumerable<Employee> GetEmployees()
    {
        _cache.TryGetValue("Employees", out IEnumerable<Employee>? employees);

        return employees ?? SetEmployees();
    }

    public IEnumerable<Employee> SetEmployees()
    {
        var employees = _context.Employees
            .ToList();
        _cache.Set("Employees", employees,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(100000)));
        return employees;
    }
    
    public IEnumerable<OrderDetail> GetOrderDetails()
    {
        _cache.TryGetValue("OrderDetails", out IEnumerable<OrderDetail>? orders);

        return orders ?? SetOrderDetails();
    }

    public IEnumerable<OrderDetail> SetOrderDetails()
    {
        var orders = _context.OrderDetails.Include(od => od.Furniture).Include(od => od.Order).ToList();
        _cache.Set("OrderDetails", orders,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(100000)));
        return orders;
    }
}