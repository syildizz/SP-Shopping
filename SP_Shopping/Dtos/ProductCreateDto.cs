using Microsoft.AspNetCore.Mvc.Rendering;
using SP_Shopping.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Dtos
{
    public class ProductCreateDto
    {
        [Required]
        [MaxLength(50, ErrorMessage = "A genre name cannot be longer than 50 characters.")]
        public string Name { get; set; }
        [DataType(DataType.Currency)]
        [Required]
        public decimal Price { get; set; }
        [DisplayName(nameof(Category))]
        public ICollection<SelectListItem> CategorySelectList { get; set; } = [];
        public int CategoryId { get; set; }
    }

}
