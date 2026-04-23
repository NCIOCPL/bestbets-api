using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Elastic.Clients.Elasticsearch;
using NCI.OCPL.Api.Common;



namespace NCI.OCPL.Api.BestBets.Services
{
    /// <summary>
    /// This class defines a client that can be used to fetch best bets data from Elasticsearch.
    /// </summary>
    public class ESBestBetsDisplayService : IBestBetsDisplayService
    {
        private ElasticsearchClient _elasticClient;
        private CGBBIndexOptions _bestbetsConfig;
        private readonly ILogger<ESBestBetsDisplayService> _logger;

        /// <summary>
        /// Creates a new instance of a CancerGovBestBetsClient
        /// </summary>
        /// <param name="client">The client to be used for connections</param>
        /// <param name="config">The client to be used for connections</param>
        /// <param name="logger">The client to be used for connections</param>
        public ESBestBetsDisplayService(ElasticsearchClient client,
            IOptions<CGBBIndexOptions> config,
            ILogger<ESBestBetsDisplayService> logger) {
            _elasticClient = client;
            _bestbetsConfig = config.Value;
            _logger = logger;
        }

        /// <summary>
        /// Gets the best bets category list asynchronously
        /// </summary>
        /// <param name="collection">The collection to use. This will be 'live' or 'preview'.</param>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public async Task<IBestBetDisplay> GetBestBetForDisplay(string collection, string categoryID)
        {
            // Set up alias
            string alias = (collection == "preview") ?
                    this._bestbetsConfig.PreviewAliasName :
                    this._bestbetsConfig.LiveAliasName;

            // Validate category ID isn't null and is a number
            if (string.IsNullOrWhiteSpace(categoryID))
            {
                throw new ArgumentNullException("The resource identifier is null or an empty string.");
            }
            int catID;
            bool isValid = int.TryParse(categoryID, out catID);

            BestBetsCategoryDisplay result = null;

            if (isValid)
            {
                GetResponse<BestBetsCategoryDisplay> response = null;

                try
                {
                    // Fetch the category display with the given ID from the API.
                    response = await _elasticClient.GetAsync<BestBetsCategoryDisplay>(new GetRequest(alias, categoryID));

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Could not fetch category ID {categoryID.Replace(Environment.NewLine, String.Empty)}");
                    throw new APIErrorException(500, $"Could not fetch category ID {categoryID}");
                }

                // The ES client treats "Not Found" and server errors as both "Not Found" and "Not Valid",
                // so we also have to check the status code to determine what's really going on.
                if (!response.Found && response.ApiCallDetails.HttpStatusCode == 404)
                {
                    throw new APIErrorException(404, "Category not found.");
                }

                // If the API's response isn't valid, throw an error and return 500 status code.
                if (!response.IsValidResponse)
                {
                    throw new APIErrorException(500, "Errors occurred.");
                }

                result = response.Source;
            }
            else
            {
                // Throw an exception if the given ID is invalid (not an int).
                throw new APIErrorException(400, "The category identifier is invalid.");
            }

            return result;
        }
    }
}