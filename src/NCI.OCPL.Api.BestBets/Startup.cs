using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Nest;
using Elasticsearch.Net;

using NSwag.AspNetCore;
using NJsonSchema;
using System.Reflection;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.BestBets.Services;

namespace NCI.OCPL.Api.BestBets
{
    /// <summary>
    /// Defines the configuration for the Best Bets API
    /// </summary>
    public class Startup : NciStartupBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:NCI.OCPL.Api.BestBets.Startup"/> class.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        public Startup(IConfiguration configuration)
            : base(configuration) { }


        /*****************************
         * ConfigureServices methods *
         *****************************/

        /// <summary>
        /// Adds the configuration mappings.
        /// </summary>
        /// <param name="services">Services.</param>
        protected override void AddAdditionalConfigurationMappings(IServiceCollection services)
        {
            services.Configure<CGBBIndexOptions>(Configuration.GetSection("CGBestBetsIndex"));

        }

        /// <summary>
        /// Adds in the application specific services
        /// </summary>
        /// <param name="services">Services.</param>
        protected override void AddAppServices(IServiceCollection services)
        {
            //Add our Health Service
            services.AddTransient<IHealthCheckService, ESBestBetsHealthService>();

            //Add our Display Service
            services.AddTransient<IBestBetsDisplayService, ESBestBetsDisplayService>();

            //Add in the token analyzer
            services.AddTransient<ITokenAnalyzerService, ESTokenAnalyzerService>();

            //Add our Match Service
            services.AddTransient<IBestBetsMatchService, ESBestBetsMatchService>();
        }

        /*****************************
         *     Configure methods     *
         *****************************/

        /// <summary>
        /// Configure the specified app and env.
        /// </summary>
        /// <returns>The configure.</returns>
        /// <param name="app">App.</param>
        /// <param name="env">Env.</param>
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        protected override void ConfigureAppSpecific(IApplicationBuilder app, IWebHostEnvironment env)
        {
            return;
        }

    }
}
