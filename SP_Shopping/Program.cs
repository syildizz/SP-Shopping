using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using SP_Shopping.Config;
using SP_Shopping.Data;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Service;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;

namespace SP_Shopping;

public class Program
{

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseSqlServer(connectionString)
                
            );
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<ApplicationRole>()
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

        var imageLocationsOption = builder.Configuration.GetSection(ImageLocationsOption.ImageLocations).Get<ImageLocationsOption>();

        builder.Services.AddSingleton<IImageHandlerDefaulting<UserProfileImageKey>>(
            new ImageHandlerDefaulting<UserProfileImageKey>
            (
                folderPath: builder.Environment.WebRootPath,
                defaultImageFolder: imageLocationsOption?.Default.Folder ?? "img",
                defaultProp: imageLocationsOption?.Default.User ?? "default_pfp",
                imageFolder: imageLocationsOption?.User.Folder ?? "user-pfp",
                imgExtension: "png"
            )
        );
        builder.Services.AddSingleton<IImageHandlerDefaulting<ProductImageKey>>(
            new ImageHandlerDefaulting<ProductImageKey>
            (
                folderPath: builder.Environment.WebRootPath,
                defaultImageFolder: imageLocationsOption?.Default.Folder ?? "img",
                defaultProp: imageLocationsOption?.Default.Product ?? "default_product",
                imageFolder: imageLocationsOption?.Product.Folder ?? "product",
                imgExtension: "png"
            )
        );

        builder.Services.AddScoped<IShoppingServices, ShoppingServices>();

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
            name: "adminRoute",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
        );
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}"
        );
        app.MapRazorPages();

        if (app.Configuration.GetSection("SeedDatabase").Get<bool?>() is not null and true)
        {
            await new DbSeeder(app).Seed();
            app.Services.GetRequiredService<ILogger<Program>>().LogInformation("Finished seeding database");
            return;
        }

        app.Run();
    }

}
