using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SP_Shopping.Data;
using System.Linq.Expressions;

namespace SP_Shopping.Repository;

public class RepositoryBase<TEntity>(ApplicationDbContext context) : IRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext _context = context;

    public virtual List<TEntity> GetAll()
    {
        return _context.Set<TEntity>()
            .ToList();
    }

    public List<TEntity> GetAll
    (
        List<Expression<Func<TEntity, object>>> includedModel
    )
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        foreach (var include in includedModel)
        {
            query = query.Include(include);
        }
        return query.ToList();
    }

    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await _context.Set<TEntity>()
            .ToListAsync();
    }

    public virtual async Task<List<TEntity>> GetAllAsync
    (
        List<Expression<Func<TEntity, object>>> includedModel
    )
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        foreach (var include in includedModel)
        {
            query = query.Include(include);
        }
        return await query.ToListAsync();
    }

    public virtual TEntity? GetByKey(params object?[]? keyValues)
    {
        return _context.Set<TEntity>()
            .Find(keyValues);
    }

    public virtual async Task<TEntity?> GetByKeyAsync(params object?[]? keyValues)
    {
        return await _context.Set<TEntity>()
            .FindAsync(keyValues);
    }

    public virtual TEntity? GetByPredicate(Expression<Func<TEntity, bool>> predicate)
    {
        return _context.Set<TEntity>()
            .Where(predicate)
            .FirstOrDefault();
    }

    public TEntity? GetByPredicate
    (
        Expression<Func<TEntity, bool>> predicate,
        List<Expression<Func<TEntity, object>>> includedModel
    )
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        foreach (var include in includedModel)
        {
            query = query.Include(include);
        }
        query = query.Where(predicate);
        return query.FirstOrDefault();
    }

    public virtual async Task<TEntity?> GetByPredicateAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _context.Set<TEntity>()
            .Where(predicate)
            .FirstOrDefaultAsync();
    }

    public virtual async Task<TEntity?> GetByPredicateAsync
    (
        Expression<Func<TEntity, bool>> predicate,
        List<Expression<Func<TEntity, object>>> includedModel
    )
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        foreach (var include in includedModel)
        {
            query = query.Include(include);
        }
        query = query.Where(predicate);
        return await query.FirstOrDefaultAsync();
    }

    public virtual IEnumerable<TEntity?> GetByPredicateList(Expression<Func<TEntity, bool>> predicate)
    {
        return _context.Set<TEntity>()
            .Where(predicate)
            .ToList();
    }

    public virtual IEnumerable<TEntity?> GetByPredicateList
    (
        Expression<Func<TEntity, bool>> predicate,
        List<Expression<Func<TEntity, object>>> includedModel
    )
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        foreach (var include in includedModel)
        {
            query = query.Include(include);
        }
        query = query.Where(predicate);
        return query.ToList();
    }

    public virtual async Task<IEnumerable<TEntity?>> GetByPredicateListAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _context.Set<TEntity>()
            .Where(predicate)
            .ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity?>> GetByPredicateListAsync
    (
        Expression<Func<TEntity, bool>> predicate,
        List<Expression<Func<TEntity, object>>> includedModel
    )
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        foreach (var include in includedModel)
        {
            query = query.Include(include);
        }
        query = query.Where(predicate);
        return await query.ToListAsync();
    }

    public virtual bool Create(TEntity entity)
    {
        _context.Set<TEntity>().Add(entity);
        int numSaved = _context.SaveChanges();
        return (numSaved > 0);
    }

    public virtual async Task<bool> CreateAsync(TEntity entity)
    {
        await _context.Set<TEntity>().AddAsync(entity);
        int numSaved = await _context.SaveChangesAsync();
        return (numSaved > 0);
    }

    public virtual bool Update(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
        int numSaved = _context.SaveChanges();
        return (numSaved > 0);
    }

    public virtual async Task<bool> UpdateAsync(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
        int numSaved = await _context.SaveChangesAsync();
        return (numSaved > 0);
    }
    public virtual bool UpdateCertainFields
    (
        TEntity entity,
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    )
    {
        _context.Set<TEntity>()
            .Where(predicate)
            .ExecuteUpdate(setPropertyCalls);
        int numSaved = _context.SaveChanges();
        return (numSaved > 0);
    }

    public virtual async Task<bool> UpdateCertainFieldsAsync
    (
        TEntity entity,
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    )
    {
        await _context.Set<TEntity>()
            .Where(predicate)
            .ExecuteUpdateAsync(setPropertyCalls);
        int numSaved = await _context.SaveChangesAsync();
        return (numSaved > 0);
    }

    public virtual bool Delete(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
        int numSaved = _context.SaveChanges();
        return (numSaved > 0);
    }

    public virtual async Task<bool> DeleteAsync(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
        int numSaved = await _context.SaveChangesAsync();
        return (numSaved > 0);
    }

    public virtual bool DeleteByPredicate
    (
        TEntity entity,
        Expression<Func<TEntity, bool>> predicate
    )
    {
        _context.Set<TEntity>()
            .Where(predicate)
            .ExecuteDelete();
        int numSaved = _context.SaveChanges();
        return (numSaved > 0);
    }

    public virtual async Task<bool> DeleteByPredicateAsync
    (
        TEntity entity,
        Expression<Func<TEntity, bool>> predicate
    )
    {
        await _context.Set<TEntity>()
            .Where(predicate)
            .ExecuteDeleteAsync();
        int numSaved = await _context.SaveChangesAsync();
        return (numSaved > 0);
    }

}
