using SP_Shopping.Models;

namespace SP_Shopping.ServiceDtos;

public class ProductGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public string? Description { get; set; }
    public string? SubmitterId { get; set; }
    public ApplicationUser? Submitter { get; set; } = null;
    public List<CartItem> CartItem { get; set; } = [];
    public DateTime InsertionDate { get; set; }
    public DateTime? ModificationDate { get; set; } = null;
}
