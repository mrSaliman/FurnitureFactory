using FurnitureFactory.Areas.FurnitureFactory.Data;
using FurnitureFactory.Areas.FurnitureFactory.Models;
using Microsoft.Extensions.Caching.Memory;

namespace FurnitureFactory.Areas.FurnitureFactory.Services.Cache;

public class CustomerCache : CacheService
{
    public CustomerCache(IMemoryCache cache, AcmeDataContext context, OrderCache orderCache) : base(cache, context)
    {
        Children.Add(orderCache);
    }

    public override IEnumerable<Customer> Get()
    {
        Cache.TryGetValue("Customers", out IEnumerable<Customer>? customers);

        return customers ?? Set();
    }

    public override IEnumerable<Customer> Set()
    {
        var customers = Context.Customers
            .ToList();
        Cache.Set("Customers", customers,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(SaveTime)));
        return customers;
    }
}