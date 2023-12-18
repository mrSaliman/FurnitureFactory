using FurnitureFactory.Areas.FurnitureFactory.Data;
using FurnitureFactory.Areas.FurnitureFactory.Models;
using Microsoft.Extensions.Caching.Memory;

namespace FurnitureFactory.Areas.FurnitureFactory.Services.Cache;

public class FurnitureCache : CacheService
{
    public FurnitureCache(IMemoryCache cache, AcmeDataContext context, OrderDetailCache orderDetailCache) : base(cache,
        context)
    {
        Children.Add(orderDetailCache);
    }

    public override IEnumerable<Furniture> Get()
    {
        Cache.TryGetValue("Furnitures", out IEnumerable<Furniture>? furnitures);

        return furnitures ?? Set();
    }

    public override IEnumerable<Furniture> Set()
    {
        var furnitures = Context.Furnitures
            .ToList();
        Cache.Set("Furnitures", furnitures,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(SaveTime)));
        return furnitures;
    }
}