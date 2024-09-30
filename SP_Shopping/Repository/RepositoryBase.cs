using Microsoft.EntityFrameworkCore;
using SP_Shopping.Data;
using SP_Shopping.Models;

namespace SP_Shopping.Repository;

public class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext _context;
    public RepositoryBase(ApplicationDbContext context)
    {
        _context = context;
    }
    public List<TEntity> GetAll()
    {
        return _context.Set<TEntity>()
            .ToList();
    }
    public async Task<List<TEntity>> GetAllAsync()
    {
        return await _context.Set<TEntity>()
            .ToListAsync();
    }
    public TEntity? GetById(int id)
    {
        return _context.Set<TEntity>()
            .Find(id);
    }
    public async Task<TEntity?> GetByIdAsync(int id)
    {
        return await _context.Set<TEntity>()
            .FindAsync(id);
    }
    public bool Create(TEntity entity)
    {
        _context.Set<TEntity>().Add(entity);
        int numSaved = _context.SaveChanges();
        return (numSaved > 0);
    }
    public async Task<bool> CreateAsync(TEntity entity)
    {
        await _context.Set<TEntity>().AddAsync(entity);
        int numSaved = await _context.SaveChangesAsync();
        return (numSaved > 0);
    }
    public async Task<bool> UpdateAsync(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
        int numSaved = await _context.SaveChangesAsync();
        return (numSaved > 0);
    }
    public bool Update(TEntity entity)
    {

        _context.Set<TEntity>().Update(entity);
        int numSaved = _context.SaveChanges();
        return (numSaved > 0);
    }

    public async Task<bool> DeleteAsync(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
        int numSaved = await _context.SaveChangesAsync();
        return (numSaved > 0);
    }
    public bool Delete(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
        int numSaved = _context.SaveChanges();
        return (numSaved > 0);
    }
}