using Microsoft.EntityFrameworkCore;
using SP_Shopping.Data;
using SP_Shopping.Models;

namespace SP_Shopping.Repository;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public Product? GetById(int productId)
    {
        return _context.Set<Product>()
            .Include(p => p.Category)
            .Where(p => p.Id == productId)
            .FirstOrDefault();
    }
    public async Task<Product?> GetByIdAsync(int productId)
    {
        return await _context.Set<Product>()
            .Include(p => p.Category)
            .Where(p => p.Id == productId)
            .FirstOrDefaultAsync();
    }
    public Product? GetByName(string productName)
    {
        return _context.Set<Product>()
            .Include(p => p.Category)
            .Where(p => p.Name == productName)
            .FirstOrDefault();
    }
    public async Task<Product?> GetByNameAsync(string productName)
    {
        return await _context.Set<Product>()
            .Include(p => p.Category)
            .Where(p => p.Name == productName)
            .FirstOrDefaultAsync();
    }
    public List<Product> GetAll()
    {
        return _context.Set<Product>()
            .Include(p => p.Category)
            .ToList();
    }
    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Set<Product>()
            .Include(p => p.Category)
            .ToListAsync();
    }
    public bool Create(Product product)
    {
        product.InsertionDate = DateTime.Now;
        _context.Add(product);
        int numSaved = _context.SaveChanges();
        return (numSaved > 0);
    }
    public async Task<bool> CreateAsync(Product product)
    {
        product.InsertionDate = DateTime.Now;
        await _context.AddAsync(product);
        int numSaved = await _context.SaveChangesAsync();
        return (numSaved > 0);
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        product.ModificationDate = DateTime.Now;
        await _context.Products
            .Where(p => p.Id == product.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Name, product.Name)
                .SetProperty(b => b.Price, product.Price)
                .SetProperty(b => b.CategoryId, product.CategoryId)
                .SetProperty(b => b.ModificationDate, product.ModificationDate)
            );
        //_context.Update(product);
        int numSaved = await _context.SaveChangesAsync();
        return (numSaved > 0);
    }
    public bool Update(Product product)
    {
        product.ModificationDate = DateTime.Now;
        _context.Products
            .Where(p => p.Id == product.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Name, product.Name)
                .SetProperty(b => b.Price, product.Price)
                .SetProperty(b => b.CategoryId, product.CategoryId)
                .SetProperty(b => b.ModificationDate, product.ModificationDate)
            );
        //_context.Update(product);
        int numSaved = _context.SaveChanges();
        return (numSaved > 0);
    }

    public async Task<bool> DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        int numSaved = await _context.SaveChangesAsync();
        return (numSaved > 0);
    }
    public bool Delete(Product product)
    {
        _context.Products.Remove(product);
        int numSaved = _context.SaveChanges();
        return (numSaved > 0);
    }
}
