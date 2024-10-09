using Microsoft.Extensions.Caching.Memory;

namespace SP_Shopping.Repository;

public class MemoryCacher(IMemoryCache memoryCache) : Attribute
{
    private readonly IMemoryCache _memoryCache = memoryCache;

    public T GetOrCreate<T>(string cacheKey, Func<T> getValue)
    {
        if (_memoryCache.TryGetValue(cacheKey, out T value)) return value!;

        value = getValue();

        _memoryCache.Set(cacheKey, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20)
        });

        return value;

    }

    public void Remove(string cacheKey)
    {
        _memoryCache.Remove(cacheKey);
    }

}

