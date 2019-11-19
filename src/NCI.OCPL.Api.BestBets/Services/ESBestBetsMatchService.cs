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
    //TODO: Remove APIExceptions from this class and make them regular exceptions.  
    //TODO: Maybe remove the logger too as this class no longer needs to log, the calling class should.  

    /// <summary>
    /// Class represents an Elasticsearch based Best Bets Match Service
    /// </summary>
    public class ESBestBetsMatchService : IBestBetsMatchService
    {
        private IElasticClient _elasticClient;
        private ITokenAnalyzerService _tokenAnalyzer;
        private CGBBIndexOptions _bestbetsConfig;
        private readonly ILogger<ESBestBetsMatchService> _logger;

        /// <summary>
        /// Creates a new instance of a ESBestBetsMatchService
        /// </summary>        
        public ESBestBetsMatchService(IElasticClient client,
            ITokenAnalyzerService tokenAnalyzer,
            IOptions<CGBBIndexOptions> config,
            ILogger<ESBestBetsMatchService> logger)
        {
            _elasticClient = client;
            _tokenAnalyzer = tokenAnalyzer;
            _bestbetsConfig = config.Value;
            _logger = logger;
        }

        /// <summary>
        /// Gets a list of the BestBet Category IDs that matched our term asynchronously
        /// </summary>
        /// <param name="collection">The search index to use</param>
        /// <param name="language">The two-character language code to constrain the matches to</param>
        /// <param name="cleanedTerm">A term that have been cleaned of punctuation and special characters</param>
        /// <returns>An array of category ids</returns>
        public async Task<string[]> GetMatches(string collection, string language, string cleanedTerm)
        {

            // Step 2. Get Number of Tokens in the term
            int numTokens = await _tokenAnalyzer.GetTokenCount(collection, cleanedTerm);

            // Step 4. Iterate over the matches    
            IEnumerable<BestBetsMatch> matches = await GetBestBetMatchesAsync(
                collection,
                cleanedTerm,
                language,
                numTokens
            );

            // Step 5. Process the matches and extract only the category IDs we will
            // be returning to the client
            string[] validCategories = FilterValidCategories(matches, numTokens);

            return validCategories;
        }

        /// <summary>
        /// Process a list of BestBetsMatches and returns an array of category IDs for display. 
        /// </summary>
        /// <param name="matches">A list of matches</param>
        /// <param name="numTokens">The number of tokens in the search term</param>
        /// <returns>An array of category ids</returns>
        private string[] FilterValidCategories(IEnumerable<BestBetsMatch> matches, int numTokens)
        {
            //CatIDs to ignore because of a negation
            List<string> excludedIDs = new List<string>();

            //The IDs that we will end up sending back to the client.
            List<string> matchedIDs = new List<string>();

            // Iterate through ALL the matches and extract the categories that
            // should be displayed.  There may be multiple matches for a single
            // category, the probability increase with the number of tokens. 
            foreach (BestBetsMatch match in matches)
            {

                //Exact matches need to match the exact number of tokens as well.
                //Exact matches can be used for both inclusion and exclusion
                if (match.IsExact && (match.TokenCount != numTokens))
                    continue;


                if (match.IsNegated)
                {
                    // A negated match will remove a category from the display.
                    // For example, "Breast Cancer Treatment" would return the 
                    // Best Bets for "Breast Cancer" and "Breast Cancer Treatment".
                    // However, as "Breast Cancer Treatment" is more specific, a 
                    // BB editor has created a Negated synonyn of "Treatment" for
                    // the "Breast Cancer" category.  So we should only show
                    // "Breast Cancer Treatment" to the user. 
                    if (!excludedIDs.Contains(match.ContentID))
                    {
                        excludedIDs.Add(match.ContentID);
                    }

                    matchedIDs.Remove(match.ContentID);
                }
                else
                {
                    // Just a normal match.  Let's make sure that we are not excluding
                    // that category first, otherwise, add it to the list of matches.
                    if (!matchedIDs.Contains(match.ContentID)
                        && !excludedIDs.Contains(match.ContentID))
                    {
                        matchedIDs.Add(match.ContentID);
                    }
                }
            }

            return matchedIDs.ToArray();
        }

        /// <summary>
        /// Gets a "round" of Best Bet matches asynchronously
        /// </summary>
        /// <returns>The set of matches.</returns>
        /// <param name="collection">The search index to use</param> 
        /// <param name="cleanedTerm">The search phrase</param>
        /// <param name="searchTokenCount">Search token count.</param>
        /// <param name="lang">Lang.</param>
        /// <param name="matchedTokenCount">Matched token count.</param>
        private async Task<IEnumerable<BestBetsMatch>> GetSetOfMatchesAsync(string collection, string cleanedTerm, int searchTokenCount, string lang, int matchedTokenCount)
        {
            //This is the query
            var matchQuery = new NumericRangeQuery { Field = "tokencount", LessThanOrEqualTo = matchedTokenCount } &&
                             new TermQuery { Field = "is_exact", Value = 0 } &&
                             new TermQuery { Field = "language", Value = lang } &&
                             new MatchQuery { Field = "synonym", Query = cleanedTerm, MinimumShouldMatch = matchedTokenCount };

            if (searchTokenCount == matchedTokenCount)
            {
                //Add in exact match query too
                matchQuery = matchQuery ||
                             new TermQuery { Field = "tokencount", Value = matchedTokenCount } &&
                             new TermQuery { Field = "is_exact", Value = 1 } &&
                             new TermQuery { Field = "language", Value = lang } &&
                             new MatchQuery { Field = "synonym", Query = cleanedTerm, MinimumShouldMatch = matchedTokenCount };
            }

            try
            {
                string alias = (collection == "preview") ?
                    this._bestbetsConfig.PreviewAliasName :
                    this._bestbetsConfig.LiveAliasName;

                var req = new SearchRequest<BestBetsMatch>(alias)
                {
                    Query = matchQuery,
                    Size = 10000 //Make sure this more than the number of synonyms
                };

                var response = await this._elasticClient.SearchAsync<BestBetsMatch>(req);

                //Test if response is valid
                if (!response.IsValid)
                {
                    _logger.LogError("Elasticsearch Response is Not Valid. Term '{0}'", cleanedTerm);
                    _logger.LogError("Returned debug info: {0}.", response.DebugInformation);
                    throw new APIErrorException(500, "Errors Occurred.");
                }

                return response.Documents;

            }
            catch (APIErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Elasticsearch request failed for Term '{0}'", cleanedTerm);
                _logger.LogError(ex.Message);
                throw new APIErrorException(500, "Errors Occurred.");
            }

        }

        /// <summary>
        /// Helper function that generates a list of best bet matches to iterate through.
        /// </summary>
        /// <param name="collection">The search index to use</param> 
        /// <param name="cleanedTerm">The Cleaned Term</param>
        /// <param name="language">The language of the best bets to fetch</param>
        /// <param name="numTokens">The number of tokens an analyzer would break this up into</param>
        /// <returns>IEnumerable&lt;BestBetsMatch&gt; suitable for iterating through</returns>
        private async Task<IEnumerable<BestBetsMatch>> GetBestBetMatchesAsync(string collection, string cleanedTerm, string language, int numTokens)
        {
            //Pool up all the fetches to be called in one shot
            var tasks = from tokenCount in Enumerable.Range(1, numTokens)
                        select this.GetSetOfMatchesAsync(collection, cleanedTerm, numTokens, language, tokenCount);

            try
            {
                //Call them and wait for their results
                var results = await Task.WhenAll(tasks);

                return results.SelectMany(i => i);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}