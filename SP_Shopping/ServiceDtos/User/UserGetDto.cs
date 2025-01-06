using SP_Shopping.Models;
using SP_Shopping.ServiceDtos.Product;

namespace SP_Shopping.ServiceDtos.User;

public class UserGetDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public List<ApplicationRole> Roles { get; set; } = [];
    public string Description { get; set; }
    public DateTime InsertionDate { get; set; }
    public List<CartItem> CartItems { get; set; } = [];
    public List<ProductGetDto>? Products { get; set; }
}
