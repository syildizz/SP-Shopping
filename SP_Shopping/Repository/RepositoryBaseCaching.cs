using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Memory;
using SP_Shopping.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace SP_Shopping.Repository;

public class RepositoryBaseCaching<TEntity>(ApplicationDbContext context, IMemoryCache memoryCache, ILogger<RepositoryBaseCaching<TEntity>> logger, CacheStorage cacheStorage) : RepositoryBase<TEntity>(context), IRepository<TEntity> where TEntity : class
{
    private readonly MemoryCacher _memoryCacher = new MemoryCacher(memoryCache);
    private readonly ILogger<RepositoryBaseCaching<TEntity>> _logger = logger;
    private readonly CacheStorage _cacheStorage = cacheStorage;

    private void RemoveKeys()
    {
        foreach (var key in _cacheStorage.CacheKeys)
        {
            _memoryCacher.Remove(key);
            _cacheStorage.CacheKeys.Remove(key);
            _logger.LogInformation("Removing key {key}", key);
        }
    }

    public override List<TEntity> GetAll()
    {
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(GetAll)}";
        _cacheStorage.CacheKeys.Add(cacheKey);
        _logger.LogInformation("Added key {key}", cacheKey);
        return _memoryCacher.GetOrCreate(cacheKey, base.GetAll);
    }

    public override List<TResult> GetAll<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query)
    {
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(GetAll)}_{query.GetMethodInfo()}_{nameof(TResult)}";
        _cacheStorage.CacheKeys.Add(cacheKey);
        _logger.LogInformation("Added key {key}", cacheKey);
        return _memoryCacher.GetOrCreate(cacheKey, () => base.GetAll(query));
    }

    public override async Task<List<TEntity>> GetAllAsync()
    {
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(GetAll)}";
        _cacheStorage.CacheKeys.Add(cacheKey);
        _logger.LogInformation("Added key {key}", cacheKey);
        return await _memoryCacher.GetOrCreate(cacheKey, async () => await base.GetAllAsync());
    }

    public override async Task<List<TResult>> GetAllAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query)
    {
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(GetAll)}_{query.GetMethodInfo()}_{nameof(TResult)}";
        _cacheStorage.CacheKeys.Add(cacheKey);
        _logger.LogInformation("Added key {key}", cacheKey);
        return await _memoryCacher.GetOrCreate(cacheKey, async () => await base.GetAllAsync(query));
    }

    public override TEntity? GetByKey(params object?[]? keyValues)
    {
        // DOESN'T WORK: KeyValues just prints "Object[]"
        //var cacheKey = $"{typeof(TEntity).FullName}_{nameof(GetByKey)}_{keyValues}";
        //_cacheStorage.CacheKeys.Add(cacheKey);
        //return _memoryCacher.GetOrCreate(cacheKey, () => base.GetByKey(keyValues));
        return base.GetByKey(keyValues);
    }

    public override async Task<TEntity?> GetByKeyAsync(params object?[]? keyValues)
    {
        // DOESN'T WORK: KeyValues just prints "Object[]"
        //var cacheKey = $"{typeof(TEntity).FullName}_{nameof(GetByKey)}_{keyValues}";
        //Console.WriteLine($"{keyValues} + {keyValues}");
        //_cacheStorage.CacheKeys.Add(cacheKey);
        //return await _memoryCacher.GetOrCreate(cacheKey, async () => await base.GetByKeyAsync(keyValues));
        return await base.GetByKeyAsync(keyValues);
    }

#nullable disable
    public override TResult GetSingle<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query)
    {
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(GetSingle)}_{query.GetMethodInfo()}_{nameof(TResult)}";
        _cacheStorage.CacheKeys.Add(cacheKey);
        _logger.LogInformation("Added key {key}", cacheKey);
        return _memoryCacher.GetOrCreate(cacheKey, () => base.GetSingle(query));
    }
    public override async Task<TResult> GetSingleAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query)
    {
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(GetSingle)}_{query.GetMethodInfo()}_{nameof(TResult)}";
        _cacheStorage.CacheKeys.Add(cacheKey);
        _logger.LogInformation("Added key {key}", cacheKey);
        return await _memoryCacher.GetOrCreate(cacheKey, () => base.GetSingleAsync(query));
    }
#nullable enable

    public override void Create(TEntity entity)
    {
        base.Create(entity);
        RemoveKeys();
    }

    public override async Task CreateAsync(TEntity entity)
    {
        await base.CreateAsync(entity);
        RemoveKeys();
    }
    public override void Update(TEntity entity)
    {
        base.Update(entity);
        RemoveKeys();
    }
    
    public override int UpdateCertainFields
    (
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    )
    {
        var result = base.UpdateCertainFields(query, setPropertyCalls);
        RemoveKeys();
        return result;
    }

    public override async Task<int> UpdateCertainFieldsAsync
    (
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    )
    {
        var result = await base.UpdateCertainFieldsAsync(query, setPropertyCalls);
        RemoveKeys();
        return result;
    }

    public override void Delete(TEntity entity)
    {
        base.Delete(entity);
        RemoveKeys();
    }

    public override int DeleteCertainEntries(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        var result = base.DeleteCertainEntries(query);
        RemoveKeys();
        return result;
    }
    
    public override async Task<int> DeleteCertainEntriesAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        var result = await base.DeleteCertainEntriesAsync(query);
        RemoveKeys();
        return result;
    }

    public override bool Exists(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(Exists)}_{query.GetMethodInfo()}";
        _cacheStorage.CacheKeys.Add(cacheKey);
        return _memoryCacher.GetOrCreate(cacheKey, () => base.Exists(query));
    }

    public override async Task<bool> ExistsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(Exists)}_{query.GetMethodInfo()}";
        _cacheStorage.CacheKeys.Add(cacheKey);
        return await _memoryCacher.GetOrCreate(cacheKey, () => base.ExistsAsync(query));
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
