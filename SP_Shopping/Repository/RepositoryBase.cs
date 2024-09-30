using Microsoft.EntityFrameworkCore;
using SP_Shopping.Data;

namespace SP_Shopping.Repository;

public class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext _context;
    public RepositoryBase(ApplicationDbContext context)
    {
        _context = context;
    }
    public virtual List<TEntity> GetAll()
    {
        return _context.Set<TEntity>()
            .ToList();
    }
    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await _context.Set<TEntity>()
            .ToListAsync();
    }
    public virtual TEntity? GetById(int id)
    {
        return _context.Set<TEntity>()
            .Find(id);
    }
    public virtual async Task<TEntity?> GetByIdAsync(int id)
    {
        return await _context.Set<TEntity>()
            .FindAsync(id);
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
    public virtual async Task<bool> UpdateAsync(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
        int numSaved = await _context.SaveChangesAsync();
        return (numSaved > 0);
    }
    public virtual bool Update(TEntity entity)
    {

        _context.Set<TEntity>().Update(entity);
        int numSaved = _context.SaveChanges();
        return (numSaved > 0);
    }

    public virtual async Task<bool> DeleteAsync(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
        int numSaved = await _context.SaveChangesAsync();
        return (numSaved > 0);
    }
    public virtual bool Delete(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
        int numSaved = _context.SaveChanges();
        return (numSaved > 0);
    }
}