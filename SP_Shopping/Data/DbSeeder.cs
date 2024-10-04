using Microsoft.EntityFrameworkCore;
using SP_Shopping.Models;

namespace SP_Shopping.Data;

public class DbSeeder
{
    private readonly ModelBuilder _modelBuilder;

    public DbSeeder(ModelBuilder modelBuilder)
    {
        _modelBuilder = modelBuilder;
    }

    public void Seed()
    {
        IEnumerable<Product>? seedData = new ProductSeederFromJson("MOCK_DATA.json").MakeProductList();
        if (seedData != null)
        {
            _modelBuilder.Entity<Product>().HasData(seedData);
        }
    }

}
