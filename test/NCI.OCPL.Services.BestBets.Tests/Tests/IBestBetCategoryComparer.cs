using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace NCI.OCPL.Services.BestBets.Tests
{

    /// <summary>
    /// Comparer for IBestBetSynonym
    /// </summary>
    public class IBestBetSynonymComparer : IEqualityComparer<IBestBetSynonym>
    {
        public bool Equals(IBestBetSynonym x, IBestBetSynonym y)
        {

            // If the items are both null, or if one or the other is null, return 
            // the correct response right away.
            if (x == null && y== null) 
            {
                return true;
            } 
            else if (x == null || y == null)
            {
                return false;
            }

            bool isEqual =
                x.IsExactMatch == y.IsExactMatch
                && x.Text == y.Text;
                                
            return isEqual;
        }

        public int GetHashCode(IBestBetSynonym obj)
        {
            int hash = 0;
            hash ^= obj.IsExactMatch.GetHashCode();
            hash ^= obj.Text.GetHashCode();

            return hash;
        }
    }

    /// <summary>
    /// A IEqualityComparer for IBestBetCategory
    /// </summary>
    public class IBestBetCategoryComparer : IEqualityComparer<IBestBetCategory>
    {
            public bool Equals(IBestBetCategory x, IBestBetCategory y)
            {
                // If the items are both null, or if one or the other is null, return 
                // the correct response right away.
                if (x == null && y== null) 
                {
                    return true;
                } 
                else if (x == null || y == null)
                {
                    return false;
                }                

                bool isEqual = 
                    x.Name == y.Name 
                    && x.ID == y.ID
                    && x.Display == y.Display
                    && x.Weight == y.Weight
                    //&& x.HTML == y.HTML
                    && x.IsExactMatch == y.IsExactMatch
                    && x.Language == y.Language
                    && AreSynonymListsEqual(x.ExcludeSynonyms, y.ExcludeSynonyms)
                    && AreSynonymListsEqual(x.IncludeSynonyms, y.IncludeSynonyms);
                    
                
                return isEqual;                
            }

            /// <summary>
            /// Helper function to determine if two synonym lists are equal, order does not matter.
            /// </summary>
            /// <param name="x">Synonym list 1</param>
            /// <param name="y">Synonym list 2</param>
            /// <returns></returns>
            private bool AreSynonymListsEqual(IBestBetSynonym[] x, IBestBetSynonym[] y) {
                // If the items are both null, or if one or the other is null, return 
                // the correct response right away.
                
                if (x == null && y== null) 
                {
                    return true;
                } 
                else if (x == null || y == null)
                {
                    return false;
                }                

                //Generate a set of those values that are not in both lists.
                //if this is not 0, then there is an error.
                var diffxy = x.Except(y, new IBestBetSynonymComparer());

                return diffxy.Count() == 0;
            }

            public int GetHashCode(IBestBetCategory obj)
            {
                int hash = 0;
                hash ^= 
                    obj.ID.GetHashCode()
                    ^ obj.Name.GetHashCode()
                    ^ obj.Display.GetHashCode()
                    ^ obj.Language.GetHashCode()
                    ^ obj.IsExactMatch.GetHashCode()
                    ^ obj.HTML.GetHashCode()
                    ^ obj.IncludeSynonyms.GetHashCode()
                    ^ obj.ExcludeSynonyms.GetHashCode();

                return hash;
            }
        
    }
}