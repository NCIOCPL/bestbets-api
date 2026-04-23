using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;

using NCI.OCPL.Api.Common;


namespace NCI.OCPL.Api.BestBets
{
    /// <summary>
    /// Host application for BestBets.
    /// </summary>
    public class Program : NciApiProgramBase
    {
        /// <summary>
        /// The main, where it all begins.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            CreateHostBuilder<Startup>(args).Build().Run();
        }
    }
}