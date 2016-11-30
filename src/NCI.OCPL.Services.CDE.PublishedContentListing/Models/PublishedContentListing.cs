using System;
using System.Collections.Generic;

namespace NCI.OCPL.Services.CDE.PublishedContentListing
{
    /// <summary>
    /// A collection of metadata structures for CancerGov published content.
    /// </summary>

    class PublishedContentListing : IPublishedContentListing
    {
        public string[] Directories {get; set;}

        public PublishedContentInfo[] Files {get; set;}

        IEnumerable<string> IPublishedContentListing.Directories
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        IEnumerable<IPublishedContentInfo> IPublishedContentListing.Files
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}