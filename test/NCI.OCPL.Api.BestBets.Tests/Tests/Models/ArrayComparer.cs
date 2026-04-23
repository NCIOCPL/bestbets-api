using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NCI.OCPL.Api.BestBets.Tests
{

    /// <summary>
    /// A IEqualityComparer for IBestBetDisplay
    /// </summary>
    public class ArrayComparer : IEqualityComparer<string[]>
    {
        /// <summary>
        /// Helper function to determine param arrays are equal, order does not matter.
        /// </summary>
        /// <param name="x">Param array 1</param>
        /// <param name="y">Param array 2</param>
        /// <returns></returns>
        public bool Equals(string[] x, string[] y)
        {
            // If the items are both null, or if one or the other is null, return
            // the correct response right away.

            if (x == null && y == null)
            {
                return true;
            }
            else if (x == null || y == null)
            {
                return false;
            }

            if (x.Count() != y.Count())
            {
                return false;
            }

            //Generate a set of those values that are not in both lists.
            //if this is not 0, then there is an error.
            var diffxy = x.Except(y);

            return diffxy.Count() == 0;
        }

        public int GetHashCode(string[] obj)
        {
            int hash = 0;
            hash ^=
                obj.GetHashCode();

            return hash;
        }
    }
}