using System.Collections.Generic;
using System.Linq;

using Xunit;

using NCI.OCPL.Utils.Testing;

using NCI.OCPL.Api.BestBets.Tests.CategoryTestData;
using System.Xml;
using System.IO;
using System;

namespace NCI.OCPL.Api.BestBets.Tests
{
    /// <summary>
    /// Tests for the CancerGovBestBet class, 
    /// and by extension the CancerGovBestBetSynonym class.
    /// These are really just deserialization tests,    
    /// </summary>
    public class CancerGovBestBetSynonymTest
    {
        [Fact]
        public void GetSchema_ReturnsNull()
        {
            CancerGovBestBetSynonym bestBetSyn = new CancerGovBestBetSynonym();

            Assert.Null(bestBetSyn.GetSchema());
        }

        [Fact]
        public void WriteXML_ThrowsNotSupportedException()
        {
            CancerGovBestBetSynonym bestBetSyn = new CancerGovBestBetSynonym();
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "\t"
            };

            XmlWriter xmlWriter = XmlWriter.Create(new StringWriter());
                
            Assert.Throws<NotSupportedException>(() => bestBetSyn.WriteXml(xmlWriter));
            
        }
    }
}