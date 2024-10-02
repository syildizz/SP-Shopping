using Microsoft.AspNetCore.Identity;

namespace SP_Shopping.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<Cart> Cart { get; set; }
        public List<Product> Products { get; set; }
    }
}
