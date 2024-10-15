using SP_Shopping.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Dtos;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
public class ProductCreateDto
{
    [Required]
    [MaxLength(50, ErrorMessage = "A genre name cannot be longer than 50 characters.")]
    public string Name { get; set; }
    [DataType(DataType.Currency)]
    [Required]
    public decimal Price { get; set; }
    [DisplayName(nameof(Category))]
    public int CategoryId { get; set; }
    public string? Description { get; set; }
    [DisplayName("Poster")]
    public string? SubmitterName { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

