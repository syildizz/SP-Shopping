using SP_Shopping.Models;
using SP_Shopping.Utilities.MessageHandler;

namespace SP_Shopping.Service;
public interface IProductService
{
    List<TResult> GetAll<TResult>();
    List<TResult> GetAll<TResult>(Func<IQueryable<Product>, IQueryable<TResult>> query);
    Task<List<TResult>> GetAllAsync<TResult>();
    Task<List<TResult>> GetAllAsync<TResult>(Func<IQueryable<Product>, IQueryable<TResult>> query);
    TResult? GetSingle<TResult>(Func<IQueryable<Product>, IQueryable<TResult>> query);
    Task<TResult?> GetSingleAsync<TResult>(Func<IQueryable<Product>, IQueryable<TResult>> query);
    bool Exists(Func<IQueryable<Product>, IQueryable<Product>> query);
    Task<bool> ExistsAsync(Func<IQueryable<Product>, IQueryable<Product>> query);
    (bool succeeded, ICollection<Message>? errorMessages) TryCreate(Product product, IFormFile? image);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryCreateAsync(Product product, IFormFile? image);
    (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(Product product, IFormFile? image);
    Task<(bool succeeded, ICollection<Message>? errorMesages)> TryUpdateAsync(Product product, IFormFile? image);
    (bool succeeded, ICollection<Message>? errorMessages) TryDelete(Product product);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(Product product);
    (bool succeeded, ICollection<Message>? errorMessages) TryDeleteCascade(Product product);
}