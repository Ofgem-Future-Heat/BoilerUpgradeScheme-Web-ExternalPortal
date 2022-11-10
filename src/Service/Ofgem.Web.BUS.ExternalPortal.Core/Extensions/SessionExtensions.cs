using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Extensions;

public static class SessionExtensions
{
    /// <summary>
    /// Serialises a complex object and stores it in session.
    /// </summary>
    /// <typeparam name="T">The type of object to store.</typeparam>
    /// <param name="session">The session.</param>
    /// <param name="key">The object's key.</param>
    /// <param name="value">The value to store.</param>
    public static void Put<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }

    /// <summary>
    /// Gets a value from session and deserialises to a complex object.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="session">The session.</param>
    /// <param name="key">The item's key.</param>
    /// <returns>The deserialised object or null if the object cannot be found.</returns>
    public static T? GetOrDefault<T>(this ISession session, string key)
    {
        if (session.Keys.Any(k => k == key))
        {
            var stringValue = session.GetString(key);
            return string.IsNullOrEmpty(stringValue) ? default(T) : JsonConvert.DeserializeObject<T>(stringValue);
        }

        return default(T);
    }
}
