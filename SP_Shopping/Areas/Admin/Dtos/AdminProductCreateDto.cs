using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Attributes;
using SP_Shopping.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Areas.Admin.Dtos;


[RequestSizeLimit(2_000_000)]
public class AdminProductCreateDto
{
    public int? Id { get; set; }
    [Required]
    [MaxLength(50, ErrorMessage = "A genre name cannot be longer than 50 characters.")]
    public string Name { get; set; }
    [DataType(DataType.Currency)]
    [Required]
    [PrecisionAndScale(18, 2)]
    public decimal Price { get; set; }
    [DisplayName(nameof(Category))]
    public int CategoryId { get; set; }
    public string? Description { get; set; }
    [Display(Name = "Product Picture")]
    [DataType(DataType.Upload)]
    public IFormFile? ProductImage { get; set; }
    [DisplayName("Submitter")]
    public string SubmitterId { get; set; }
}

