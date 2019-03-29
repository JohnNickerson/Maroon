namespace AssimilationSoftware.Maroon.Search
{
    public interface ISearchSpecification<T>
    {
        bool IsSatisfiedBy(T item);
    }
}
