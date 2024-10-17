using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

namespace SP_Shopping.Repository;

public class MemoryCacher<TKey>(IMemoryCache memoryCache) : IMemoryCacher<TKey>
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly HashSet<TKey> CacheKeys = [];

    public TValue GetOrCreate<TValue>(TKey cacheKey, Func<TValue> getValue)
    {
        if (_memoryCache.TryGetValue(cacheKey, out TValue value)) return value!;

        value = getValue();

        _memoryCache.Set(cacheKey, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6)
        });
        CacheKeys.Add(cacheKey);

        return value;

    }

    public void Remove(string cacheKey)
    {
        _memoryCache.Remove(cacheKey);
    }

    public void Clear()
    {
        foreach (var key in CacheKeys)
        {
            _memoryCache.Remove(key);
            CacheKeys.Remove(key);
        }
    }

    public void ClearWith(Func<TKey, bool> filter)
    {
        foreach (var key in CacheKeys.Where(filter))
        {
            _memoryCache.Remove(key);
            CacheKeys.Remove(key);
        }
    }

}

