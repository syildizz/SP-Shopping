using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SP_Shopping.Models;

[PrimaryKey(nameof(UserId), nameof(ProductId))]
public class CartItem
{
    [Key]
    [Column(Order = 1)]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    [Key]
    [Column(Order = 2)]
    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }
    public Product Product { get; set; }
    [RegularExpression("([0-9]+)", ErrorMessage = "Please enter valid Number")]
    [Range(0, int.MaxValue)]
    public int Count { get; set; }
}
