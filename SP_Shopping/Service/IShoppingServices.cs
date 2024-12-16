
namespace SP_Shopping.Service;

public interface IShoppingServices
{
    ICartItemService CartItem { get; }
    ICategoryService Category { get; }
    IProductService Product { get; }
    IUserService User { get; }
    IRoleService Role { get; }
    bool DoInTransaction(Func<bool> action);
    Task<bool> DoInTransactionAsync(Func<Task<bool>> action);
}