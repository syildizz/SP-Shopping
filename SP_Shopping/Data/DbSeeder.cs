using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using SP_Shopping.Models;
using SP_Shopping.Service;
using SP_Shopping.ServiceDtos.Category;
using SP_Shopping.ServiceDtos.Product;
using SP_Shopping.ServiceDtos.User;
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

public class DbSeeder : IDisposable
{
    private readonly WebApplication _app;
    private readonly IServiceScope _scope;

    private readonly ILogger<DbSeeder> _logger;

    private readonly IShoppingServices _shoppingServices;
    
    private readonly string _seedFolder;

    private readonly IMapper _mapper;

    private readonly Random _random = new();

    public DbSeeder(WebApplication app, string seedFolder = "MOCK_DATA")
    {
        _app = app;
        _seedFolder = seedFolder;
        
        _scope = _app.Services.CreateAsyncScope();

        _logger = _scope.ServiceProvider.GetRequiredService<ILogger<DbSeeder>>();

        _mapper = _scope.ServiceProvider.GetRequiredService<IMapper>();

        _shoppingServices = _scope.ServiceProvider.GetRequiredService<IShoppingServices>();

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

        List<CategoryCreateDto> cdtos = categorySeedData.Select(c => new CategoryCreateDto { Name = c.Name }).DistinctBy(c => c.Name).ToList();

        List<int> categoryIds = [];

        foreach (var cdto in cdtos)
        {
            var (succeeded, id, errmsgs) = await _shoppingServices.Category.TryCreateAsync(cdto);
            if (!succeeded)
            {
                _logger.LogError("Failed to seed category in database due to {ErrMsgs}", errmsgs);
                break;
            }
            categoryIds.Add((int)id!);
        }

        if (!await AddRoles())
        {
            return;
        }

        List<string> userIds = [];

        if (!(await MakeAdminUser(imageStreams)).TryOut(out string? adminId))
        {
            return;
        }
        else
        {
            userIds.Add(adminId!);
        }


        List<UserCreateDto> udtos = userSeedData
            .Select(u => new UserCreateDto()
            {
                UserName = u.UserName,
                Password = "123456",
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Roles = [],
                Description = u.Description,
                Image = imageStreams[_random.Next(imageStreams.Count)]
            })
            .DistinctBy(u => u.UserName)
            .ToList();

        foreach (var udto in udtos)
        {
            var (succeeded, id, errMsgs) = await _shoppingServices.User.TryCreateAsync(udto);
            if (udto.Image is not null)
            {
                udto.Image.Position = 0;
            }
            if (!succeeded)
            {
                _logger.LogError("Failed to seed user in database due to {ErrMsgs}", errMsgs);
                return;
            }
            userIds.Add(id!);
        }


        List<ProductCreateDto> pcdtos = productSeedData
            .Select(p => new ProductCreateDto
            {
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                CategoryId = categoryIds[_random.Next(categoryIds.Count)],
                SubmitterId = userIds[_random.Next(userIds.Count)],
                Image = imageStreams[_random.Next(imageStreams.Count)]
            })
            .DistinctBy(p => p.Name)
            .ToList();

        List<int> productIds = [];

        foreach (var pcdto in pcdtos)
        {
            var (succeeded, id, errmsgs) = await _shoppingServices.Product.TryCreateAsync(pcdto);
            if (pcdto.Image is not null)
            {
                pcdto.Image.Position = 0;
            }
            if (!succeeded)
            {
                _logger.LogError("Failed to seed product in database due to {ErrMsgs}", errmsgs);
                return;
            }
            productIds.Add(id ?? throw new Exception("Impossible"));
        }

        List<CartItem> cartItems = cartItemSeedData
            .Select(c => new CartItem
            {
                UserId = userIds[_random.Next(userIds.Count)],
                ProductId = productIds[_random.Next(productIds.Count)],
                Count = c.Count
            })
            .DistinctBy(c => new { c.ProductId, c.UserId })
            .ToList();

        foreach (var cartItem in cartItems)
        {
            var (succeed, errmsgs) = await _shoppingServices.CartItem.TryCreateAsync(cartItem);
            if (!succeed)
            {
                _logger.LogError("Failed to seed cartItem in database due to {Errmsgs}", errmsgs);
                return;
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
            if (!await _shoppingServices.Role.ExistsAsync(q => q.Where(r => r.Name == role)))
            {
                if (!(await _shoppingServices.Role.TryCreateAsync(new ApplicationRole(role))).TryOut(out var errMsgs))
                {
                    _logger.LogError("Failed to create role for role \"{Role}\" due to {ErrMsgs}", role, errMsgs);
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


    private async Task<(bool succeeded, string? id)> MakeAdminUser(List<Stream?> imageStreams)
    {

        ApplicationRole? adminRole = await _shoppingServices.Role.GetSingleAsync(q => q.Where(r => r.Name == "Admin"));
        if (adminRole == null)
        {
            _logger.LogError("Failed to get admin role from database");
            return (false, null);
        }

        var admindto = new UserCreateDto
        {
            UserName = "admin",
            Password = "123456",
            Email = "admin@admin.com",
            PhoneNumber = "111111",
            Description = "Admin user",
            Roles = adminRole is not null ? [adminRole] : [],
            Image = imageStreams[_random.Next(imageStreams.Count)]
        };

        var (succeeded, id, errMsgs) = await _shoppingServices.User.TryCreateAsync(admindto);
        if (admindto.Image is not null)
        {
            admindto.Image.Position = 0;
        }
        if (!succeeded)
        {
            _logger.LogError("Failed to seed user in database due to {ErrMsgs}", errMsgs);
            return (false, null);
        }

        return (true, id);
    }




}
