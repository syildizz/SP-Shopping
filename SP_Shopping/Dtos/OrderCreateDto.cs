using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Dtos;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
public class OrderCreateDto
{
    [Required(ErrorMessage="You need to enter a username.")]
    [DisplayName("Customer Name")]
    public string UserName { get; set; }
    [DisplayName("Product Name")]
    public List<string> ProductNames { get; set; } = [];
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
