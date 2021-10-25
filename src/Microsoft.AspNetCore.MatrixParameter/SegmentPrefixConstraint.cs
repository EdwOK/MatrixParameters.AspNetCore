using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.MatrixParameter
{
    public class SegmentPrefixConstraint : IRouteConstraint
    {
        public string Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (values.TryGetValue(routeKey, out var value))
            {
                var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (valueString is null)
                {
                    return "a mojet ti?";
                }

                return valueString.StartsWith($"{routeKey};", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(valueString, routeKey, StringComparison.OrdinalIgnoreCase);
            }

            return "ti pidor jopta";
        }
    }
}