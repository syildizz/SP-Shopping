using SP_Shopping.Models;
using SP_Shopping.Utilities.MessageHandler;

namespace SP_Shopping.Service;
public interface ICartItemService
{
    bool Exists(Func<IQueryable<CartItem>, IQueryable<CartItem>> query);
    Task<bool> ExistsAsync(Func<IQueryable<CartItem>, IQueryable<CartItem>> query);
    List<CartItem> GetAll();
    List<TResult> GetAll<TResult>(Func<IQueryable<CartItem>, IQueryable<TResult>> query);
    Task<List<CartItem>> GetAllAsync();
    Task<List<TResult>> GetAllAsync<TResult>(Func<IQueryable<CartItem>, IQueryable<TResult>> query);
    CartItem? GetByKey(params object?[]? keyValues);
    Task<CartItem?> GetByKeyAsync(params object?[]? keyValues);
    TResult? GetSingle<TResult>(Func<IQueryable<CartItem>, IQueryable<TResult>> query);
    Task<TResult?> GetSingleAsync<TResult>(Func<IQueryable<CartItem>, IQueryable<TResult>> query);
    (bool succeeded, ICollection<Message>? errorMessages) TryCreate(CartItem cartItem);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryCreateAsync(CartItem cartItem);
    (bool succeeded, ICollection<Message>? errorMessages) TryDelete(CartItem cartItem);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(CartItem cartItem);
    (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(CartItem cartItem);
    Task<(bool succeeded, ICollection<Message>? errorMesages)> TryUpdateAsync(CartItem cartItem);
}