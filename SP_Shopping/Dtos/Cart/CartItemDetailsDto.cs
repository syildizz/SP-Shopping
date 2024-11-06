using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Dtos.Cart;

public class CartItemDetailsDto
{
    public string UserId { get; set; }
    public int ProductId { get; set; }
    [DisplayName("Cart Owner")]
    public string UserName { get; set; }
    [DisplayName("Seller")]
    public string SubmitterName { get; set; }
    [DisplayName("Product Name")]
    public string ProductName { get; set; }
    [RegularExpression("([0-9]+)", ErrorMessage = "Please enter valid Number")]
    [Range(0, int.MaxValue)]
    public int Count { get; set; }
}
