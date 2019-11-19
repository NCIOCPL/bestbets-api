using System;
using Nest;

namespace NCI.OCPL.Api.BestBets
{
    /// <summary>
    /// Represents Display information about a Best Bet
    /// </summary>
    [ElasticsearchType(Name = "categorydisplay")]
    public class BestBetsCategoryDisplay : IBestBetDisplay
    {
        /// <summary>
        /// Gets or sets the name of the category for this Best Bet Match
        /// </summary>
        [Text(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the content ID of the category of this match
        /// </summary>
        [Text(Name = "contentid")]
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets the HTML for display for this category
        /// </summary>
        [Text(Name = "content")]
        public string HTML { get; set; }

        /// <summary>
        /// Gets the weight of this category to determine ordering on display
        /// </summary>
        [Text(Name = "weight")]
        public int Weight { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BestBetsCategoryDisplay() { }
    }
}
