using SP_Shopping.Models;

namespace SP_Shopping.Repository;

public interface IProductRepository : IRepository<Product>
{
    Product? GetByName(string productName);
    Task<Product?> GetByNameAsync(string productName);
}