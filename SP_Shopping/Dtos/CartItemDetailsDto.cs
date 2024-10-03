using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace SP_Shopping.Dtos
{
    public class CartItemDetailsDto
    {
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public string UserName { get; set; }
        public string ProductName { get; set; }
    }
}
