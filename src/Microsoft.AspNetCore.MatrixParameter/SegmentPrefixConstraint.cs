using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.MatrixParameter;

public class SegmentPrefixConstraint : IRouteConstraint
{
    private const string KeyDelimiter = ";";

    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values,
        RouteDirection routeDirection)
    {
        if (values.TryGetValue(routeKey, out var value))
        {
            var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (valueString is null)
            {
                return false;
            }

            return valueString.StartsWith($"{routeKey}{KeyDelimiter}", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(valueString, routeKey, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}