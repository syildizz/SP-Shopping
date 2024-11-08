using System.ComponentModel;

namespace SP_Shopping.Areas.Admin.Dtos.Cart;

public class AdminCartItemCreateDto
{
    [DisplayName("Cart Owner Id")]
    public string UserId { get; set; }
    [DisplayName("Product Id")]
    public int ProductId { get; set; }
    public int Count { get; set; }
}
