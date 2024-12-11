using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Models;

public class ApplicationUser : IdentityUser
{
    public List<CartItem> CartItem { get; set; }
    public List<Product>? Products { get; set; }
    [MaxLength(1000, ErrorMessage = "The description can be at maximum 1000 characters long")]
    public string? Description { get; set; }
    public DateTime InsertionDate { get; set; }
    public List<ApplicationRole> Roles { get; set; } = [];
}
