using FurnitureFactory.Areas.FurnitureFactory.Data;
using FurnitureFactory.Areas.FurnitureFactory.Models;
using Microsoft.Extensions.Caching.Memory;

namespace FurnitureFactory.Areas.FurnitureFactory.Services;

public class EmployeeCache : CacheService
{
    public EmployeeCache(IMemoryCache cache, AcmeDataContext context, OrderCache orderCache) : base(cache, context)
    {
        Children.Add(orderCache);
    }

    public override IEnumerable<Employee> Get()
    {
        Cache.TryGetValue("Employees", out IEnumerable<Employee>? employees);

        return employees ?? Set();
    }

    public override IEnumerable<Employee> Set()
    {
        var employees = Context.Employees
            .ToList();
        Cache.Set("Employees", employees,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(SaveTime)));
        return employees;
    }
}