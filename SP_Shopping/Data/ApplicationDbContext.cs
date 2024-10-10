using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using SP_Shopping.Models;

namespace SP_Shopping.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {}
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<CartItem> CartItems { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Product>()
            .Navigation(p => p.Category)
            .AutoInclude();

        //new DbSeeder(builder).Seed();

        base.OnModelCreating(builder);
    }

    //protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    //{
    //    configurationBuilder.Conventions.Remove(typeof(CascadeDeleteConvention));
    //    configurationBuilder.Conventions.Remove(typeof(SqlServerOnDeleteConvention));
    //}

}
