using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Category
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "A genre name cannot be longer than 50 characters.")]
        public required string Name { get; set; }
        public required IEnumerable<Product> Products { get; set; }
    }
}
