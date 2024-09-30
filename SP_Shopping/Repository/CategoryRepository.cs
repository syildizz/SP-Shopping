using SP_Shopping.Data;
using SP_Shopping.Models;

namespace SP_Shopping.Repository;

public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {

    }
}
