﻿using System;
using System.IO;
using System.Threading.Tasks;

using Elasticsearch.Net;

using NCI.OCPL.Utils.Testing;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NCI.OCPL.Api.BestBets.Tests.Util;

namespace NCI.OCPL.Api.BestBets.Tests.ESMatchTestData
{
    /// <summary>
    /// This class is a helper around the ElasticsearchInterceptingConntection to handle
    /// analyzer responses for a BB Match Request
    /// </summary>
    /// <seealso cref="NCI.OCPL.Api.BestBets.Tests.Util.ElasticsearchInterceptingConnection" />
    public class ESMatchTokenizerConnection : ElasticsearchInterceptingConnection
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
        public ESMatchTokenizerConnection(string testFilePrefix)
        {
            this.TestFilePrefix = testFilePrefix;

            //Add Handlers            
            this.RegisterRequestHandlerForType<Nest.AnalyzeResponse>((req, res) =>
            {
                //I don't care about the request for this... for now.

                //Get the file name for this round
                res.Stream = TestingTools.GetTestFileAsStream(GetTestFileName());

                res.StatusCode = 200;
            });
        }

        private string GetTestFileName()
        {
            return string.Format("ESMatchData/{0}_analyze.json", TestFilePrefix);
        }

    }
}
