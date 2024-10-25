using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using SP_Shopping.Models;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;

namespace SP_Shopping.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext
    (
        DbContextOptions<ApplicationDbContext> options, 
        IImageHandlerDefaulting<UserProfileImageKey> userImageHandler, 
        IImageHandlerDefaulting<ProductImageKey> productImageHandler
    )
        : base(options)
    {
        _userImageHandler = userImageHandler;
        _productImageHandler = productImageHandler;
    }
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<CartItem> CartItems { get; set; } = default!;

    private readonly IImageHandlerDefaulting<UserProfileImageKey> _userImageHandler;
    private readonly IImageHandlerDefaulting<ProductImageKey> _productImageHandler;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Product>()
            .Navigation(p => p.Category)
            .AutoInclude();

        //new DbSeeder(builder).Seed();

        base.OnModelCreating(builder);
    }

    public override int SaveChanges()
    {
        ChangeTracker.DetectChanges();
        var added = ChangeTracker.Entries()
                    .Where(t => t.State == EntityState.Deleted)
                    .Select(t => t.Entity)
                    .ToArray();

        foreach (var entity in added)
        {
            if (entity is ApplicationUser user)
            {
                _userImageHandler.DeleteImage(new(user.Id));
            }
            else if (entity is Product product)
            {
                _productImageHandler.DeleteImage(new(product.Id));
            }
        }
        return base.SaveChanges();
    }

    //protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    //{
    //    configurationBuilder.Conventions.Remove(typeof(CascadeDeleteConvention));
    //    configurationBuilder.Conventions.Remove(typeof(SqlServerOnDeleteConvention));
    //}

}
