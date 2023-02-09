using NCI.OCPL.Api.Common.Testing;

namespace NCI.OCPL.Api.BestBets.Tests.ESHealthTestData
{
    /// <summary>
    /// Class used for mocking requests to Elasticsearch that return an error.
    /// </summary>
    /// <seealso cref="NCI.OCPL.Api.BestBets.Tests.Util.ElasticsearchInterceptingConnection" />
    public class ESErrorConnection : ElasticsearchInterceptingConnection
    {
        /// <summary>
        /// Creates a new instance of the ESMatchConnection class
        /// </summary>
        /// <param name="testErrorCode">HTTP status code to return</param>
        public ESErrorConnection(int testErrorCode)
        {
            //Add Handlers
            this.RegisterRequestHandlerForType<Nest.ClusterHealthResponse>((req, res) =>
            {
                // Health check is a GET request (e.g. https://localhost:9299/_cluster/health/bestbets?pretty)
                // but for an error, all we care about is the response code, so we don't load a data file.

                res.StatusCode = testErrorCode;
            });

        }
    }
}

