using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Areas.Admin.Dtos.Cart;

public class AdminCartItemCreateDto
{
    [DisplayName("Cart Owner Id")]
    public string UserId { get; set; }
    [DisplayName("Product Id")]
    public int ProductId { get; set; }
    [RegularExpression("([0-9]+)", ErrorMessage = "Count must be a positive integer")]
    public int Count { get; set; }
}
