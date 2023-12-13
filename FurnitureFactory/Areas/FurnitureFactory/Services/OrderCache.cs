using FurnitureFactory.Areas.FurnitureFactory.Data;
using FurnitureFactory.Areas.FurnitureFactory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FurnitureFactory.Areas.FurnitureFactory.Services;

public class OrderCache : CacheService
{
    public OrderCache(IMemoryCache cache, AcmeDataContext context, OrderDetailCache orderDetailCache) : base(cache, context)
    {
        Children.Add(orderDetailCache);
    }

    public override IEnumerable<Order> Get()
    {
        Cache.TryGetValue("Orders", out IEnumerable<Order>? orders);

        return orders ?? Set();
    }

    public override IEnumerable<Order> Set()
    {
        var orders = Context.Orders.Include(o => o.ResponsibleEmployee).Include(o => o.Customer);
        var result = orders.ToList();
        Cache.Set("Orders", result,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(SaveTime)));
        return result;
    }
}