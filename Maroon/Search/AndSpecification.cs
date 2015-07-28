using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Search
{
    public class AndSpecification<T> : ISearchSpecification<T>
    {
        private ISearchSpecification<T>[] _conds;

        public AndSpecification(params ISearchSpecification<T>[] conditions)
        {
            _conds = conditions;
        }

        public bool IsSatisfiedBy(T file)
        {
            foreach (var con in _conds)
            {
                if (!con.IsSatisfiedBy(file))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
