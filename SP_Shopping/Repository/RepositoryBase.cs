using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SP_Shopping.Data;
using System.Linq.Expressions;

namespace SP_Shopping.Repository;

public class RepositoryBase<TEntity>(ApplicationDbContext context) : IRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext _context = context;

    public virtual IQueryable<TEntity> Get()
    {
        return _context.Set<TEntity>().AsQueryable();
    }

    public virtual List<TEntity> GetAll()
    {
        return _context.Set<TEntity>().ToList();
    }

    public virtual List<TResult> GetAll<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query)
    {
        return query(_context.Set<TEntity>()).ToList();
    }

    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await _context.Set<TEntity>().ToListAsync();
    }

    public virtual async Task<List<TResult>> GetAllAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query)
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

    public virtual TResult? GetSingle<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query) where TResult : class
    {
        return query(_context.Set<TEntity>()).SingleOrDefault();
    }


    public virtual async Task<TResult?> GetSingleAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query) where TResult : class
    {
        return await query(_context.Set<TEntity>()).SingleOrDefaultAsync();
    }


    public virtual void Create(TEntity entity)
    {
        _context.Set<TEntity>().Add(entity);
    }

    public virtual async Task CreateAsync(TEntity entity)
    {
        await _context.Set<TEntity>().AddAsync(entity);
    }

    public virtual void Update(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
    }

    public virtual int UpdateCertainFields
    (
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    )
    {
        return query(_context.Set<TEntity>())
            .ExecuteUpdate(setPropertyCalls);
    }

    public virtual async Task<int> UpdateCertainFieldsAsync
    (
        Func<IQueryable<TEntity>, IQueryable<TEntity>> query,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls
    )
    {
        return await query(_context.Set<TEntity>())
            .ExecuteUpdateAsync(setPropertyCalls);
    }

    public virtual void Delete(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
    }

    public virtual int DeleteCertainEntries(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        return query(_context.Set<TEntity>())
            .ExecuteDelete();
    }

    public virtual async Task<int> DeleteCertainEntriesAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        return await query(_context.Set<TEntity>())
            .ExecuteDeleteAsync();
    }

    public virtual bool Exists(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        return query(_context.Set<TEntity>()).Any();
    }

    public virtual async Task<bool> ExistsAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> query)
    {
        return await query(_context.Set<TEntity>()).AnyAsync();
    }

    public virtual int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public virtual async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

}
