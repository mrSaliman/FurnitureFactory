using FurnitureFactory.Areas.FurnitureFactory.Data;

namespace FurnitureFactory.Areas.FurnitureFactory.Middleware;

public class DbInitializerMiddleware
{
    private readonly RequestDelegate _next;

    public DbInitializerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context, IServiceProvider serviceProvider, AcmeDataContext dbContext)
    {
        if (context.Session.Keys.Contains("starting")) return _next.Invoke(context);
        DbInitializer.Initialize(dbContext);
        context.Session.SetString("starting", "Yes");

        return _next.Invoke(context);
    }
}

public static class DbInitializerExtensions
{
    public static IApplicationBuilder UseDbInitializer(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DbInitializerMiddleware>();
    }
}