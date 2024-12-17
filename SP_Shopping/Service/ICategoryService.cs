using SP_Shopping.Models;
using SP_Shopping.Utilities.MessageHandler;

namespace SP_Shopping.Service;
public interface ICategoryService
{
    List<Category> GetAll();
    List<TResult> GetAll<TResult>(string cacheKey, Func<IQueryable<Category>, IQueryable<TResult>> query);
    Task<List<Category>> GetAllAsync();
    Task<List<TResult>> GetAllAsync<TResult>(string cacheKey, Func<IQueryable<Category>, IQueryable<TResult>> query);
    Category? GetByKey(string cacheKey, params object?[]? keyValues);
    Task<Category?> GetByKeyAsync(string cacheKey, params object?[]? keyValues);
    TResult? GetSingle<TResult>(string cacheKey, Func<IQueryable<Category>, IQueryable<TResult>> query);
    Task<TResult?> GetSingleAsync<TResult>(string cacheKey, Func<IQueryable<Category>, IQueryable<TResult>> query);
    bool Exists(Func<IQueryable<Category>, IQueryable<Category>> query);
    Task<bool> ExistsAsync(Func<IQueryable<Category>, IQueryable<Category>> query);
    (bool succeeded, ICollection<Message>? errorMessages) TryCreate(Category category);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryCreateAsync(Category category);
    (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(Category category);
    Task<(bool succeeded, ICollection<Message>? errorMesages)> TryUpdateAsync(Category category);
    (bool succeeded, ICollection<Message>? errorMessages) TryDelete(Category category);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(Category category);
}