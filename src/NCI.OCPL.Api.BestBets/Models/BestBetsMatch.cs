using System;

using Nest;

namespace NCI.OCPL.Api.BestBets
{
    /// <summary>
    /// Represents a Best Best Match from the Search Engine
    /// </summary>
    [ElasticsearchType(RelationName = "synonyms")]
    public class BestBetsMatch
    {
        /// <summary>
        /// Gets or sets the name of the category for this Best Bet Match
        /// </summary>
        [Text(Name = "category")]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the content ID of the category of this match
        /// </summary>
        [Text(Name = "contentid")]
        public string ContentID {get; set;}

        /// <summary>
        /// Gets or sets the synonym that was matched
        /// </summary>
        [Text(Name = "synonym")]
        public string Synonym {get; set;}

        /// <summary>
        /// Gets or sets the two-character language code for this match
        /// </summary>
        [Text(Name = "language")]
        public string Language {get; set;}

        /// <summary>
        /// Gets or sets whether this match is a negated one or not.
        /// </summary>
        [Boolean(Name = "is_negated")]
        public bool IsNegated {get; set;}

        /// <summary>
        /// Gets or sets whether this match is an exact one or not.
        /// </summary>
        [Boolean(Name = "is_exact")]
        public bool IsExact {get; set;}

        /// <summary>
        /// Gets or sets the number of tokens the ElasticSearch analyzer would
        /// return for this synonym.
        /// </summary>
        [Number(Name = "tokencount")]
        public int TokenCount {get; set;}

    }
}