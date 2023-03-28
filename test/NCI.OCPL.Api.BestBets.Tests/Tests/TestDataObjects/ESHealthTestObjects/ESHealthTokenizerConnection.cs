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
    /// This class is a helper around the ElasticsearchInterceptingConntection to handle
    /// analyzer responses for a BB HealthCheck Request.  (Basically, this class only exists
    /// because we need to have a TokenizerConnection in order to instantiate Match Service.)
    /// </summary>
    /// <seealso cref="NCI.OCPL.Api.BestBets.Tests.Util.ElasticsearchInterceptingConnection" />
    public class ESHealthTokenizerConnection : ElasticsearchInterceptingConnection
    {
        /// <summary>
        /// Gets the prefix of a testdata file for this test.
        /// </summary>
        /// <returns></returns>
        private string TestFilePrefix { get; set; }

        /// <summary>
        /// Creates a new instance of the ESMatchTokenizerConnection class
        /// </summary>
        /// <param name="testFilePrefix">The prefix of the test files</param>
        public ESHealthTokenizerConnection(string testFilePrefix)
        {
            this.TestFilePrefix = testFilePrefix;

            //Add Handlers
            this.RegisterRequestHandlerForType<Nest.ClusterHealthResponse>((req, res) =>
            {
                //I don't care about the request for this... for now.

                //Get the file name for this round
                res.Stream = TestingTools.GetTestFileAsStream(GetTestFileName());

                res.StatusCode = 200;
            });
        }

        private string GetTestFileName()
        {
            return string.Format("ESHealthData/{0}_analyze.json", TestFilePrefix);
        }

    }
}
