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
        return _context.Set<TEntity>().ToList();
    }

    public virtual List<TEntity> GetAll(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        return query(_context.Set<TEntity>()).ToList();
    }

    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await _context.Set<TEntity>().ToListAsync();
    }

    public virtual async Task<List<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        return await query(_context.Set<TEntity>()).ToListAsync();
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

    public virtual TEntity? GetSingle(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        return query(_context.Set<TEntity>()).FirstOrDefault();
    }


    public virtual async Task<TEntity?> GetSingleAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        return await query(_context.Set<TEntity>()).FirstOrDefaultAsync();
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
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    )
    {
        query(_context.Set<TEntity>())
            .ExecuteUpdate(setPropertyCalls);
        int numSaved = _context.SaveChanges();
        return (numSaved > 0);
    }

    public virtual async Task<bool> UpdateCertainFieldsAsync
    (
        TEntity entity,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    )
    {
        await query(_context.Set<TEntity>())
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

    public virtual bool DeleteCertainEntries
    (
        TEntity entity,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query
    )
    {
        query(_context.Set<TEntity>())
            .ExecuteDelete();
        int numSaved = _context.SaveChanges();
        return (numSaved > 0);
    }

    public virtual async Task<bool> DeleteCertainEntriesAsync
    (
        TEntity entity,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query
    )
    {
        await query(_context.Set<TEntity>())
            .ExecuteDeleteAsync();
        int numSaved = await _context.SaveChangesAsync();
        return (numSaved > 0);
    }

}
