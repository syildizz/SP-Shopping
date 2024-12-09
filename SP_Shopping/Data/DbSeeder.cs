using Microsoft.AspNetCore.Identity;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Service;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using System.Text.Json;

namespace SP_Shopping.Data;

public record ProductJson
    (string Name, decimal Price, string Description);

public record CategoryJson
    (string Name);

public record UserJson
    (string UserName, string Email, string PhoneNumber, string Description);

public record CartItemJson
    (int Count);

public class DbSeeder : IDisposable
{
    private readonly WebApplication _app;
    private readonly IServiceScope _scope;

    private readonly ILogger<DbSeeder> _logger;
    
    private readonly IRepository<Product> _productRepository;
    private readonly IImageHandlerDefaulting<ProductImageKey> _productImageHandler;
    private readonly ProductService _productService;

    private readonly IRepositoryCaching<Category> _categoryRepository;
    private readonly CategoryService _categoryService;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IImageHandlerDefaulting<UserProfileImageKey> _profileImageHandler;
    private readonly UserService _userService;

    private readonly RoleManager<IdentityRole> _roleManager;

    private readonly IRepository<CartItem> _cartItemRepository;
    private readonly CartItemService _cartItemService;

    private readonly ApplicationDbContext _context;

    private readonly string _seedFolder;

    private readonly Random _random = new Random();

