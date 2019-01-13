using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Search
{
    /// <summary>
    /// A constant search specification that is never satisfied.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FalseSpecification<T> : ISearchSpecification<T>
    {
        /// <summary>
        /// Tests whether the item satisfies the specification.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>False.</returns>
        public bool IsSatisfiedBy(T item)
        {
            return false;
        }
    }
}
