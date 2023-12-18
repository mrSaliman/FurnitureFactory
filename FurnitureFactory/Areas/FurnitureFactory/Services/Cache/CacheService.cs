using FurnitureFactory.Areas.FurnitureFactory.Data;
using Microsoft.Extensions.Caching.Memory;

namespace FurnitureFactory.Areas.FurnitureFactory.Services.Cache;

public abstract class CacheService
{
    protected readonly IMemoryCache Cache;
    protected readonly AcmeDataContext Context;
    protected readonly List<CacheService> Children;
    protected const int SaveTime = 2 * 12 * 240;

    protected CacheService(IMemoryCache cache, AcmeDataContext context)
    {
        Cache = cache;
        Context = context;
        Children = new List<CacheService>();
    }

    public abstract object Get();
    public abstract object Set();

    public void Update()
    {
        Set();
        foreach (var cacheService in Children) cacheService.Update();
    }
}