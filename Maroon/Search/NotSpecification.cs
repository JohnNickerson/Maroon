using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Search
{
    public class NotSpecification<T> : ISearchSpecification<T>
    {
        private ISearchSpecification<T>[] _conds;

        public NotSpecification(params ISearchSpecification<T>[] conditions)
        {
            _conds = conditions;
        }

        public bool IsSatisfiedBy(T file)
        {
            foreach (var c in _conds)
            {
                if (c.IsSatisfiedBy(file))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
