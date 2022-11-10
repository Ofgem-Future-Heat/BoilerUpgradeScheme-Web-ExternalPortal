using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace Ofgem.Web.BUS.ExternalPortal.Core.Extensions;

/// <summary>
/// Extensions for the MVC TempData dictionary 
/// </summary>
public static class TempDataExtensions
{
    /// <summary>
    /// Serialises and adds a complex object to TempData
    /// </summary>
    /// <typeparam name="T">The object type</typeparam>
    /// <param name="key">Key for the added object</param>
    /// <param name="value">Object to add</param>
    public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
    {
        tempData[key] = JsonConvert.SerializeObject(value);
    }

    /// <summary>
    /// Gets and deserialises a complex object from TempData
    /// </summary>
    /// <typeparam name="T">The object type</typeparam>
    /// <param name="key">Key (location in TempData) of the required object</param>
    /// <returns>The deserialised object</returns>
    public static T? Get<T>(this ITempDataDictionary tempData, string key) where T : class
    {
        tempData.TryGetValue(key, out var obj);
        return obj == null ? null : JsonConvert.DeserializeObject<T>((string)obj);
    }
}
