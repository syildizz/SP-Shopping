using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Dtos.Cart;

public class CartItemCreateDto
{
    [RegularExpression("([0-9]+)", ErrorMessage = "Cart cannot have negative count of items")]
    public int Count { get; set; }
}
