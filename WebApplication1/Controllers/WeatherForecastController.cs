using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("file/mhash/{multihash:required}/page/{pageOffset}")]
        public IActionResult GetFile(
            [FromRoute] string multihash,
            [SegmentPrefix] string pageOffset,
            [MatrixParameter("{pageOffset}", Name = "content-type")] string contentType)
        {
            return Ok(string.Join("//", multihash, pageOffset, contentType));
        }

        [HttpPut]
        public IActionResult Put([FromForm] FileRequest request)
        {
            _logger.LogInformation("Get file with size {FileSize}", request.File.Length);
            return Ok();
        }
    }
}
