using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;

using Elastic.Clients.Elasticsearch;
using Moq;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.Common.Testing;


using NCI.OCPL.Api.BestBets.Services;
using NCI.OCPL.Api.BestBets.Tests.ESDisplayTestData;
using System.Threading.Tasks;

namespace NCI.OCPL.Api.BestBets.Tests.ESBestBetsDisplayServiceTests
{
    public class GetBestBetForDisplayTests
    {

        public static IEnumerable<object[]> JsonData => new[] {
            new object[] { new PancoastTumorDisplayTestData() },
            new object[] { new FotosDeCancerDisplayTestData() }
        };

        public static IEnumerable<object[]> NotFoundData => new[] {
            new object[] { new NotFoundDisplayTestData() }
        };

        /// <summary>
        /// Test that URI for Elasticsearch is set up correctly.
        /// </summary>
        [Theory, MemberData(nameof(JsonData))]
        public async Task GetBestBetForDisplay_TestURISetup(BaseDisplayTestData data)
        {
            Uri esURI = null;
            string responseBody = TestingTools.ReadTestFile("ESDisplayData/" + data.TestFilePath);
            var settings = TestingElasticsearchClientSettingsFactory.Create(
                responseBody,
                200,
                details => esURI = details.Uri
            );
            ElasticsearchClient client = new ElasticsearchClient(settings);

            // Setup the mocked Options
            IOptions<CGBBIndexOptions> bbClientOptions = GetMockOptions();

            ESBestBetsDisplayService bbClient = new ESBestBetsDisplayService(client, bbClientOptions, new NullLogger<ESBestBetsDisplayService>());

            // We don't actually care that this returns anything - only that the intercepting connection
            // sets up the request URI correctly.
            IBestBetDisplay actDisplay = await bbClient.GetBestBetForDisplay("preview", "431121");

            Assert.Equal(
                new string[] { "/", "bestbets_preview_v1/", "_doc/", "431121" },
                esURI.Segments,
                new ArrayComparer());

            actDisplay = await bbClient.GetBestBetForDisplay("live", "431121");

            Assert.Equal(
                new string[] { "/", "bestbets_live_v1/", "_doc/", "431121" },
                esURI.Segments,
                new ArrayComparer());
        }

        /// <summary>
        /// Test failure to connect to and retrieve response from API.
        /// </summary>
        [Fact()]
        public async Task GetBestBetForDisplay_TestAPIConnectionFailure()
        {
            var settings = TestingElasticsearchClientSettingsFactory.Create("{}", 500);
            ElasticsearchClient client = new ElasticsearchClient(settings);

            // Setup the mocked Options
            IOptions<CGBBIndexOptions> bbClientOptions = GetMockOptions();

            ESBestBetsDisplayService bbClient = new ESBestBetsDisplayService(client, bbClientOptions, new NullLogger<ESBestBetsDisplayService>());

            APIErrorException ex = await Assert.ThrowsAsync<APIErrorException>(() => bbClient.GetBestBetForDisplay("live", "431121"));
            Assert.Equal(500, ex.HttpStatusCode);
        }

        /// <summary>
        /// Test invalid response from API.
        /// </summary>
        [Fact()]
        public async Task GetBestBetForDisplay_TestInvalidResponse()
        {
            var settings = TestingElasticsearchClientSettingsFactory.Create("not-json", 200);
            ElasticsearchClient client = new ElasticsearchClient(settings);

            // Setup the mocked Options
            IOptions<CGBBIndexOptions> bbClientOptions = GetMockOptions();

            ESBestBetsDisplayService bbClient = new ESBestBetsDisplayService(client, bbClientOptions, new NullLogger<ESBestBetsDisplayService>());

            APIErrorException ex = await Assert.ThrowsAsync<APIErrorException>(() => bbClient.GetBestBetForDisplay("live", "431121"));
            Assert.Equal(500, ex.HttpStatusCode);
        }

        /// <summary>
        /// Tests the correct loading of various data files.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Theory, MemberData(nameof(JsonData))]
        public async Task GetBestBetForDisplay_DataLoading(BaseDisplayTestData data)
        {
            ElasticsearchClient client = GetElasticClientWithData(data);

            // Setup the mocked Options
            IOptions<CGBBIndexOptions> bbClientOptions = GetMockOptions();

            ESBestBetsDisplayService bbClient = new ESBestBetsDisplayService(client, bbClientOptions, new NullLogger<ESBestBetsDisplayService>());

            IBestBetDisplay actDisplay = await bbClient.GetBestBetForDisplay("live", data.ExpectedData.ID);

            Assert.Equal(data.ExpectedData, actDisplay, new IBestBetDisplayComparer());
        }

        /// <summary>
        /// Test for handling API cannot find ID.
        /// </summary>
        [Theory, MemberData(nameof(NotFoundData))]
        public async Task GetBestBetForDisplay_IDNotFoundError(BaseDisplayTestData data)
        {
            // This test needs the mock ES instance to return a 404 status, and therefore can't use the
            // same GetElasticClientWithData method as the other tests.
            string responseBody = TestingTools.ReadTestFile("ESDisplayData/" + data.TestFilePath);
            var settings = TestingElasticsearchClientSettingsFactory.Create(responseBody, 404);
            ElasticsearchClient client = new ElasticsearchClient(settings);

            // Setup the mocked Options
            IOptions<CGBBIndexOptions> bbClientOptions = GetMockOptions();

            ESBestBetsDisplayService bbClient = new ESBestBetsDisplayService(client, bbClientOptions, new NullLogger<ESBestBetsDisplayService>());

            APIErrorException ex = await Assert.ThrowsAsync<APIErrorException>(() => bbClient.GetBestBetForDisplay("live", "12345"));
            Assert.Equal(404, ex.HttpStatusCode);
        }

        /// <summary>
        /// Test for handling invalid ID.
        /// </summary>
        [Theory, MemberData(nameof(JsonData))]
        public async Task GetBestBetForDisplay_InvalidIDError(BaseDisplayTestData data)
        {
            ElasticsearchClient client = GetElasticClientWithData(data);

            // Setup the mocked Options
            IOptions<CGBBIndexOptions> bbClientOptions = GetMockOptions();

            ESBestBetsDisplayService bbClient = new ESBestBetsDisplayService(client, bbClientOptions, new NullLogger<ESBestBetsDisplayService>());

            APIErrorException ex = await Assert.ThrowsAsync<APIErrorException>(() => bbClient.GetBestBetForDisplay("live", "chicken"));
            Assert.Equal(400, ex.HttpStatusCode);
        }

        private ElasticsearchClient GetElasticClientWithData(BaseDisplayTestData data) {
            string responseBody = TestingTools.ReadTestFile("ESDisplayData/" + data.TestFilePath);
            var settings = TestingElasticsearchClientSettingsFactory.Create(responseBody, 200);
            return new ElasticsearchClient(settings);
        }

        private IOptions<CGBBIndexOptions> GetMockOptions()
        {
            Mock<IOptions<CGBBIndexOptions>> bbClientOptions = new Mock<IOptions<CGBBIndexOptions>>();
            bbClientOptions
                .SetupGet(opt => opt.Value)
                .Returns(new CGBBIndexOptions()
                {
                    PreviewAliasName = "bestbets_preview_v1",
                    LiveAliasName = "bestbets_live_v1"
                }
            );

            return bbClientOptions.Object;
        }
    }

}