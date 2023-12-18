using Newtonsoft.Json;

namespace FurnitureFactory.Areas.FurnitureFactory.Filters;

public static class SessionExtensions
{
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }

    public static T? Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonConvert.DeserializeObject<T>(value);
    }

    public static void Set(this ISession session, string key, Dictionary<string, string> dictionary)
    {
        session.SetString(key, JsonConvert.SerializeObject(dictionary));
    }

    public static Dictionary<string, string>? GetDict(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
    }
}