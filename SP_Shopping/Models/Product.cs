using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SP_Shopping.Models
{
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
        [DataType(DataType.DateTime)]
        public DateTime InsertionDate { get; set; } 
        [DataType(DataType.DateTime)]
        public DateTime? ModificationDate { get; set; }
    }
}
