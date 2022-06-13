using MatrixParameters.AspNetCore.Samples;
using MatrixParameters.AspNetCore.Samples.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MatrixParameters.AspNetCore.Tests;

public class FruitsControllerTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;

    public FruitsControllerTests(WebApplicationFactory<Startup> factory) => _factory = factory;

    [Theory]
    [InlineData("bananas", new[] { "yellow", "green" }, "oregon", new[] { "good" }, "customers/2/bananas;color=yellow,green;rate=good/oregon")]
    public async Task Get_FruitsFromLocation_ReturnsSuccess(string fruits, string[] color, string location, string[] rate, string url)
    {
        // Arrange
        var client = _factory.CreateClient();
        var expectedResult = FruitsControllerResults.GetFruitsFromLocation(fruits, color, location, rate);

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();

        var actualResult = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(actualResult);
        Assert.Equal(expectedResult, actualResult);
    }
}