using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Service;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using SP_Shopping.Utilities;
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

public class DbSeeder
{
    private readonly ModelBuilder _modelBuilder;
    private readonly string _seedFolder;

    public DbSeeder(string seedFolder = "MOCK_DATA")
    {
        _seedFolder = seedFolder;
    }

    public async Task Seed(WebApplication app)
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

        using var scope = app.Services.CreateAsyncScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbSeeder>>();

        var productRepository = scope.ServiceProvider.GetRequiredService<IRepository<Product>>();
        var productImageHandler = scope.ServiceProvider.GetRequiredService<IImageHandlerDefaulting<ProductImageKey>>();
        var productService = new ProductService(productRepository, productImageHandler);

        var categoryRepository = scope.ServiceProvider.GetRequiredService<IRepositoryCaching<Category>>();
        var categoryService = new CategoryService(categoryRepository, productRepository, productService);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IRepository<ApplicationUser>>();
        var profileImageHandler = scope.ServiceProvider.GetRequiredService<IImageHandlerDefaulting<UserProfileImageKey>>();
        var messageHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler>();
        var userService = new UserService(userRepository, productRepository, userManager, profileImageHandler, messageHandler, productService);

        var cartItemRepository = scope.ServiceProvider.GetRequiredService<IRepository<CartItem>>();
        var cartItemService = new CartItemService(cartItemRepository);

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var random = new Random();

        List<Category> categories = categorySeedData.Select(c => new Category { Name = c.Name }).DistinctBy(c => c.Name).ToList();

        foreach (var category in categories)
        {
            var (succeeded, _) = await categoryService.TryCreateAsync(category);
            if (!succeeded)
            {
                logger.LogError("Failed to seed category in database");
                break;
            }
        }

        var passwordHasher = new PasswordHasher<ApplicationUser>();
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
            var succeeded = await userManager.CreateAsync(user, "123456");
            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;
            if (!succeeded.Succeeded)
            {
                logger.LogError("Failed to seed user in database");
                break;
            }
        }

        List<Product> products = productSeedData
            .Select(p => new Product
            {
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                CategoryId = categories[random.Next(categories.Count)].Id,
                SubmitterId = users[random.Next(users.Count)].Id,
            })
            .DistinctBy(p => p.Name)
            .ToList();

        foreach (var product in products)
        {
            var (succeeded, errmsgs) = await productService.TryCreateAsync(product, null);
            if (!succeeded)
            {
                logger.LogError("Failed to seed product in database");
                continue;
            }
        }

        List<CartItem> cartItems = cartItemSeedData
            .Select(c => new CartItem
            {
                UserId = users[random.Next(users.Count)].Id,
                ProductId = products[random.Next(products.Count)].Id,
                Count = c.Count
            })
            .DistinctBy(c => new { c.ProductId, c.UserId })
            .ToList();

        foreach (var cartItem in cartItems)
        {
            var (succeed, errmsgs) = await cartItemService.TryCreateAsync(cartItem);
            if (!succeed)
            {
                logger.LogError("Failed to seed cartItem in database");
                continue;
            }
        }
    }

}
