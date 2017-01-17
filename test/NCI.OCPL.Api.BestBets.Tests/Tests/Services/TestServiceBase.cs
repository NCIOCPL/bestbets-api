﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Moq;


namespace NCI.OCPL.Api.BestBets.Tests
{
    /// <summary>
    /// Provides common methods used by service test classes.
    /// </summary>
    public class TestServiceBase
    {
        /// <summary>
        /// Helper method to create a mocked up CGBBIndexOptions object.
        /// </summary>
        /// <returns></returns>
        protected IOptions<CGBBIndexOptions> GetMockConfig()
        {
            Moq.Mock<IOptions<CGBBIndexOptions>> config = new Mock<IOptions<CGBBIndexOptions>>();
            config
                .SetupGet(o => o.Value)
                .Returns(new CGBBIndexOptions()
                {
                    AliasName = "BestBets"
                });

            return config.Object;
        }


    }
}
