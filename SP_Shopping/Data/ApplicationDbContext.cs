using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using SP_Shopping.Models;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;

namespace SP_Shopping.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
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
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .ToTable(tb => tb.HasTrigger("trg_DeleteUsers"));

        // Many-to-Many: Users <-> Roles
        builder.Entity<ApplicationUser>()
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity<IdentityUserRole<string>>(
                j => j.HasOne<ApplicationRole>().WithMany().HasForeignKey(ur => ur.RoleId),
                j => j.HasOne<ApplicationUser>().WithMany().HasForeignKey(ur => ur.UserId),
                j =>
                {
                    j.HasKey(ur => new { ur.UserId, ur.RoleId });
                    j.ToTable("AspNetUserRoles"); // Default Identity table
                }
            );    
    }

}
