using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssimilationSoftware.Maroon.Search
{
    public interface ISearchSpecification<T>
    {
        bool IsSatisfiedBy(T item);
    }
}
