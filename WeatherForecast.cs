using Newtonsoft.Json;

namespace weatherforecast_webapi_dotnet6_iac
{
    public class WeatherForecast
    {
        [JsonProperty("id")]
        public string Id { get { return Guid.NewGuid().ToString(); } }

        [JsonProperty("date")]
        public string? Date { get; set; }

        [JsonProperty("temperatureC")]
        public int TemperatureC { get; set; }

        [JsonProperty("summary")]
        public string? Summary { get; set; }
    }
}
