using System.ComponentModel.DataAnnotations.Schema;

namespace SP_Shopping.Models;

public class Order
{
    public int Id { get; set; }
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = default!;
    public List<Product> Products { get; set; } = null!;
    public DateTime InsertionDate { get; set; }
    public DateTime? ModificationDate { get; set; } = default;
}
