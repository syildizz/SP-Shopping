using SP_Shopping.Utilities.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Areas.Admin.Dtos.Product;

public class AdminProductDetailsDto
{
    public int Id { get; set; }
    [Required]
    [MaxLength(100, ErrorMessage = "The name of the product can at most be 100 characters.")]
    [DisplayName("Name")]
    public string Name { get; set; }
    [DataType(DataType.Currency)]
    [Required]
    [PrecisionAndScale(18, 2)]
    public decimal Price { get; set; }
    [DisplayName("Category Name")]
    public string? CategoryName { get; set; }
    public string? Description { get; set; }
    [DisplayName("Submitter Name")]
    public string SubmitterName { get; set; }
    [DisplayName("Submitter Id")]
    public string SubmitterId { get; set; }
    [DataType(DataType.DateTime)]
    [DisplayName("Publishing Date")]
    public DateTime InsertionDate { get; set; }
    [DataType(DataType.DateTime)]
    [DisplayName("Last Modification Date")]
    public DateTime? ModificationDate { get; set; }
}
