using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Memory;
using SP_Shopping.Data;
using SP_Shopping.Models;
using System.Linq.Expressions;

namespace SP_Shopping.Repository;

public class RepositoryBaseCaching<TEntity>(ApplicationDbContext context, IMemoryCache memoryCache) : RepositoryBase<TEntity>(context), IRepository<TEntity> where TEntity : class
{
    private readonly MemoryCacher _memoryCacher = new MemoryCacher(memoryCache);
    private readonly HashSet<string> ListCacheKeys = [];
    private readonly Dictionary<string, HashSet<string>> KeyCacheKeys = [];
    private readonly Dictionary<string, HashSet<string>> SingleCacheKeys = [];

    private void RemoveListKeys()
    {
        foreach (var value in ListCacheKeys)
        {
            _memoryCacher.Remove(value);
        }
    }

    private void AddToKeyCacheKeys(string cacheKey, object?[]? keyValues)
    {
        if (!SingleCacheKeys.ContainsKey(keyValues.ToString()!))
        {
            SingleCacheKeys.Add(keyValues.ToString()!, new HashSet<string>());
        }
        SingleCacheKeys[keyValues.ToString()!].Add(cacheKey);
    }

    public override List<TEntity> GetAll()
    {
        var cacheKey = $"{nameof(TEntity)}List";
        return _memoryCacher.GetOrCreate(cacheKey, base.GetAll);
    }

    public override List<TEntity> GetAll(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        var cacheKey = $"{nameof(TEntity)}List_{query.GetHashCode()}";
        ListCacheKeys.Add(cacheKey);
        return _memoryCacher.GetOrCreate(cacheKey, () => base.GetAll(query));
    }

    public override async Task<List<TEntity>> GetAllAsync()
    {
        var cacheKey = $"{nameof(TEntity)}List";
        return await _memoryCacher.GetOrCreate(cacheKey, async () => await base.GetAllAsync());
    }
    public override async Task<List<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        var cacheKey = $"{nameof(TEntity)}List_{query.GetHashCode()}";
        ListCacheKeys.Add(cacheKey);
        return await _memoryCacher.GetOrCreate(cacheKey, async () => await base.GetAllAsync(query));
    }
    public override TEntity? GetByKey(params object?[]? keyValues)
    {
        //if (keyValues == null) return base.GetByKey(keyValues);
        //var cacheKey = $"{nameof(TEntity)}Key_{keyValues!.ToString()}";
        //AddToKeyCacheKeys(cacheKey, keyValues);
        return base.GetByKey(keyValues);
    }
    public override async Task<TEntity?> GetByKeyAsync(params object?[]? keyValues)
    {
        //if (keyValues == null) return await base.GetByKeyAsync(keyValues);
        //var cacheKey = $"{nameof(TEntity)}Key_{keyValues!.ToString()}";
        //AddToKeyCacheKeys(cacheKey, keyValues);
        return await base.GetByKeyAsync(keyValues);

    }
    public override TEntity? GetSingle(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        return base.GetSingle(query);
    }
    public override async Task<TEntity?> GetSingleAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        return await base.GetSingleAsync(query);
    }
    public override bool Create(TEntity entity)
    {
        var result = base.Create(entity);
        RemoveListKeys();
        return result;
    }

    public override async Task<bool> CreateAsync(TEntity entity)
    {
        var result = await base.CreateAsync(entity);
        RemoveListKeys();
        return result;
    }
    public override bool Update(TEntity entity)
    {
        var result = base.Update(entity);
        RemoveListKeys();
        return result;
    }
    public override async Task<bool> UpdateAsync(TEntity entity)
    {
        var result = await base.UpdateAsync(entity);
        RemoveListKeys();
        return result;
    }
    
    public override bool UpdateCertainFields
    (
        TEntity entity,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    )
    {
        var result = base.UpdateCertainFields(entity, query, setPropertyCalls);
        RemoveListKeys();
        return result;
    }

    public override async Task<bool> UpdateCertainFieldsAsync
    (
        TEntity entity,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    )
    {
        var result = await base.UpdateCertainFieldsAsync(entity, query, setPropertyCalls);
        RemoveListKeys();
        return result;
    }
    public override bool Delete(TEntity entity)
    {
        var result = base.Delete(entity);
        RemoveListKeys();
        return result;
    }
    public override async Task<bool> DeleteAsync(TEntity entity)
    {
        var result = await base.DeleteAsync(entity);
        RemoveListKeys();
        return result;
    }
    public override bool DeleteCertainEntries(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        var result = base.DeleteCertainEntries(query);
        RemoveListKeys();
        return result;
    }
    public override async Task<bool> DeleteCertainEntriesAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        var result = await base.DeleteCertainEntriesAsync(query);
        RemoveListKeys();
        return result;
    }
    public override bool Exists(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        return base.Exists(query);
    }
    public override async Task<bool> ExistsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        return await base.ExistsAsync(query);
    }
}
