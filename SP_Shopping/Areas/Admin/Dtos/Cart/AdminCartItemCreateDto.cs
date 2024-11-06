using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace SP_Shopping.Areas.Admin.Dtos.Cart;

public class AdminCartItemCreateDto
{
    public string UserId { get; set; }
    public int ProductId { get; set; }
    public int Count { get; set; }
}
