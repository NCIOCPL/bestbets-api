using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

using Elastic.Clients.Elasticsearch;
using Xunit;

using NCI.OCPL.Api.Common.Testing;
using NCI.OCPL.Api.BestBets.Services;
using System.Threading.Tasks;

namespace NCI.OCPL.Api.BestBets.Tests
{
    public class ESBestBetsHealthServiceTests : TestServiceBase
    {
        [Theory]
        [InlineData("green")]
        [InlineData("yellow")]
        public async Task HealthStatus_Healthy(string datafile)
        {
            ESBestBetsHealthService service = GetHealthServiceFromFile($"ESHealthData/{datafile}.json", 200);

            bool isHealthy = await service.IsHealthy();

            Assert.True(isHealthy);
        }

        [Theory]
        [InlineData("red")]
        [InlineData("unexpected")]   // i.e. "Unexpected color"
        public async Task HealthStatus_Unhealthy(string datafile)
        {
            ESBestBetsHealthService service = GetHealthServiceFromFile($"ESHealthData/{datafile}.json", 200);

            bool isHealthy = await service.IsHealthy();

            Assert.False(isHealthy);
        }

        /// <summary>
        /// Test for when the ES healthcheck returns a non-200 response code
        /// (response.IsValid comes back as false).
        /// </summary>
        /// <param name="httpStatus"></param>
        [Theory]
        [InlineData(404)]
        [InlineData(500)]
        public async Task HealthStatus_InvalidResponse(int httpStatus)
        {
            ESBestBetsHealthService service = GetHealthService("{}", httpStatus);

            bool res = await service.IsHealthy();
            Assert.False(res);
        }

        private ESBestBetsHealthService GetHealthServiceFromFile(string filename, int statusCode)
        {
            string responseBody = TestingTools.ReadTestFile(filename);
            return GetHealthService(responseBody, statusCode);
        }

        private ESBestBetsHealthService GetHealthService(string responseBody, int statusCode)
        {
            var settings = TestingElasticsearchClientSettingsFactory.Create(responseBody, statusCode);
            ElasticsearchClient client = new ElasticsearchClient(settings);

            IOptions<CGBBIndexOptions> config = GetMockConfig();

            return new ESBestBetsHealthService(client, config, new NullLogger<ESBestBetsHealthService>());
        }
    }
}