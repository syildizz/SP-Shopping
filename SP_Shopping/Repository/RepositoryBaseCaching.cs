using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Caching.Memory;
using SP_Shopping.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace SP_Shopping.Repository;

public class RepositoryBaseCaching<TEntity>
(
    ApplicationDbContext context,
	IMemoryCacher<string> memoryCacher,
	ILogger<RepositoryBaseCaching<TEntity>> logger
) : 
    RepositoryBase<TEntity>(context),
	IRepositoryCaching<TEntity> where TEntity : class
{
    private readonly IMemoryCacher<string> _memoryCacher = memoryCacher;
    private readonly ILogger<RepositoryBaseCaching<TEntity>> _logger = logger;

    public override List<TEntity> GetAll()
    {
        var _cacheKey = $"{typeof(TEntity).FullName}_All";
        _logger.LogInformation("Adding key {key}", _cacheKey);
        return _memoryCacher.GetOrCreate(_cacheKey, base.GetAll);
    }

    public List<TResult> GetAll<TResult>(string cacheKey, Func<IQueryable<TEntity>, IQueryable<TResult>> query)
    {
        var _cacheKey = $"{typeof(TEntity).FullName}_{cacheKey}";
        _logger.LogInformation("Adding key {key}", _cacheKey);
        return _memoryCacher.GetOrCreate(_cacheKey, () => base.GetAll(query));
    }

    public async Task<List<TEntity>> GetAllAsync(string cacheKey)
    {
        var _cacheKey = $"{typeof(TEntity).FullName}_All";
        _logger.LogInformation("Adding key {key}", _cacheKey);
        return await _memoryCacher.GetOrCreate(_cacheKey, async () => await base.GetAllAsync());
    }

    public async Task<List<TResult>> GetAllAsync<TResult>(string cacheKey, Func<IQueryable<TEntity>, IQueryable<TResult>> query)
    {
        var _cacheKey = $"{typeof(TEntity).FullName}_{cacheKey}";
        _logger.LogInformation("Adding key {key}", _cacheKey);
        return await _memoryCacher.GetOrCreate(_cacheKey, async () => await base.GetAllAsync(query));
    }

    public TEntity? GetByKey(string cacheKey, params object?[]? keyValues)
    {
        var _cacheKey = $"{typeof(TEntity).FullName}_{cacheKey}";
        _logger.LogInformation("Adding key {key}", _cacheKey);
        return _memoryCacher.GetOrCreate(_cacheKey, () => base.GetByKey(keyValues));
    }

    public async Task<TEntity?> GetByKeyAsync(string cacheKey, params object?[]? keyValues)
    {
        var _cacheKey = $"{typeof(TEntity).FullName}_{cacheKey}";
        _logger.LogInformation("Adding key {key}", _cacheKey);
        return await _memoryCacher.GetOrCreate(_cacheKey, async () => await base.GetByKeyAsync(keyValues));
    }

    public TResult? GetSingle<TResult>(string cacheKey, Func<IQueryable<TEntity>, IQueryable<TResult>> query)
    {
        var _cacheKey = $"{typeof(TEntity).FullName}_{cacheKey}";
        _logger.LogInformation("Adding key {key}", _cacheKey);
        return _memoryCacher.GetOrCreate(_cacheKey, () => base.GetSingle(query));
    }
    public async Task<TResult?> GetSingleAsync<TResult>(string cacheKey, Func<IQueryable<TEntity>, IQueryable<TResult>> query)
    {
        var _cacheKey = $"{typeof(TEntity).FullName}_{cacheKey}";
        _logger.LogInformation("Adding key {key}", _cacheKey);
        return await _memoryCacher.GetOrCreate(_cacheKey, () => base.GetSingleAsync(query));
    }

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

    public bool Exists(string cacheKey, Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        var _cacheKey = $"{typeof(TEntity).FullName}_{cacheKey}";
        _logger.LogInformation("Adding key {key}", _cacheKey);
        return _memoryCacher.GetOrCreate(_cacheKey, () => base.Exists(query));
    }

    public async Task<bool> ExistsAsync(string cacheKey, Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        var _cacheKey = $"{typeof(TEntity).FullName}_{cacheKey}";
        _logger.LogInformation("Adding key {key}", _cacheKey);
        return await _memoryCacher.GetOrCreate(_cacheKey, () => base.ExistsAsync(query));
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
