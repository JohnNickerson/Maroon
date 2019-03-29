namespace AssimilationSoftware.Maroon.Search
{
    public class TrueSpecification<T> : ISearchSpecification<T>
    {
        public bool IsSatisfiedBy(T item)
        {
            return true;
        }
    }
}
