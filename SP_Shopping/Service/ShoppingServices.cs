using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SP_Shopping.Data;
using SP_Shopping.Hubs;
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
        RoleManager<ApplicationRole> roleManager,
        IMapper mapper,
        IHubContext<ProductHub, IProductHubClient> productHubContext
    )
    {
        _context = context;

        IRepository<Product> productRepository = new RepositoryBase<Product>(_context);
        Product = new ProductService(productRepository, productImageHandler, mapper, productHubContext);

        IRepositoryCaching<Category> categoryRepository = new RepositoryBaseCaching<Category>(_context, memoryCacher, memoryCacherLogger);
        Category = new CategoryService(categoryRepository, productRepository, Product, mapper);

        IRepository<CartItem> cartItemRepository = new RepositoryBase<CartItem>(_context);
        CartItem = new CartItemService(cartItemRepository, mapper);

        IRepository<ApplicationUser> userRepository = new RepositoryBase<ApplicationUser>(_context);
        User = new UserService(userRepository, productRepository, userManager, profileImageHandler, Product, mapper);

        IRepository<ApplicationRole> roleRepository = new RepositoryBase<ApplicationRole>(_context);
        Role = new RoleService(roleRepository, roleManager, mapper);
    }

}
