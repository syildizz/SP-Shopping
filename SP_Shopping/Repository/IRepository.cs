
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace SP_Shopping.Repository;

public interface IRepository<TEntity> where TEntity : class
{

    List<TEntity> GetAll();
    List<TEntity> GetAll(Func<IQueryable<TEntity>, IQueryable<TEntity>> query);
    Task<List<TEntity>> GetAllAsync();
    Task<List<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query);
    TEntity? GetByKey(params object?[]? keyValues);
    Task<TEntity?> GetByKeyAsync(params object?[]? keyValues);
    TEntity? GetSingle(Func<IQueryable<TEntity>, IQueryable<TEntity>> query);
    Task<TEntity?> GetSingleAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query);
    bool Create(TEntity entity);
    Task<bool> CreateAsync(TEntity entity);

    bool Update(TEntity entity);
    Task<bool> UpdateAsync(TEntity entity);
    bool UpdateCertainFields
    (
        TEntity entity,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    );
    Task<bool> UpdateCertainFieldsAsync
    (
        TEntity entity,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    );

    bool Delete(TEntity entity);
    Task<bool> DeleteAsync(TEntity entity);
    public bool DeleteCertainEntries
    (
        TEntity entity,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query
    );
    public Task<bool> DeleteCertainEntriesAsync
    (
        TEntity entity,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query
    );

}