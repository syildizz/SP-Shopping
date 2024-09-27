using Microsoft.EntityFrameworkCore;
using SP_Shopping.Data;
using SP_Shopping.Models;
using SQLitePCL;

namespace SP_Shopping.Repository
{
    public class ProductRepository
    {
        private readonly DbContext _context;

        public ProductRepository(DbContext context)
        {
            _context = context;
        }
        public Product? GetProductByName(string productName)
        {
            return _context.Set<Product>().Where(p => p.Name == productName).FirstOrDefault();
        }
        public async Task<Product?> GetProductByNameAsync(string productName)
        {
            return await _context.Set<Product>().Where(p => p.Name == productName).FirstOrDefaultAsync();
        }
    }
}
