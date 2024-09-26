using SP_Shopping.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.ViewModels
{
    public class ProductDetailsViewModel
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100, ErrorMessage="The name of the product can at most be 100 characters.")]
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

        public ProductDetailsViewModel SetProductFields(Product product)
        {
            Id = product.Id;
            Name = product.Name;
            Price = product.Price;
            CategoryName = product.Category?.Name;
            InsertionDate = product.InsertionDate;
            ModificationDate = product.ModificationDate;
            return this;
        }
    }
}