    public DbSeeder(WebApplication app, string seedFolder = "MOCK_DATA")
    {
        _app = app;
        _seedFolder = seedFolder;
        
        _scope = _app.Services.CreateAsyncScope();

        _logger = _scope.ServiceProvider.GetRequiredService<ILogger<DbSeeder>>();

        _productRepository = _scope.ServiceProvider.GetRequiredService<IRepository<Product>>();
        _productImageHandler = _scope.ServiceProvider.GetRequiredService<IImageHandlerDefaulting<ProductImageKey>>();
        _productService = new ProductService(_productRepository, _productImageHandler);

        _categoryRepository = _scope.ServiceProvider.GetRequiredService<IRepositoryCaching<Category>>();
        _categoryService = new CategoryService(_categoryRepository, _productRepository, _productService);

        _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        _userRepository = _scope.ServiceProvider.GetRequiredService<IRepository<ApplicationUser>>();
        _profileImageHandler = _scope.ServiceProvider.GetRequiredService<IImageHandlerDefaulting<UserProfileImageKey>>();
        _userService = new UserService(_userRepository, _productRepository, _userManager, _profileImageHandler, _productService);

        _roleManager = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        _cartItemRepository = _scope.ServiceProvider.GetRequiredService<IRepository<CartItem>>();
        _cartItemService = new CartItemService(_cartItemRepository);

        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public void Dispose()
    {
        _scope.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task Seed()
    {
        List<CategoryJson>? categorySeedData;
        List<ProductJson>? productSeedData;
        List<UserJson>? userSeedData;
        List<CartItemJson>? cartItemSeedData;
        using (var productJsonFile = File.OpenRead(Path.Combine(_seedFolder, "Product.json")))
        using (var categoryJsonFile = File.OpenRead(Path.Combine(_seedFolder, "Category.json")))
        using (var userJsonFile = File.OpenRead(Path.Combine(_seedFolder, "User.json")))
        using (var cartItemJsonFile = File.OpenRead(Path.Combine(_seedFolder, "CartItem.json")))
        {
            categorySeedData = await JsonSerializer.DeserializeAsync<List<CategoryJson>>(categoryJsonFile);
            productSeedData = await JsonSerializer.DeserializeAsync<List<ProductJson>>(productJsonFile);
            userSeedData = await JsonSerializer.DeserializeAsync<List<UserJson>>(userJsonFile);
            cartItemSeedData = await JsonSerializer.DeserializeAsync<List<CartItemJson>>(cartItemJsonFile);
        }

        if (
               categorySeedData is null 
            || productSeedData is null 
            || userSeedData is null 
            || cartItemSeedData is null
        )
        {
            return;
        }

        List<Stream?> imageStreams = [];
        try
        {
            foreach (var file in Directory.GetFiles(Path.Combine(_seedFolder, "images")))
            {
                if (file.Contains(".gitkeep")) continue;
                imageStreams.Add(File.OpenRead(file));
            }
        }
        catch (DirectoryNotFoundException)
        {
            _logger.LogWarning("Directory is empty, no images will be used");
        }
        imageStreams.Add(null);

        List<Category> categories = categorySeedData.Select(c => new Category { Name = c.Name }).DistinctBy(c => c.Name).ToList();

        foreach (var category in categories)
        {
            var (succeeded, errmsgs) = await _categoryService.TryCreateAsync(category);
            if (!succeeded)
            {
                _logger.LogError("Failed to seed category in database due to {ErrMsgs}", errmsgs);
                break;
            }
        }

        await AddRoles();

        List<ApplicationUser> users = userSeedData
            .Select(u =>
            {
                var user = new ApplicationUser();
                user.UserName = u.UserName;
                user.Email = u.Email;
                user.PhoneNumber = u.PhoneNumber;
                user.Description = u.Description;
                return user;
            })
            .DistinctBy(u => u.UserName)
            .ToList();

        foreach (var user in users)
        {
            var succeeded = await _userManager.CreateAsync(user, "123456");
            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;
            if (!succeeded.Succeeded)
            {
                _logger.LogError("Failed to seed user in database due to {ErrMsgs}", succeeded.Errors);
                break;
            }
            Stream? chosenImage = imageStreams[_random.Next(imageStreams.Count)];
            if (chosenImage is not null)
            {
                if (!await _profileImageHandler.SetImageAsync(new(user.Id), chosenImage))
                {
                    _logger.LogWarning("Failed to seed profile picture of user");
                }
                chosenImage.Position = 0;
            }
        }

        await MakeAdminUser(imageStreams);

        List<Product> products = productSeedData
            .Select(p => new Product
            {
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                CategoryId = categories[_random.Next(categories.Count)].Id,
                SubmitterId = users[_random.Next(users.Count)].Id,
            })
            .DistinctBy(p => p.Name)
            .ToList();

        foreach (var product in products)
        {
            Stream? chosenImage = imageStreams[_random.Next(imageStreams.Count)];
            FormFile? ff = chosenImage is null ? null : new FormFile(chosenImage, 0, chosenImage.Length, "idk", "idk");
            var (succeeded, errmsgs) = await _productService.TryCreateAsync(product, ff);
            if (ff is not null)
            {
                chosenImage!.Position = 0;
            }
            if (!succeeded)
            {
                _logger.LogError("Failed to seed product in database due to {ErrMsgs}", errmsgs);
                continue;
            }
        }

        List<CartItem> cartItems = cartItemSeedData
            .Select(c => new CartItem
            {
                UserId = users[_random.Next(users.Count)].Id,
                ProductId = products[_random.Next(products.Count)].Id,
                Count = c.Count
            })
            .DistinctBy(c => new { c.ProductId, c.UserId })
            .ToList();

        foreach (var cartItem in cartItems)
        {
            var (succeed, errmsgs) = await _cartItemService.TryCreateAsync(cartItem);
            if (!succeed)
            {
                _logger.LogError("Failed to seed cartItem in database due to {Errmsgs}", errmsgs);
                continue;
            }
        }

        foreach (var s in imageStreams)
        {
            if (s is not null) await s.DisposeAsync();
        }

    }

    public async Task<bool> AddRoles()
    {
        var roles = GetRoleNames();
        if (roles is null) return false;
        return await AddRoles(roles);
    }

    // https://stackoverflow.com/a/73410638
    public async Task<bool> AddRoles(List<string> roles)
    {
        foreach (string role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var succeed = await _roleManager.CreateAsync(new IdentityRole(role));
                if (!succeed.Succeeded)
                {
                    _logger.LogError("Failed to create role for role \"{Role}\" due to {ErrMsgs}", role, succeed.Errors);
                    return false;
                }
            }
        }
        return true;
    }

    private List<string>? GetRoleNames()
    {
        const string roleSectionName = "Roles";
        List<string> roles = _app.Configuration.GetSection(roleSectionName).Get<List<string>>() 
            ?? throw new Exception($"Section {roleSectionName} does not exist in config");
        return roles;
    }


    private async Task MakeAdminUser(List<Stream?> imageStreams)
    {

        var admin = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@admin.com",
            PhoneNumber = "111111",
            Description = "Admin user"
        };

        var succeeded = await _userManager.CreateAsync(admin, "123456");
        admin.EmailConfirmed = true;
        admin.PhoneNumberConfirmed = true;
        if (!succeeded.Succeeded)
        {
            _logger.LogError("Failed to seed user in database due to {ErrMsgs}", succeeded.Errors);
            return;
        }

        succeeded = await _userManager.AddToRoleAsync(admin, "Admin");
        if (!succeeded.Succeeded)
        {
            _logger.LogError("Failed to set admin as admin due to {ErrMsgs}", succeeded.Errors);
        }

        Stream? chosenImage = imageStreams[_random.Next(imageStreams.Count)];
        if (chosenImage is not null)
        {
            if (!await _profileImageHandler.SetImageAsync(new(admin.Id), chosenImage))
            {
                _logger.LogWarning("Failed to seed image for admin");
            }
            chosenImage.Position = 0;
        }


    }




}
