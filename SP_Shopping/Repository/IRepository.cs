
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace SP_Shopping.Repository;

public interface IRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> Get();
    List<TEntity> GetAll();
    List<TResult> GetAll<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query);
    Task<List<TEntity>> GetAllAsync();
    Task<List<TResult>> GetAllAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query);
    TEntity? GetByKey(params object?[]? keyValues);
    Task<TEntity?> GetByKeyAsync(params object?[]? keyValues);
    TResult? GetSingle<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query);
    Task<TResult?> GetSingleAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query);
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
    bool DoInTransaction(Func<bool> action);
    Task<bool> DoInTransactionAsync(Func<Task<bool>> action);
}