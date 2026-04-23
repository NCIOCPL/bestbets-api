using System.Threading.Tasks;

namespace NCI.OCPL.Api.BestBets
{

    /// <summary>
    /// Interface defines the methods of a service to retrieve the BestBets display (e.g. the list of links)
    /// </summary>
    public interface IBestBetsDisplayService
    {

        /// <summary>
        /// Gets a single Best Bet asynchronously
        /// </summary>
        /// <param name="collection">The collection to use. This will be 'live' or 'preview'.</param>
        /// <param name="categoryID">The category ID to retrieve</param>
        /// <returns>A IBestBetDisplay represented by the category</returns>
        Task<IBestBetDisplay> GetBestBetForDisplay(string collection, string categoryID);

    }

}