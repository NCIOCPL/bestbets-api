﻿
using Elasticsearch.Net;
using Nest;

using Xunit;
using Moq;

using System;
using System.Threading.Tasks;

using NCI.OCPL.Utils.Testing;
using NCI.OCPL.Api.BestBets.Indexer.Services;
using Microsoft.Extensions.Options;
using NCI.OCPL.Api.BestBets.Tests.Util;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace NCI.OCPL.Api.BestBets.Indexer.Tests
{
    public class ESBBIndexerServiceTests_MakeIndexCurrentAlias : ESBBIndexerServiceTests__Base
    {
        [Fact]
        public void MakeCurrentNoExisting()
        {
            string aliasName = "bestbets";
            string newIndexName = aliasName + "20161207125500";

            string actualPath = string.Empty;
            object actualRequestOptions = null;

            ESBBIndexerService service = this.GetIndexerService(
                aliasName,
                conn =>
                {

                    //This is the handler for AliasExists calls. This handles no existing alias.
                    conn.RegisterRequestHandlerForType<Nest.ExistsResponse>((req, res) =>
                    {
                        //Setup Response
                        res.StatusCode = 404;
                    });

                    //This is the handler for Alias, and the focus of this test.
                    conn.RegisterRequestHandlerForType<BulkAliasResponse>((req, res) =>
                    {
                        actualPath = req.Path;
                        actualRequestOptions = actualRequestOptions = conn.GetRequestPost(req);

                        res.StatusCode = 200;
                        res.Stream = TestingTools.GetTestFileAsStream("ES_Acknowledged_True_Response.json");
                    });
                }
            );

            service.MakeIndexCurrentAlias(newIndexName);

            //Make sure the Request is the expected request
            //Assert.Equal("_alias/" + aliasName, actualPath);
            //Assert.Null(actualRequestOptions);

            //Are assertions will be on the actual request options.
            Assert.True(false);            

        }

        public void MakeCurrentOneExisting()
        {
            string aliasName = "bestbets";
            string newIndexName = aliasName + "20161207125500";

            string actualPath = string.Empty;
            object actualRequestOptions = null;

            ESBBIndexerService service = this.GetIndexerService(
                aliasName,
                conn =>
                {

                    //This is the handler for AliasExists calls
                    conn.RegisterRequestHandlerForType<Nest.ExistsResponse>((req, res) =>
                    {
                        //Just need to return 200 to say alias does exist
                        res.StatusCode = 200;                                                                     
                        res.Stream = null;
                        res.Exception = null;
                    });

                    //This is the handler for getting the indices associated with the alias
                    conn.RegisterRequestHandlerForType<Nest.GetAliasesResponse>((req, res) =>
                    {
                        //Store off Request
                        actualPath = req.Path;
                        actualRequestOptions = conn.GetRequestPost(req);
                        
                        //Setup Response
                        res.StatusCode = 200;
                        res.Stream = TestingTools.GetTestFileAsStream("GetIndicesForAlias_SingleItemResponse.json");
                    });                    

                    //This is the handler for Alias, and the focus of this test.
                    conn.RegisterRequestHandlerForType<BulkAliasResponse>((req, res) =>
                    {
                        actualPath = req.Path;
                        actualRequestOptions = actualRequestOptions = conn.GetRequestPost(req);

                        res.StatusCode = 200;
                        res.Stream = TestingTools.GetTestFileAsStream("ES_Acknowledged_True_Response.json");
                    });
                }
            );

            service.MakeIndexCurrentAlias(newIndexName);

            //Make sure the Request is the expected request
            //Assert.Equal("_alias/" + aliasName, actualPath);
            //Assert.Null(actualRequestOptions);

            //Are assertions will be on the actual request options.
            Assert.True(false);            

        }

        [Fact]
        public void MakeCurrentManyExisting()
        {
            string aliasName = "bestbets";
            string newIndexName = aliasName + "20161207125500";

            string actualPath = string.Empty;
            JObject actualRequestOptions = null;

            ESBBIndexerService service = this.GetIndexerService(
                aliasName,
                conn =>
                {

                    //This is the handler for AliasExists calls
                    conn.RegisterRequestHandlerForType<Nest.ExistsResponse>((req, res) =>
                    {
                        //Just need to return 200 to say alias does exist
                        res.StatusCode = 200;
                        res.Stream = null;
                        res.Exception = null;
                    });

                    //This is the handler for getting the indices associated with the alias
                    conn.RegisterRequestHandlerForType<Nest.GetAliasesResponse>((req, res) =>
                    {
                        //Store off Request
                        actualPath = req.Path;
                        actualRequestOptions = conn.GetRequestPost(req);

                        //Setup Response
                        res.StatusCode = 200;
                        res.Stream = TestingTools.GetTestFileAsStream("GetIndicesForAlias_MultiItemResponse.json");
                    });

                    //This is the handler for Alias, and the focus of this test.
                    conn.RegisterRequestHandlerForType<BulkAliasResponse>((req, res) =>
                    {
                        actualPath = req.Path;
                        actualRequestOptions = actualRequestOptions = conn.GetRequestPost(req);

                        res.StatusCode = 200;
                        res.Stream = TestingTools.GetTestFileAsStream("ES_Acknowledged_True_Response.json");
                    });
                }
            );

            service.MakeIndexCurrentAlias(newIndexName);

            // Make sure the Request is the expected request
            // Index being added should be newIndexName
            // Alias being addded should be aliasName
            string[] expectedAddIndices = new string[] { newIndexName };
            string[] expectedRemoveIndices = new string[] { "bestbets20161202152737", "bestbets20161201165226" };

            // Pull out the lists of aliases and indexes being added/removed.
            string[] actualAddedAliases =
                (from act in actualRequestOptions["actions"] where act["add"] != null
                 select (string)act["add"]["alias"]).ToArray();

            string[] actualAddedIndices =
                (from act in actualRequestOptions["actions"] where act["add"] != null
                 select (string)act["add"]["index"]).ToArray();

            string[] actualRemovedAliases =
                (from act in actualRequestOptions["actions"] where act["remove"] != null
                 select (string)act["remove"]["alias"]).ToArray();

            string[] actualRemovedIndices =
                (from act in actualRequestOptions["actions"] where act["remove"] != null
                 select (string)act["remove"]["index"]).ToArray();

            // Items where both actions are missing, or both are present.  (List should be empty.)
            var unexpectedList =
                from act in actualRequestOptions["actions"]
                where (act["add"] != null && act["remove"] != null) || (act["add"] == null && act["remove"] == null)
                select act;

            // Check list lengths.
            bool correctListSizes =
                actualAddedAliases.Length == 1
                && actualAddedIndices.Length == 1
                && actualRemovedAliases.Length == 2
                && actualRemovedIndices.Length == 2
                && unexpectedList.Count() == 0;

            // Check that all expected indices are accounted for.
            bool expectedAddIndicesPresent = expectedAddIndices.All(index => actualAddedIndices.Contains(index));
            bool expectedRemoveIndicesPresent = expectedRemoveIndices.All(index => actualRemovedIndices.Contains(index));

            // All aliases should match aliasName.
            bool addAliasCorrect = actualAddedAliases.All(alias => alias == aliasName);
            bool removeAliasCorrect = actualRemovedAliases.All(alias => alias == aliasName);


            Assert.True(correctListSizes
                && expectedAddIndicesPresent
                && expectedRemoveIndicesPresent
                && addAliasCorrect
                && removeAliasCorrect);
        }
    }
}
