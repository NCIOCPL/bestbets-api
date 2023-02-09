using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Testing;

using Xunit;
using Moq;
using RichardSzalay.MockHttp;

using NCI.OCPL.Api.Common.Testing;
using NCI.OCPL.Api.Common;

using Elasticsearch.Net;
using Nest;

using NCI.OCPL.Api.BestBets.Services;
using NCI.OCPL.Api.BestBets.Tests.ESDisplayTestData;

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
        public async void GetBestBetForDisplay_TestURISetup(BaseDisplayTestData data)
        {
            Uri esURI = null;

            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.GetResponse<BestBetsCategoryDisplay>>((req, res) =>
            {
                //Get the file name for this round
                res.Stream = TestingTools.GetTestFileAsStream("ESDisplayData/" + data.TestFilePath);

                res.StatusCode = 200;

                esURI = req.Uri;
            });

            //While this has a URI, it does not matter, an InMemoryConnection never requests
            //from the server.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

            var connectionSettings = new ConnectionSettings(pool, conn);
            IElasticClient client = new ElasticClient(connectionSettings);

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
        public async void GetBestBetForDisplay_TestAPIConnectionFailure()
        {
            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.GetResponse<BestBetsCategoryDisplay>>((req, res) =>
            {
                res.StatusCode = 500;
            });

            //While this has a URI, it does not matter, an InMemoryConnection never requests
            //from the server.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

            var connectionSettings = new ConnectionSettings(pool, conn);
            IElasticClient client = new ElasticClient(connectionSettings);

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
        public async void GetBestBetForDisplay_TestInvalidResponse()
        {
            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.GetResponse<BestBetsCategoryDisplay>>((req, res) =>
            {

            });

            //While this has a URI, it does not matter, an InMemoryConnection never requests
            //from the server.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

            var connectionSettings = new ConnectionSettings(pool, conn);
            IElasticClient client = new ElasticClient(connectionSettings);

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
        public async void GetBestBetForDisplay_DataLoading(BaseDisplayTestData data)
        {
            IElasticClient client = GetElasticClientWithData(data);

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
        public async void GetBestBetForDisplay_IDNotFoundError(BaseDisplayTestData data)
        {
            IElasticClient client = GetElasticClientWithData(data);

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
        public async void GetBestBetForDisplay_InvalidIDError(BaseDisplayTestData data)
        {
            IElasticClient client = GetElasticClientWithData(data);

            // Setup the mocked Options
            IOptions<CGBBIndexOptions> bbClientOptions = GetMockOptions();

            ESBestBetsDisplayService bbClient = new ESBestBetsDisplayService(client, bbClientOptions, new NullLogger<ESBestBetsDisplayService>());

            APIErrorException ex = await Assert.ThrowsAsync<APIErrorException>(() => bbClient.GetBestBetForDisplay("live", "chicken"));
            Assert.Equal(400, ex.HttpStatusCode);
        }

        private IElasticClient GetElasticClientWithData(BaseDisplayTestData data) {
            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.GetResponse<BestBetsCategoryDisplay>>((req, res) =>
            {
                //Get the file name for this round
                res.Stream = TestingTools.GetTestFileAsStream("ESDisplayData/" + data.TestFilePath);

                res.StatusCode = 200;
            });

            //While this has a URI, it does not matter, an InMemoryConnection never requests
            //from the server.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

            var connectionSettings = new ConnectionSettings(pool, conn);
            IElasticClient client = new ElasticClient(connectionSettings);

            return client;
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