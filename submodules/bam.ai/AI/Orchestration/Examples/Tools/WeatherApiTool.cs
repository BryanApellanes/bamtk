using Bam.AI.Orchestration.Tools;
using Microsoft.Extensions.Logging;


namespace Bam.AI.Orchestration.Examples
{
    /// <summary>
    /// Example tool implementation for weather API queries.
    /// </summary>
    /// <remarks>
    /// This is a mock implementation for demonstration purposes.
    /// In production, this would call a real weather API like:
    /// - OpenWeatherMap
    /// - Weather.gov
    /// - WeatherAPI
    /// - AccuWeather
    /// </remarks>
    public class WeatherApiTool : IToolImplementation
    {
        private readonly ILogger<WeatherApiTool> _logger;


        // Mock weather data
        private static readonly Dictionary<string, WeatherData> MockWeatherData = new()
        {
            ["seattle"] = new WeatherData { Temperature = 55, Conditions = "Rainy", Humidity = 85 },
            ["san francisco"] = new WeatherData { Temperature = 65, Conditions = "Foggy", Humidity = 75 },
            ["new york"] = new WeatherData { Temperature = 72, Conditions = "Partly Cloudy", Humidity = 60 },
            ["miami"] = new WeatherData { Temperature = 85, Conditions = "Sunny", Humidity = 70 },
            ["chicago"] = new WeatherData { Temperature = 50, Conditions = "Windy", Humidity = 55 },
        };


        public WeatherApiTool(ILogger<WeatherApiTool> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// Executes the weather API query.
        /// </summary>
        public async Task<object> ExecuteAsync(
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing weather API tool");


            // Extract parameters
            if (!parameters.TryGetValue("location", out var locationObj))
            {
                throw new ToolExecutionException(
                    "weather_api",
                    "Missing required parameter: location");
            }


            var location = locationObj.ToString();
            var unit = parameters.TryGetValue("unit", out var unitObj)
                ? unitObj.ToString()
                : "fahrenheit";


            _logger.LogDebug("Looking up weather for {Location} in {Unit}", location, unit);


            // Simulate API delay
            await Task.Delay(500, cancellationToken);


            // Find weather data (case-insensitive, partial match)
            var weatherKey = MockWeatherData.Keys.FirstOrDefault(k =>
                location.Contains(k, StringComparison.OrdinalIgnoreCase));


            if (weatherKey == null)
            {
                throw new ToolExecutionException(
                    "weather_api",
                    $"Weather data not available for location: {location}");
            }


            var weather = MockWeatherData[weatherKey];


            // Convert temperature if needed
            var temperature = unit.ToLowerInvariant() == "celsius"
                ? (weather.Temperature - 32) * 5 / 9
                : weather.Temperature;


            var result = new
            {
                location = location,
                temperature = Math.Round(temperature, 1),
                unit = unit,
                conditions = weather.Conditions,
                humidity = weather.Humidity,
                timestamp = DateTime.UtcNow
            };


            _logger.LogInformation(
                "Weather lookup successful: {Location} - {Temp}°{Unit}, {Conditions}",
                location,
                result.temperature,
                unit,
                weather.Conditions);


            return result;
        }


        private class WeatherData
        {
            public double Temperature { get; set; }
            public string Conditions { get; set; }
            public int Humidity { get; set; }
        }
    }
}
