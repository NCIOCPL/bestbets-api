using System;
using System.IO;
using System.Threading.Tasks;

using Elasticsearch.Net;

using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NCI.OCPL.Api.Common.Testing;

using NCI.OCPL.Api.BestBets.Tests.Util;

namespace NCI.OCPL.Api.BestBets.Tests.ESHealthTestData
{
    /// <summary>
    /// Class used for mocking BestBet HealthCheck requests to Elasticsearch.  This should be
    /// used as the base class of test specific Connections object passed into an ElasticClient.
    /// </summary>
    /// <seealso cref="NCI.OCPL.Api.BestBets.Tests.Util.ElasticsearchInterceptingConnection" />
    public class ESHealthConnection : ElasticsearchInterceptingConnection
    {
        /// <summary>
        /// Gets the prefix of a testdata file for this test.
        /// </summary>
        /// <returns></returns>
        private string TestFilePrefix { get; set; }

        /// <summary>
        /// Creates a new instance of the ESMatchConnection class
        /// </summary>
        /// <param name="testFilePrefix">The prefix of the test files</param>
        public ESHealthConnection(string testFilePrefix)
        {
            this.TestFilePrefix = testFilePrefix;

            //Add Handlers
            this.RegisterRequestHandlerForType<Nest.ClusterHealthResponse>((req, res) =>
            {
                // Health check is a GET request (e.g. https://localhost:9299/_cluster/health/bestbets?pretty)
                // so we don't need to do anything special, just load the data file.
                //Get the file name for this round
                res.Stream = TestingTools.GetTestFileAsStream(GetTestFileName());

                res.StatusCode = 200;
            });

        }

        private string GetTestFileName()
        {
            return string.Format("ESHealthData/{0}.json", TestFilePrefix);
        }
    }
}

