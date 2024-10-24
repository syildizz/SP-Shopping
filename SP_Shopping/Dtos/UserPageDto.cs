using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Dtos;

public class UserPageDto
{

    public class UserPageProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
        [DisplayName("Category")]
        public string CategoryName { get; set; }
    }

    public string Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    [DisplayName("Account Creation")]
    public DateTime InsertionDate { get; set; }
    public IEnumerable<UserPageProductDto>? ProductDetails { get; set; }
}
