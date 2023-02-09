using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Elasticsearch.Net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


using NCI.OCPL.Api.Common.Testing;
using NCI.OCPL.Api.BestBets.Tests.Util;

namespace NCI.OCPL.Api.BestBets.Tests.ESMatchTestData
{
    /// <summary>
    /// Class used for mocking BestBet Match requests to Elasticsearch.  This should be
    /// used as the base class of test specific Connections object passed into an ElasticClient.
    /// </summary>
    /// <seealso cref="NCI.OCPL.Api.BestBets.Tests.Util.ElasticsearchInterceptingConnection" />
    public class ESMatchConnection : ElasticsearchInterceptingConnection
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
        public ESMatchConnection(string testFilePrefix)
        {
            this.TestFilePrefix = testFilePrefix;

            //Add Handlers
            this.RegisterRequestHandlerForType<Nest.SearchResponse<BestBetsMatch>>((req, res) =>
            {
                //Get the request parameters
                dynamic postObj = this.GetRequestPost(req);

                //Determine which round we are performing
                //int numTokens = postObj["params"].matchedtokencount;

                //If this is one item the bool node will be the nested match
                //if it is both exact and matches, then the bool node will
                //be a should. This code is tightly matched to the built query
                //in the implementation
                int numTokens = -1;

                var boolNode = postObj["query"]["bool"];
                if (boolNode["should"] != null) {
                    numTokens = boolNode["should"][0]
                                        ["bool"]["must"][3]
                                        ["match"]["synonym"]
                                        ["minimum_should_match"].ToObject<int>();
                } else {
                    numTokens = boolNode["must"][3]
                                        ["match"]["synonym"]
                                        ["minimum_should_match"].ToObject<int>();
                }



                //Get the file name for this round
                res.Stream = TestingTools.GetTestFileAsStream(GetTestFileName(numTokens));

                res.StatusCode = 200;
            });

        }

        private string GetTestFileName(int numTerms)
        {
            return string.Format("ESMatchData/{0}_{1}.json", TestFilePrefix, numTerms);
        }
    }
}