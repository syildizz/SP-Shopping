using Microsoft.AspNetCore.Identity;

namespace SP_Shopping.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<Order> orders = default!;
    }
}
