using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SP_Shopping.Models;

[PrimaryKey(nameof(UserId), nameof(ProductId))]
public class CartItem
{
    [Key]
    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }
    public Product Product { get; set; }
    [Key]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}
