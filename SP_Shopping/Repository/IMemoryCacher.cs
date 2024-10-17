
namespace SP_Shopping.Repository;

public interface IMemoryCacher<TKey>
{
    void Clear();
    void ClearWith(Func<TKey, bool> filter);
    TValue GetOrCreate<TValue>(TKey cacheKey, Func<TValue> getValue);
    void Remove(string cacheKey);
}