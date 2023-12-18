using FurnitureFactory.Areas.FurnitureFactory.Data;
using FurnitureFactory.Areas.FurnitureFactory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FurnitureFactory.Areas.FurnitureFactory.Services.Cache;

public class OrderDetailCache : CacheService
{
    public OrderDetailCache(IMemoryCache cache, AcmeDataContext context) : base(cache, context)
    {
    }

    public override IEnumerable<OrderDetail> Get()
    {
        Cache.TryGetValue("OrderDetails", out IEnumerable<OrderDetail>? orders);

        return orders ?? Set();
    }

    public override IEnumerable<OrderDetail> Set()
    {
        var orders = Context.OrderDetails.Include(od => od.Furniture).Include(od => od.Order).ToList();
        Cache.Set("OrderDetails", orders,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(SaveTime)));
        return orders;
    }
}