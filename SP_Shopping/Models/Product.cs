using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SP_Shopping.Models;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

[Index(nameof(Name),IsUnique = true)]
public class Product
{
    public int Id { get; set; }
    [Required]
    [MaxLength(100, ErrorMessage="The name of the product can at most be 100 characters.")]
    public string Name { get; set; }
    [DataType(DataType.Currency)]
    [Required]
    public decimal Price { get; set; }
    [ForeignKey(nameof(Category))]
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    [ForeignKey(nameof(ApplicationUser))]
    public string? SubmitterId { get; set; }
    public ApplicationUser? Submitter { get; set; }
    public List<CartItem> CartItem { get; set; }
    [DataType(DataType.DateTime)]
    public DateTime InsertionDate { get; set; } 
    public DateTime? ModificationDate { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

