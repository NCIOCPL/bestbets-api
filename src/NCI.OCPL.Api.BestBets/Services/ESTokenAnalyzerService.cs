using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Elasticsearch.Net;
using Nest;

using NCI.OCPL.Api.Common;


namespace NCI.OCPL.Api.BestBets.Services
{
    /// <summary>
    /// Concrete implementation of an Elasticsearch backed ITokenAnalyzerService
    /// </summary>
    /// <seealso cref="NCI.OCPL.Api.BestBets.ITokenAnalyzerService" />
    public class ESTokenAnalyzerService : ITokenAnalyzerService
    {
        private IElasticClient _elasticClient;
        private CGBBIndexOptions _bestbetsConfig;
        private readonly ILogger<ESTokenAnalyzerService> _logger;

        /// <summary>
        /// Creates a new instance of a ESBestBetsMatchService
        /// </summary>
        public ESTokenAnalyzerService(IElasticClient client,
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

            AnalyzeResponse analyzeResponse;
            string indexForAnalysis = (collection == "preview") ?
                                        _bestbetsConfig.PreviewAliasName :
                                        _bestbetsConfig.LiveAliasName;

            try
            {
                analyzeResponse = await this._elasticClient.Indices.AnalyzeAsync(
                    a => a
                    .Index(indexForAnalysis)
                    .Analyzer("nostem")
                    .Text(term)
                );
            }
            catch(UnexpectedElasticsearchClientException ex)
            {
                _logger.LogError(ex, "Error analyzing token count for term '{0}'. Reason: '{1}'. DebugInformation: {2}",
                    term.Replace(Environment.NewLine, String.Empty),
                    ex.FailureReason, ex.DebugInformation);
                _logger.LogInformation("Trying again for term '{0}", term);

                // Try again. (this is really just for when we run out of sockets)
                analyzeResponse = await this._elasticClient.Indices.AnalyzeAsync(
                    a => a
                    .Index(indexForAnalysis)
                    .Analyzer("nostem")
                    .Text(term)
                );
            }

            if (!analyzeResponse.IsValid)
            {
                _logger.LogError("Elasticsearch Response for GetTokenCount is Not Valid.  Term '{0}'", term);
                _logger.LogError("Returned debug info: {0}.", analyzeResponse.DebugInformation);
                throw new APIErrorException(500, "Errors Occurred.");
            }

            int numberOfTokens = analyzeResponse.Tokens.Count(tok => ALLOWED_TOKEN_TYPES.Contains(tok.Type));

            return numberOfTokens;
        }

    }
}
