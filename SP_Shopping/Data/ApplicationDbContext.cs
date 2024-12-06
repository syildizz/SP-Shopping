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
        builder.Entity<ApplicationUser>()
            .ToTable(tb => tb.HasTrigger("trg_DeleteUsers"));

        base.OnModelCreating(builder);
    }

}
