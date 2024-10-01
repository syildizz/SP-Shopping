using SP_Shopping.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Dtos;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
public class ProductDetailsDto
{
    public int Id { get; set; }
    [Required]
    [MaxLength(100, ErrorMessage = "The name of the product can at most be 100 characters.")]
    [DisplayName("Name")]
    public string Name { get; set; }
    [DataType(DataType.Currency)]
    [Required]
    [DisplayName("Price")]
    public decimal Price { get; set; }
    [DisplayName("Category")]
    public string? CategoryName { get; set; }
    [DataType(DataType.DateTime)]
    [DisplayName("Publishing Date")]
    public DateTime InsertionDate { get; set; }
    [DataType(DataType.DateTime)]
    [DisplayName("Last Modified")]
    public DateTime? ModificationDate { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
