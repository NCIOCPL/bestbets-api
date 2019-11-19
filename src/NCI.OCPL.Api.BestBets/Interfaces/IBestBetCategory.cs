namespace NCI.OCPL.Api.BestBets
{
    /// <summary>
    /// Represents all the data for a Best Bet Category
    /// </summary>
    public interface IBestBetCategory : IBestBetDisplay
    {
        /// <summary>
        /// Is this Best Bet's Category Name an Exact Match? 
        /// </summary>
        bool IsExactMatch { get; }

        /// <summary>
        /// Gets the language of this Best Bet Category
        /// </summary>
        /// <returns></returns>
        string Language { get; }

        /// <summary>
        ///No clue what this is...
        /// </summary>
        bool Display { get; }

        /// <summary>
        /// Gets all of the Synonyms that if matched would INCLUDE this category in results
        /// </summary>
        IBestBetSynonym[] IncludeSynonyms { get; }

        /// <summary>
        /// Gets all of the Synonyms that if matched would EXCLUDE this category in results
        /// </summary>
        IBestBetSynonym[] ExcludeSynonyms { get; }

    }
}