using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1
{
    public class Limits
    {
        public const long RequestFileSizeLimit = 100 * FileSizeUnits.Megabytes;

        public const long MultipartBodySizeLimit = 30 * FileSizeUnits.Megabytes;
    }

    public static class FileSizeUnits
    {
        public const long Bytes = 1L;
        public const long Kilobytes = 1024L;
        public const long Megabytes = 1024L * 1024;
        public const long Gigabytes = 1024L * 1024 * 1024;
        public const long Terabytes = 1024L * 1024 * 1024 * 1024;
    }

    public class FileRequest
    {
        [Required]
        public IFormFile File { get; set; } = null!;
    }

    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }
    }
}
