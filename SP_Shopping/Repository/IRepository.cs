
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace SP_Shopping.Repository;

public interface IRepository<TEntity> where TEntity : class
{

    List<TEntity> GetAll();
    List<TEntity> GetAll(List<Expression<Func<TEntity, object>>> includedModel);
    Task<List<TEntity>> GetAllAsync();
    Task<List<TEntity>> GetAllAsync(List<Expression<Func<TEntity, object>>> includedModel);
    TEntity? GetByKey(params object?[]? keyValues);
    Task<TEntity?> GetByKeyAsync(params object?[]? keyValues);
    public TEntity? GetByPredicate(Expression<Func<TEntity, bool>> predicate);
    public TEntity? GetByPredicate
    (
        Expression<Func<TEntity, bool>> predicate,
        List<Expression<Func<TEntity, object>>> includedModel
    );
    public Task<TEntity?> GetByPredicateAsync(Expression<Func<TEntity, bool>> predicate);
    public Task<TEntity?> GetByPredicateAsync
    (
        Expression<Func<TEntity, bool>> predicate,
        List<Expression<Func<TEntity, object>>> includedModel
    );
    public IEnumerable<TEntity?> GetByPredicateList(Expression<Func<TEntity, bool>> predicate);
    public IEnumerable<TEntity?> GetByPredicateList
    (
        Expression<Func<TEntity, bool>> predicate,
        List<Expression<Func<TEntity, object>>> includedModel
    );
    public Task<IEnumerable<TEntity?>> GetByPredicateListAsync(Expression<Func<TEntity, bool>> predicate);
    public Task<IEnumerable<TEntity?>> GetByPredicateListAsync
    (
        Expression<Func<TEntity, bool>> predicate,
        List<Expression<Func<TEntity, object>>> includedModel
    );

    bool Create(TEntity entity);
    Task<bool> CreateAsync(TEntity entity);

    bool Update(TEntity entity);
    Task<bool> UpdateAsync(TEntity entity);
    bool UpdateCertainFields
    (
        TEntity entity,
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    );
    Task<bool> UpdateCertainFieldsAsync
    (
        TEntity entity,
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    );

    bool Delete(TEntity entity);
    Task<bool> DeleteAsync(TEntity entity);
    public bool DeleteByPredicate
    (
        TEntity entity,
        Expression<Func<TEntity, bool>> predicate
    );
    public Task<bool> DeleteByPredicateAsync
    (
        TEntity entity,
        Expression<Func<TEntity, bool>> predicate
    );

}