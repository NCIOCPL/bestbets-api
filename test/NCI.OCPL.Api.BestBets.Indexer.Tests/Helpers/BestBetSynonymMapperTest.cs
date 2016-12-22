using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using NCI.OCPL.Utils.Testing;
using NCI.OCPL.Api.BestBets.Tests.Util;

using NCI.OCPL.Api.BestBets.Indexer.Tests.CategoryTestData;

namespace NCI.OCPL.Api.BestBets.Indexer.Tests

{
    public class BestBetSynonymMapperTest
    {
        public static IEnumerable<object[]> MappingData => new[] {
            new object[] { new PancoastTumorSynonymTestData() },
            new object[] { new BreastCancerSynonymTestData() }
        };


        /// <summary>
        /// Verify that the correct number of synonmyms is created.
        /// </summary>
        /// <param name="data">The BaseSynonymTestData containing reference data.</param>
        [Theory, MemberData("MappingData")]
        void CorrectNumberOfSynonymsFound(BaseSynonymTestData data)
        {
            // Load test BestBet object
            string filePath = data.TestFilePath;
            CancerGovBestBet testData = TestingTools.DeserializeXML<CancerGovBestBet>(filePath);

            int expectedCount = data.ExpectedMatches.Count();

            // Create a BestBetMapper from a test BestBet.
            BestBetSynonymMapper mapper = new BestBetSynonymMapper(testData);

            int actualCount = mapper.Count();

            // Verify that returned list of BestBetMatch objects matches what's expected.
            Assert.Equal(expectedCount, actualCount);
        }

        /// <summary>
        /// Verify that synonyms are correctly converted into BestBetsMatch objects.
        /// </summary>
        /// <param name="data"></param>
        [Theory, MemberData("MappingData")]
        void SynonymsMappedCorrectly(BaseSynonymTestData data)
        {
            // Load test BestBet object
            string filePath = data.TestFilePath;
            CancerGovBestBet testData = TestingTools.DeserializeXML<CancerGovBestBet>(filePath);

            // Put the expected matches into a dictionary for fast lookup.
            Dictionary<string, BestBetsMatch> dictExpectedMatches = new Dictionary<string, BestBetsMatch>();
            foreach (BestBetsMatch item in data.ExpectedMatches)
                dictExpectedMatches.Add(item.Synonym, item);

            // Create a BestBetMapper from a test BestBet.
            BestBetSynonymMapper mapper = new BestBetSynonymMapper(testData);

            Assert.All(mapper, match => {
                string synonym = match.Synonym;
                Assert.True(dictExpectedMatches.ContainsKey(synonym));
                Assert.Equal(dictExpectedMatches[synonym], match, new BestBetsMatchComparer());
            });
        }
    }
}
