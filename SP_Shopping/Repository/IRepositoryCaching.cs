
namespace SP_Shopping.Repository;

public interface IRepositoryCaching<TEntity>: IRepository<TEntity> where TEntity : class
{
    bool Exists(string cacheKey, Func<IQueryable<TEntity>, IQueryable<TEntity>> query);
    Task<bool> ExistsAsync(string cacheKey, Func<IQueryable<TEntity>, IQueryable<TEntity>> query);
    List<TResult> GetAll<TResult>(string cacheKey, Func<IQueryable<TEntity>, IQueryable<TResult>> query);
    Task<List<TResult>> GetAllAsync<TResult>(string cacheKey, Func<IQueryable<TEntity>, IQueryable<TResult>> query);
    TEntity? GetByKey(string cacheKey, params object?[]? keyValues);
    Task<TEntity?> GetByKeyAsync(string cacheKey, params object?[]? keyValues);
    TResult? GetSingle<TResult>(string cacheKey, Func<IQueryable<TEntity>, IQueryable<TResult>> query);
    Task<TResult?> GetSingleAsync<TResult>(string cacheKey, Func<IQueryable<TEntity>, IQueryable<TResult>> query);
}