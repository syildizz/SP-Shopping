
namespace SP_Shopping.Service;

public interface IServices
{
    CartItemService CartItem { get; }
    CategoryService Category { get; }
    ProductService Product { get; }
    UserService User { get; }

    bool DoInTransaction(Func<bool> action);
    Task<bool> DoInTransactionAsync(Func<Task<bool>> action);
}