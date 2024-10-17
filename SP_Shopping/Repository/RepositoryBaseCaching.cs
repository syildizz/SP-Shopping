using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Memory;
using SP_Shopping.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace SP_Shopping.Repository;

public class RepositoryBaseCaching<TEntity>(ApplicationDbContext context, IMemoryCacher<string> memoryCacher, ILogger<RepositoryBaseCaching<TEntity>> logger) : RepositoryBase<TEntity>(context), IRepository<TEntity> where TEntity : class
{
    private readonly IMemoryCacher<string> _memoryCacher = memoryCacher;
    private readonly ILogger<RepositoryBaseCaching<TEntity>> _logger = logger;

    public override List<TEntity> GetAll()
    {
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(GetAll)}";
        _logger.LogInformation("Adding key {key}", cacheKey);
        return _memoryCacher.GetOrCreate(cacheKey, base.GetAll);
    }

    public override List<TResult> GetAll<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query)
    {
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(GetAll)}_{query.GetMethodInfo()}_{typeof(TResult).FullName}";
        _logger.LogInformation("Adding key {key}", cacheKey);
        return _memoryCacher.GetOrCreate(cacheKey, () => base.GetAll(query));
    }

    public override async Task<List<TEntity>> GetAllAsync()
    {
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(GetAll)}";
        _logger.LogInformation("Adding key {key}", cacheKey);
        return await _memoryCacher.GetOrCreate(cacheKey, async () => await base.GetAllAsync());
    }

    public override async Task<List<TResult>> GetAllAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query)
    {
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(GetAll)}_{query.GetMethodInfo()}_{typeof(TResult).FullName}";
        _logger.LogInformation("Adding key {key}", cacheKey);
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
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(GetSingle)}_{query.GetMethodInfo()}_{typeof(TResult).FullName}";
        _logger.LogInformation("Adding key {key}", cacheKey);
        return _memoryCacher.GetOrCreate(cacheKey, () => base.GetSingle(query));
    }
    public override async Task<TResult> GetSingleAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query)
    {
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(GetSingle)}_{query.GetMethodInfo()}_{typeof(TResult).FullName}";
        _logger.LogInformation("Adding key {key}", cacheKey);
        return await _memoryCacher.GetOrCreate(cacheKey, () => base.GetSingleAsync(query));
    }
#nullable enable

    public override void Create(TEntity entity)
    {
        base.Create(entity);
        _logger.LogInformation("Clearing cache for {Type}", typeof(TEntity).FullName);
        _memoryCacher.ClearWith(k => k.StartsWith(typeof(TEntity).FullName!));
    }

    public override async Task CreateAsync(TEntity entity)
    {
        await base.CreateAsync(entity);
        _logger.LogInformation("Clearing cache for {Type}", typeof(TEntity).FullName);
        _memoryCacher.ClearWith(k => k.StartsWith(typeof(TEntity).FullName!));
    }
    public override void Update(TEntity entity)
    {
        base.Update(entity);
        _logger.LogInformation("Clearing cache for {Type}", typeof(TEntity).FullName);
        _memoryCacher.ClearWith(k => k.StartsWith(typeof(TEntity).FullName!));
    }
    
    public override int UpdateCertainFields
    (
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    )
    {
        var result = base.UpdateCertainFields(query, setPropertyCalls);
        _logger.LogInformation("Clearing cache for {Type}", typeof(TEntity).FullName);
        _memoryCacher.ClearWith(k => k.StartsWith(typeof(TEntity).FullName!));
        return result;
    }

    public override async Task<int> UpdateCertainFieldsAsync
    (
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    )
    {
        var result = await base.UpdateCertainFieldsAsync(query, setPropertyCalls);
        _logger.LogInformation("Clearing cache for {Type}", typeof(TEntity).FullName);
        _memoryCacher.ClearWith(k => k.StartsWith(typeof(TEntity).FullName!));
        return result;
    }

    public override void Delete(TEntity entity)
    {
        base.Delete(entity);
        _logger.LogInformation("Clearing cache for {Type}", typeof(TEntity).FullName);
        _memoryCacher.ClearWith(k => k.StartsWith(typeof(TEntity).FullName!));
    }

    public override int DeleteCertainEntries(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        var result = base.DeleteCertainEntries(query);
        _logger.LogInformation("Clearing cache for {Type}", typeof(TEntity).FullName);
        _memoryCacher.ClearWith(k => k.StartsWith(typeof(TEntity).FullName!));
        return result;
    }
    
    public override async Task<int> DeleteCertainEntriesAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        var result = await base.DeleteCertainEntriesAsync(query);
        _logger.LogInformation("Clearing cache for {Type}", typeof(TEntity).FullName);
        _memoryCacher.ClearWith(k => k.StartsWith(typeof(TEntity).FullName!));
        return result;
    }

    public override bool Exists(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(Exists)}_{query.GetMethodInfo()}";
        _logger.LogInformation("Adding key {key}", cacheKey);
        return _memoryCacher.GetOrCreate(cacheKey, () => base.Exists(query));
    }

    public override async Task<bool> ExistsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        var cacheKey = $"{typeof(TEntity).FullName}_{nameof(Exists)}_{query.GetMethodInfo()}";
        _logger.LogInformation("Adding key {key}", cacheKey);
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
