using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace weatherforecast_webapi_dotnet6_iac.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private const string DatabaseId = "dev2arch-test-mongodb";
        private const string ContainerId = "WeatherForecast";

        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;
        private readonly Container _container;

        public WeatherForecastController()
        {
            var keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
            var EndpointUrl = $"https://{Environment.GetEnvironmentVariable("COSMOS_NAME")}.documents.azure.com:443/";

            string secretName = "cosmos-primary-key";
            var kvUri = $"https://{keyVaultName}.vault.azure.net";
            var kvClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            var secret = kvClient.GetSecret(secretName);

            _cosmosClient = new CosmosClient(EndpointUrl, secret.Value.Value);
            _database = _cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId).GetAwaiter().GetResult();
            _container = _database.CreateContainerIfNotExistsAsync(ContainerId, "/date").GetAwaiter().GetResult();
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> GetAsync(string date)
        {
            var weatherForecasts = new List<WeatherForecast>();

            var query = new QueryDefinition("SELECT * FROM WeatherForecast p WHERE p.date = @date")
                .WithParameter("@date", date);

            var feedIterator = _container.GetItemQueryIterator<WeatherForecast>(query);

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync();
                weatherForecasts.AddRange(response);
            }

            return weatherForecasts;
        }

        [HttpPost(Name = "SetWeatherForecast")]
        public async Task<ActionResult<WeatherForecast>> PostAsync(WeatherForecast item)
        {
            var response = await _container.UpsertItemAsync(item, new PartitionKey(item.Date));
            return Ok(response.Resource);
        }
    }
}
