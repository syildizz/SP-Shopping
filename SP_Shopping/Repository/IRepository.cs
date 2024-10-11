
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
    void Create(TEntity entity);
    Task CreateAsync(TEntity entity);
    void Update(TEntity entity);
    int UpdateCertainFields
    (
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    );
    Task<int> UpdateCertainFieldsAsync
    (
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    );

    void Delete(TEntity entity);
    public int DeleteCertainEntries
    (
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query
    );
    public Task<int> DeleteCertainEntriesAsync
    (
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query
    );
    public bool Exists(Func<IQueryable<TEntity>, IQueryable<TEntity>> query);
    public Task<bool> ExistsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query);

    public int SaveChanges();
    public Task<int> SaveChangesAsync();

}