using SP_Shopping.Data;
using SP_Shopping.Models;

namespace SP_Shopping.Repository
{
    public class CategoryRepository : RepositoryBase<Category>
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}
