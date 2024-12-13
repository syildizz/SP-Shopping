using SP_Shopping.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Dtos.Product;

public class ProductDetailsDto
{
    public int Id { get; set; }
    [Required]
    [MaxLength(100, ErrorMessage = "The name of the product can at most be 100 characters.")]
    [DisplayName("Name")]
    public string Name { get; set; }
    [DataType(DataType.Currency)]
    [Required]
    [PrecisionAndScale(18, 2)]
    [DisplayName("Price")]
    public decimal Price { get; set; }
    [DisplayName("Category")]
    public string? CategoryName { get; set; }
    public string? Description { get; set; }
    [DisplayName("Poster")]
    public string SubmitterName { get; set; }
    public string SubmitterId { get; set; }
    [DataType(DataType.DateTime)]
    [DisplayName("Publishing Date")]
    public DateTime InsertionDate { get; set; }
    [DataType(DataType.DateTime)]
    [DisplayName("Last Modified")]
    public DateTime? ModificationDate { get; set; }
}
