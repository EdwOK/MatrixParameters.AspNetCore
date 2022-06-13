using MatrixParameters.AspNetCore.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace MatrixParameters.AspNetCore.Samples.Controllers;

[ApiController]
[Route("customers/{customerId}")]
public class FruitsController : ControllerBase
{
    // GET customers/2/bananas;color=yellow,green;rate=good/oregon
    //
    // This route with the segments {fruits} and {location} will match a path with two segments if they are not 
    // matched with the following two actions GetApplesFromWashington and GetApplesFromLocation. Both of their 
    // routes are more specific because of constraints, and thus matched prior to this.
    [HttpGet]
    [Route("{fruits}/{location}")]
    public IActionResult GetFruitsFromLocation(
        [SegmentPrefix] string fruits, // The fruits from the route segment {fruits}.
        [MatrixParameter("bananas")] string[] color, // The matrix parameter color from the segment starting with "bananas". It is matched only if the fruits is "apples".
        [SegmentPrefix] string location, // The location from the route segment {location}.
        [MatrixParameter("{fruits}")] string[] rate) // The matrix parameter rate from the route segment "{fruits}".
    {
        return Ok(FruitsControllerResults.GetFruitsFromLocation(fruits, color, location, rate));
    }

    // GET customers/1/apples;rate=excellent;color=red/washington
    //
    // This route with segment prefixes "apples" and then "washington" (both defined by SegmentPrefixConstraint 
    // will match a path with two segments in which the first starts with "apples" and the second starts with 
    // "washington", like "/apples;.../washington;...".
    [HttpGet]
    [Route("{apples:SegmentPrefix}/{washington:SegmentPrefix}")]
    public IActionResult GetApplesFromWashington(
        long customerId, // The customer's id from the route segment {customerId} of the URI.
        [MatrixParameter] string[] color, // The matrix parameter color from any path segment of the URI.
        [MatrixParameter("apples")] string[] rate) //The matrix parameter rate from the path segment starting with "apples" of the URI.
    {
        return Ok(FruitsControllerResults.GetApplesFromWashington(customerId, color, rate));
    }

    // GET customers/2/apples;color=red;rate=good;color=green;/connecticut;rate=excellent
    //
    // This route with a segment prefix "apples" (defined by SegmentPrefixConstraint) and then the route
    // segment {location} will match a path with two segments, in which the first starts with "apples".
    [HttpGet]
    [Route("{apples:SegmentPrefix}/{location}")]
    public IActionResult GetApplesFromLocation(
        long customerId, // The customer's id from the route segment {customerId}.
        [SegmentPrefix] string location, // The segment prefix location from the route segment {location}.
        [MatrixParameter] string[] color, // The matrix parameter color from any path segment of the URI.
        [MatrixParameter("{location}")] string[] rate) //The matrix parameter rate from the route segment {location}.
    {
        return Ok(FruitsControllerResults.GetApplesFromLocation(customerId, location, color, rate));
    }

    // GET customers/2/optional/foo/apples;rate=good/california;/bar/color=green,red;rate=excellent,good
    //
    // This route with the catch-all constraint {*OptionalSubPath} will match all segments following "optional/".
    [HttpGet]
    [Route("optional/{*OptionalSubPath}")]
    public IActionResult GetAttributesFromOptionalSegments(
        [MatrixParameter] string[] color, // The color from any path segment of the URI.
        [MatrixParameter("apples")] string[] rate) // The rate from a path segment starting with the prefix "apples".
    {
        return Ok(FruitsControllerResults.GetAttributesFromOptionalSegments(color, rate));
    }
}