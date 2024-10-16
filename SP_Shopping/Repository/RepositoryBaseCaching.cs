using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
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

    public override List<TResult> GetAll<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query)
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
    public override async Task<List<TResult>> GetAllAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query)
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
    public override TResult? GetSingle<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query) where TResult : class
    {
        var result = base.GetSingle(query);
        return result;
    }
    public override async Task<TResult?> GetSingleAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query) where TResult: class
    {
        return await base.GetSingleAsync(query);
    }
    public override void Create(TEntity entity)
    {
        base.Create(entity);
        RemoveListKeys();
    }

    public override async Task CreateAsync(TEntity entity)
    {
        await base.CreateAsync(entity);
        RemoveListKeys();
    }
    public override void Update(TEntity entity)
    {
        base.Update(entity);
        RemoveListKeys();
    }
    
    public override int UpdateCertainFields
    (
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    )
    {
        var result = base.UpdateCertainFields(query, setPropertyCalls);
        RemoveListKeys();
        return result;
    }

    public override async Task<int> UpdateCertainFieldsAsync
    (
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    )
    {
        var result = await base.UpdateCertainFieldsAsync(query, setPropertyCalls);
        RemoveListKeys();
        return result;
    }
    public override void Delete(TEntity entity)
    {
        base.Delete(entity);
        RemoveListKeys();
    }
    public override int DeleteCertainEntries(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        var result = base.DeleteCertainEntries(query);
        RemoveListKeys();
        return result;
    }
    public override async Task<int> DeleteCertainEntriesAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
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

    public override int SaveChanges()
    {
        return base.SaveChanges();
    } 

    public override async Task<int> SaveChangesAsync()
    {
        return await base.SaveChangesAsync();
    }

}
