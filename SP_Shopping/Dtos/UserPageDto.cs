using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel;

namespace SP_Shopping.Dtos;

public class UserPageDto
{

    public class UserPageProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        [DisplayName("Category Name")]
        public string CategoryName { get; set; }
    }

    public string Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public IEnumerable<UserPageProductDto>? ProductDetails { get; set; }
}
