using Microsoft.AspNetCore.Identity;
using SP_Shopping.Data;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
namespace SP_Shopping.Service;

public class ShoppingServices : IShoppingServices
{
    public IProductService Product { get; }
    public ICategoryService Category { get; }
    public ICartItemService CartItem { get; }
    public IUserService User { get; }
    public IRoleService Role { get; }

    private readonly ApplicationDbContext _context;

    public ShoppingServices
    (
        ApplicationDbContext context,
        IImageHandlerDefaulting<ProductImageKey> productImageHandler,
        IImageHandlerDefaulting<UserProfileImageKey> profileImageHandler,
        IMemoryCacher<string> memoryCacher,
        ILogger<RepositoryBaseCaching<Category>> memoryCacherLogger,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager
    )
    {
        _context = context;

        IRepository<Product> productRepository = new RepositoryBase<Product>(_context);
        Product = new ProductService(productRepository, productImageHandler);

        IRepositoryCaching<Category> categoryRepository = new RepositoryBaseCaching<Category>(_context, memoryCacher, memoryCacherLogger);
        Category = new CategoryService(categoryRepository, productRepository, Product);

        IRepository<CartItem> cartItemRepository = new RepositoryBase<CartItem>(_context);
        CartItem = new CartItemService(cartItemRepository);

        IRepository<ApplicationUser> userRepository = new RepositoryBase<ApplicationUser>(_context);
        User = new UserService(userRepository, productRepository, userManager, profileImageHandler, Product);

        IRepository<ApplicationRole> roleRepository = new RepositoryBase<ApplicationRole>(_context);
        Role = new RoleService(roleRepository, roleManager);
    }

    public bool DoInTransaction(Func<bool> action)
    {
        using var transact = _context.Database.BeginTransaction();
        try
        {
            var succeeded = action();
            if (succeeded)
            {
                transact.Commit();
                return true;
            }
            else
            {
                transact.Rollback();
                return false;
            }
        }
        catch
        {
            transact.Rollback();
            throw;
        }
    }

    public async Task<bool> DoInTransactionAsync(Func<Task<bool>> action)
    {
        using var transact = await _context.Database.BeginTransactionAsync();
        try
        {
            var succeeded = await action();
            if (succeeded)
            {
                await transact.CommitAsync();
                return true;
            }
            else
            {
                await transact.RollbackAsync();
                return false;
            }
        }
        catch
        {
            await transact.RollbackAsync();
            throw;
        }
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

}
