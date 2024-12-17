using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Dtos.Cart;

public class CartItemCreateDto
{
    [RegularExpression("([0-9]+)", ErrorMessage = "Count must be a positive integer")]
    public int Count { get; set; }
}
