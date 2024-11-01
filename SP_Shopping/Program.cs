using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using SP_Shopping.Data;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.Message;

namespace SP_Shopping;

public class Program
{

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("IsAdmin", policy =>
            {
                policy
                    .RequireAuthenticatedUser()
                    .RequireRole("Admin")
                ;
            });

        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddMemoryCache();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireDigit = false;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.Zero;
            options.Lockout.MaxFailedAccessAttempts = 1000;
            options.Lockout.AllowedForNewUsers = false;

            options.User.RequireUniqueEmail = true;
        }
        );


        builder.Services.AddControllersWithViews();

        builder.Services.AddScoped(typeof(IRepository<>), typeof(RepositoryBase<>));
        builder.Services.AddScoped(typeof(IRepositoryCaching<>), typeof(RepositoryBaseCaching<>));
        builder.Services.AddSingleton<IMemoryCacher<string>, MemoryCacher<string>>();

        builder.Services.AddSingleton<IImageHandlerDefaulting<UserProfileImageKey>>(
            new ImageHandlerDefaulting<UserProfileImageKey>
            (
                folderPath: builder.Environment.WebRootPath,
                defaultProp: "default_pfp",
                keyName: "user-pfp",
                imgExtension: "png"
            )
        );
        builder.Services.AddSingleton<IImageHandlerDefaulting<ProductImageKey>>(
            new ImageHandlerDefaulting<ProductImageKey>
            (
                folderPath: builder.Environment.WebRootPath,
                defaultProp: "default_product",
                keyName: "product",
                imgExtension: "png"
            )
        );

        builder.Services.AddScoped<IMessageHandler, MessageHandler>();

        builder.Services.AddImageSharp(options =>
        {
            options.OnParseCommandsAsync = c =>
            {
                if (c.Commands.Count == 0)
                {
                    return Task.CompletedTask;
                }

                // It's a good idea to have this to provide very basic security.
                // We can safely use the static resize processor properties.
                uint width = c.Parser.ParseValue<uint>(
                    c.Commands.GetValueOrDefault(ResizeWebProcessor.Width),
                    c.Culture);

                uint height = c.Parser.ParseValue<uint>(
                    c.Commands.GetValueOrDefault(ResizeWebProcessor.Height),
                    c.Culture);

                if (width > 4000 || height > 4000)
                {
                    c.Commands.Remove(ResizeWebProcessor.Width);
                    c.Commands.Remove(ResizeWebProcessor.Height);
                }

                return Task.CompletedTask;
            };
        });



        var app = builder.Build();

        app.UseRequestLocalization("tr-TR");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseImageSharp();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        await AddRoles(app.Services);

        app.Run();
    }

    // https://stackoverflow.com/a/73410638
    private static async Task AddRoles(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var roles = GetRoleNames();
        if (roles is null) return;
        foreach (string role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    logger.LogError("Failed to create role for role \"{Role}\"", role);
                    return;
                }
            }
        }
    }

    private static List<string>? GetRoleNames()
    {
        //var roles = configuration.GetSection("Roles").Get<List<string>>();
        List<string> roles = 
        [
            "Admin"
        ];
        return roles;
    }

}
