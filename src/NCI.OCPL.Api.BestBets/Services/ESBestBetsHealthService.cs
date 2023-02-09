using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Elasticsearch.Net;
using Nest;

using Newtonsoft.Json;

using NCI.OCPL.Api.BestBets;
using NCI.OCPL.Api.Common;



namespace NCI.OCPL.Api.BestBets.Services
{
    /// <summary>
    /// This class defines a client that can be used to determine whether best bets data can be fetched from Elasticsearch.
    /// </summary>
    public class ESBestBetsHealthService : IHealthCheckService
    {
        private IElasticClient _elasticClient;
        private CGBBIndexOptions _bestbetsConfig;
        private readonly ILogger<ESBestBetsHealthService> _logger;

        /// <summary>
        /// Creates a new instance of a ESBestBetsHealthService
        /// </summary>
        /// <param name="client">The client to be used for connections</param>
        /// <param name="config">The client to be used for connections</param>
        /// <param name="logger">The client to be used for connections</param>
        public ESBestBetsHealthService(IElasticClient client,
            IOptions<CGBBIndexOptions> config,
            ILogger<ESBestBetsHealthService> logger) {
            _elasticClient = client;
            _bestbetsConfig = config.Value;
            _logger = logger;
        }

        /// <summary>
        /// True if ESBestBetsHealthService is able to retrieve BestBets.
        /// </summary>
        public async Task<bool> IsHealthy()
        {

            var hostChecks = new Task<bool>[]
            {
                this.IsHostHealthy(_bestbetsConfig.LiveAliasName),
                this.IsHostHealthy(_bestbetsConfig.PreviewAliasName),
            };

            return (await Task.WhenAll(hostChecks))
                    .Aggregate(
                        true,
                        (res, next) => res && next
                    );
        }

        /// <summary>
        /// Checks if a single host is healthy
        /// </summary>
        /// <returns>The host healthy.</returns>
        private async Task<bool> IsHostHealthy(string alias)
        {
            // Use the cluster health API to verify that the Best Bets index is functioning.
            // Maps to https://localhost:9200/_cluster/health/bestbets?pretty (or other server)
            //
            // References:
            // https://www.elastic.co/guide/en/elasticsearch/reference/master/cluster-health.html
            // https://github.com/elastic/elasticsearch/blob/master/rest-api-spec/src/main/resources/rest-api-spec/api/cluster.health.json#L20

            try
            {
                //ClusterHealthResponse response = await _elasticClient.Cluster.HealthAsync(hd => hd.Index(alias));
                ClusterHealthResponse response = await _elasticClient.Cluster.HealthAsync(null, hd=> hd.Index(alias));

                if (!response.IsValid)
                {
                    _logger.LogError($"Error checking ElasticSearch health for {alias}.");
                    _logger.LogError($"Returned debug info: {response.DebugInformation}.");
                }
                else
                {
                    if (response.Status == Health.Green || response.Status == Health.Yellow)
                    {
                        //This is the only condition that will return true
                        return true;
                    }
                    else
                    {
                        _logger.LogError($"Alias ${alias} status is not good");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking ElasticSearch health for {alias}.");
                _logger.LogError($"Exception: {ex.Message}.");
            }
            return false;
        }
    }
}