using System.Collections.Generic;
using System.Text.Json.Nodes;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Testing;

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Xunit;

using NCI.OCPL.Api.BestBets.Services;
using NCI.OCPL.Api.Common.Testing;
using System.Threading.Tasks;

namespace NCI.OCPL.Api.BestBets.Tests
{
    public class ESBestBetsMatchServiceTests : TestServiceBase
    {

        public static IEnumerable<object[]> GetMatchesData => new[] {
            // "pancoast" is a simple test as it only has 1 hit, 1 word, and 1 BB category.
            new object[] {
                "pancoast",
                "en",
                "pancoast",
                new string[] { "36012" }
            },
            // "breast cancer" is more complicated, it has 1 hit, 2 words, and the BB category
            // it matches is on page 2.  It also has a ton of negations for breast.
            new object[] {
                "breast cancer",
                "en",
                "breastcancer",
                new string[] { "36408" }
            },
            // "breast cancer treatment" is more complicated, it has 1 hit, 3 words, and no results for last page.
            // It also has a ton of negations for various combinations.
            new object[] {
                "breast cancer treatment",
                "en",
                "breastcancertreatment",
                new string[] { "36408" }
            },
            // "seer stat" is a negated exact match test.  SEER should not be returned
            new object[] {
                "seer stat",
                "en",
                "seerstat",
                new string[] { }
            },
            // "seer stat fact sheet" is a test to make sure the "seer stat" exact match is not hit because
            // we are not exactly matching the phrase "seer stat". Those search terms also match seer.
            new object[] {
                "seer stat fact sheet",
                "en",
                "seerstatfactsheet",
                new string[] { "36681" }
            }
        };


        [Theory, MemberData(nameof(GetMatchesData))]
        public async Task GetMatches_Normal(
            string searchTerm,
            string lang,
            string responseFileBase,
            string[] expectedCategories
        )
        {
            //Use real ES client, with mocked connection.

            ESTokenAnalyzerService tokenService = GetTokenizerService(responseFileBase);
            ESBestBetsMatchService service = GetMatchService(tokenService, responseFileBase);

            string[] actualMatches = await service.GetMatches("live", lang, searchTerm);

            Assert.Equal(expectedCategories, actualMatches);
        }

        private ESTokenAnalyzerService GetTokenizerService(string responseFileBase)
        {
            string body = TestingTools.ReadTestFile($"ESMatchData/{responseFileBase}_analyze.json");
            var settings = TestingElasticsearchClientSettingsFactory.Create(body, 200);
            ElasticsearchClient client = new ElasticsearchClient(settings);

            IOptions<CGBBIndexOptions> config = GetMockConfig();

            return new ESTokenAnalyzerService(client, config, new NullLogger<ESTokenAnalyzerService>());
        }

        private ESBestBetsMatchService GetMatchService(ESTokenAnalyzerService tokenService, string responseFileBase)
        {
            string builder(PostData postData, JsonNode jsonBody) {
                //Determine which round we are performing
                //int numTokens = postObj["params"].matchedtokencount;

                //If this is one item the bool node will be the nested match
                //if it is both exact and matches, then the bool node will
                //be a should. This code is tightly matched to the built query
                //in the implementation
                int numTokens = -1;

                string value;
                var boolNode = jsonBody["query"]["bool"];
                if (boolNode["should"] != null) {
                    value = boolNode["should"][0]
                                    ["bool"]["must"][3]
                                    ["match"]["synonym"]
                                    ["minimum_should_match"].GetValue<string>();
                } else {
                    value = boolNode["must"][3]
                                    ["match"]["synonym"]
                                    ["minimum_should_match"].GetValue<string>();

                }
                numTokens = int.Parse(value);

                return TestingTools.ReadTestFile($"ESMatchData/{responseFileBase}_{numTokens}.json");
            }

            var invoker = new DynamicInMemoryConnection(builder);
            var settings = TestingElasticsearchClientSettingsFactory.Create(invoker);
            ElasticsearchClient client = new ElasticsearchClient(settings);

            IOptions<CGBBIndexOptions> config = GetMockConfig();

            return new ESBestBetsMatchService(client, tokenService, config, new NullLogger<ESBestBetsMatchService>());
        }
    }
}
