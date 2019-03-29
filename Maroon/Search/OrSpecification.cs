namespace AssimilationSoftware.Maroon.Search
{
    public class OrSpecification<T> : ISearchSpecification<T>
    {
        private ISearchSpecification<T>[] _conds;

        public OrSpecification(params ISearchSpecification<T>[] orconditions)
        {
            _conds = orconditions;
        }

        public bool IsSatisfiedBy(T file)
        {
            foreach (var con in _conds)
            {
                if (con.IsSatisfiedBy(file))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
