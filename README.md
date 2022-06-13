## Microsoft.AspNetCore.MatrixParameter

This library helps you to start using [matrix parameters (or matrix URIs)](http://www.w3.org/DesignIssues/MatrixURIs.html) in the route of API action.

## Code Example

### Startup
```csharp
services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(0, new SegmentPrefixAttributeModelBinderProvider());
    options.ModelBinderProviders.Insert(1, new MatrixParameterAttributeModelBinderProvider());
});

services.AddRouting(options =>
{
    options.ConstraintMap.Add("SegmentPrefix", typeof(SegmentPrefixConstraint));
});
```

### Swagger
```csharp
services.AddSwaggerGen(c =>
{
    c.ParameterFilter<MatrixParameterFilter>();
    c.DocumentFilter<MatrixDocumentFilter>();
});
```

### Controller
```csharp
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
    var result = new Dictionary<string, string>
    {
        { "fruits", fruits },
        { "color of bananas", Join(color) },
        { "location", location },
        { "rate of " + fruits, Join(rate) }
    };
    return Ok(result);
}
```
