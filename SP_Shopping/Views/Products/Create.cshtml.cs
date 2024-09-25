using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SP_Shopping.Data;
using SP_Shopping.Models;
using SQLitePCL;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace SP_Shopping.Views.Products
{
    public class ProductCreateViewModel
    {
        [Required]
        [MaxLength(50, ErrorMessage = "A genre name cannot be longer than 50 characters.")]
        public string Name { get; set; }
        [DataType(DataType.Currency)]
        [Required]
        public decimal Price { get; set; }
        public ICollection<SelectListItem> CategorySelectItems { get; set; } = [];
        public int CategorySelectedValue { get; set; }

        public Product Product(Category? category)
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

        //public ProductCreateViewModel SetCategorySelectList(IEnumerable<Category> categories)
        //{
        //    foreach(var category in categories)
        //    {
        //        CategorySelectItems.Add(new SelectListItem()
        //            {
        //                Text = category.Name,
        //                Value = category.Id.ToString()
        //            }
        //        );
        //    }
        //    return this;
        //}

        public ProductCreateViewModel SetCategorySelectList(IEnumerable<Category> categories, int? selected = null)
        {
            foreach(var category in categories)
            {
                bool isSelected = false;
                if (selected != null && category.Id == selected)
                {
                    isSelected = true;
                }
                CategorySelectItems.Add(new SelectListItem()
                    {
                        Text = category.Name,
                        Value = category.Id.ToString(),
                        Selected = isSelected
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
