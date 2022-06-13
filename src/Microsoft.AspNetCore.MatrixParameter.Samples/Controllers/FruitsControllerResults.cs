using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.AspNetCore.MatrixParameter.Samples.Controllers;

public static class FruitsControllerResults
{
    public static Dictionary<string, string> GetFruitsFromLocation(string fruits, string[] color, string location, string[] rate) =>
        new()
        {
            { "fruits", fruits },
            { "color of bananas", Join(color) },
            { "location", location },
            { "rate of " + fruits, Join(rate) }
        };

    public static Dictionary<string, string> GetApplesFromWashington(long customerId, string[] color, string[] rate) =>
        new()
        {
            { "customerId", customerId.ToString(CultureInfo.InvariantCulture) },
            { "color", Join(color) },
            { "rate of apples", Join(rate) }
        };


    public static Dictionary<string, string> GetApplesFromLocation(long customerId, string location, string[] color, string[] rate) =>
        new()
        {
            { "customerId", customerId.ToString(CultureInfo.InvariantCulture) },
            { "location", location },
            { "color", Join(color) },
            { "rate of " + location, Join(rate) }
        };

    public static Dictionary<string, string> GetAttributesFromOptionalSegments(string[] color, string[] rate) =>
        new()
        {
            { "color", Join(color) },
            { "rate of apples", Join(rate) }
        };

    private static string Join(string[] array) => string.Join(",", array);
}