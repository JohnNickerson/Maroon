using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Search
{
    public static class SearchSpecificationExtensions
    {
        public static OrSpecification<T> Or<T>(this ISearchSpecification<T> spec, params ISearchSpecification<T>[] conditions)
        {
            var disjunction = new List<ISearchSpecification<T>> { spec };
            disjunction.AddRange(conditions);
            return new OrSpecification<T>(disjunction.ToArray());
        }

        public static NotSpecification<T> Not<T>(this ISearchSpecification<T> spec, params ISearchSpecification<T>[] conditions)
        {
            var negation = new List<ISearchSpecification<T>> { spec };
            negation.AddRange(conditions);
            return new NotSpecification<T>(negation.ToArray());
        }

        public static AndSpecification<T> And<T>(this ISearchSpecification<T> spec, params ISearchSpecification<T>[] conditions)
        {
            var conjunction = new List<ISearchSpecification<T>> { spec };
            conjunction.AddRange(conditions);
            return new AndSpecification<T>(conjunction.ToArray());
        }
    }
}
