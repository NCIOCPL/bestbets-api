﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCI.OCPL.Services.CDE.PublishedContentListing
{
    public interface IPublishedContentListing
    {
        IEnumerable<string> Directories { get; set; }

        IEnumerable<IPublishedContentInfo> Files { get; set; }
    }
}
