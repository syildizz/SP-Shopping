using Microsoft.AspNetCore.Mvc.Rendering;
using SP_Shopping.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.ViewModels
{
    public class ProductCreateViewModel
    {
        [Required]
        [MaxLength(50, ErrorMessage = "A genre name cannot be longer than 50 characters.")]
        public string Name { get; set; }
        [DataType(DataType.Currency)]
        [Required]
        public decimal Price { get; set; }
        [DisplayName(nameof(Category))]
        public ICollection<SelectListItem> CategorySelectListItems { get; set; } = [];
        public int CategorySelectedOptionValue { get; set; }

        public Product GetProductFields (Category? category)
        {
            Product product = new Product()
            {
                Name = Name,
                Price = Price,
                InsertionDate = DateTime.Now,
                Category = category
            };
            return product;

        }

        public ProductCreateViewModel SetCategorySelectList(IEnumerable<Category> categories, int? selected = null)
        {
            if (selected != null)
            {
                CategorySelectedOptionValue = (int)selected;
            }
            foreach (var category in categories)
            {
                CategorySelectListItems.Add(new SelectListItem()
                {
                    Text = category.Name,
                    Value = category.Id.ToString(),
                }
                );
            }
            return this;
        }

        public ProductCreateViewModel SetProductFields(Product product)
        {
            Name = product.Name;
            Price = product.Price;
            return this;
        }

    }

}
