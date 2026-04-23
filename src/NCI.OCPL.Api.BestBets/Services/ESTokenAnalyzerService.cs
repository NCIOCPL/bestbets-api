using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;

using NCI.OCPL.Api.Common;


namespace NCI.OCPL.Api.BestBets.Services
{
    /// <summary>
    /// Concrete implementation of an Elasticsearch backed ITokenAnalyzerService
    /// </summary>
    /// <seealso cref="NCI.OCPL.Api.BestBets.ITokenAnalyzerService" />
    public class ESTokenAnalyzerService : ITokenAnalyzerService
    {
        private ElasticsearchClient _elasticClient;
        private CGBBIndexOptions _bestbetsConfig;
        private readonly ILogger<ESTokenAnalyzerService> _logger;

        /// <summary>
        /// Creates a new instance of a ESBestBetsMatchService
        /// </summary>
        public ESTokenAnalyzerService(ElasticsearchClient client,
                        IOptions<CGBBIndexOptions> bestbetsConfig,
                        ILogger<ESTokenAnalyzerService> logger) //Needs someway to get an IElasticClient
        {
            _elasticClient = client;
            _bestbetsConfig = bestbetsConfig.Value;
            _logger = logger;
        }

        /// <summary>
        /// Gets a count of the number of tokens as tokenized by elasticsearch
        /// </summary>
        /// <param name="collection">The search index to use</param>
        /// <param name="term">The term to get token count</param>
        /// <returns>The number of tokens in the term</returns>
        public async Task<int> GetTokenCount(string collection, string term)
        {
            string[] ALLOWED_TOKEN_TYPES = { "<ALPHANUM>", "<NUM>"};

            AnalyzeIndexResponse analyzeResponse;
            string indexForAnalysis = (collection == "preview") ?
                                        _bestbetsConfig.PreviewAliasName :
                                        _bestbetsConfig.LiveAliasName;

            try
            {
                analyzeResponse = await this._elasticClient.Indices.AnalyzeAsync(
                    new AnalyzeIndexRequest(indexForAnalysis)
                    {
                        Analyzer = "nostem",
                        Text = new[] { term }
                    }
                );
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error analyzing token count for term '{0}'.", term.Replace(Environment.NewLine, String.Empty));
                _logger.LogInformation("Trying again for term '{0}'", term.Replace(Environment.NewLine, String.Empty));

                // Try again. (this is really just for when we run out of sockets)
                analyzeResponse = await this._elasticClient.Indices.AnalyzeAsync(
                    new AnalyzeIndexRequest(indexForAnalysis)
                    {
                        Analyzer = "nostem",
                        Text = new[] { term }
                    }
                );
            }

            if (!analyzeResponse.IsValidResponse)
            {
                _logger.LogError("Elasticsearch Response for GetTokenCount is Not Valid.  Term '{0}'", term);
                _logger.LogError("Returned error reason: {0}.", analyzeResponse.ElasticsearchServerError?.Error?.Reason);
                throw new APIErrorException(500, "Errors Occurred.");
            }

            int numberOfTokens = analyzeResponse.Tokens.Count(tok => ALLOWED_TOKEN_TYPES.Contains(tok.Type));

            return numberOfTokens;
        }

    }
}
