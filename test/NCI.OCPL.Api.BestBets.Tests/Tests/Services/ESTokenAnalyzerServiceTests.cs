#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

using Xunit;

using NCI.OCPL.Api.Common.Testing;
using NCI.OCPL.Api.BestBets.Services;
using System.Threading.Tasks;

namespace NCI.OCPL.Api.BestBets.Tests
{
    public class ESTokenAnalyzerServiceTests : TestServiceBase
    {

        public static IEnumerable<object[]> GetTokenCountData => new[] {
            new object[] {
                "pancoast",
                new object[] {
                    new { token = "pancoast", start_offset = 0, end_offset = 6, type = "<ALPHANUM>", position= 0 },
                },
                1
            },

            new object[] {
                "breast cancer",
                new object[] {
                    new { token = "breast", start_offset = 0, end_offset = 6, type = "<ALPHANUM>", position= 0 },
                    new { token = "cancer", start_offset = 7, end_offset = 13, type = "<ALPHANUM>", position= 1 },
                },
                2
            },
            //TODO: Add crazier tests
        };

        /// <summary>
        /// Verify the GetTokenCount() method knows how to handle responses
        /// from elastic search.
        /// </summary>
        /// <param name="searchTerm">The search term to tokenize.</param>
        /// <param name="responseTokens">The simulated response from elasticsearch.</param>
        /// <param name="expectedCount">The expected token count.</param>
        /// <returns></returns>
        [Theory, MemberData(nameof(GetTokenCountData))]
        public async Task GetTokenCount_Responses(
            string searchTerm,
            object[] responseTokens,
            int expectedCount
        )
        {
            JsonObject resObject = new JsonObject();
            resObject["tokens"] = new JsonArray(responseTokens
                .Select(responseToken => JsonSerializer.SerializeToNode(responseToken))
                .ToArray());

            var settings = TestingElasticsearchClientSettingsFactory.Create(resObject.ToString(), 200);
            ElasticsearchClient client = new ElasticsearchClient(settings);

            IOptions<CGBBIndexOptions> config = GetMockConfig();

            ESTokenAnalyzerService service = new ESTokenAnalyzerService(client, config, new NullLogger<ESTokenAnalyzerService>());
            int actualCount = await service.GetTokenCount("live", searchTerm);

            Assert.Equal(expectedCount, actualCount);

        }

        //TODO: Test failure after repeated attempts.
    }
}
