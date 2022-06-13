using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.MatrixParameter.Attributes;

/// <summary>
/// Used to bind the segment prefix value from the route.
/// </summary>
/// <example>
/// If [Route["{fruits}/{location}"] is specified and the incoming uri's relative path is
/// "/apples:color=red,green/washington;rate=good", then in the action's argument list,
/// <c>[SegmentPrefix] string fruits</c> will have fruits = apples
/// but <c>string location</c> without this attribute will have location = washington;rate=good.
/// </example>
public class SegmentPrefixAttribute : FromRouteAttribute
{
}