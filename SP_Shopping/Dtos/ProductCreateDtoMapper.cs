using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using SP_Shopping.Data;
using SP_Shopping.Models;
using SQLitePCL;

namespace SP_Shopping.Dtos
{
    public class ProductCreateDtoMapper : IMapper<ProductCreateDto, Product>
    {

        private ApplicationDbContext _context;

        public ProductCreateDtoMapper(ApplicationDbContext context)
        {
            _context = context;
        }

        public ProductCreateDto MapTo(Product product)
        {
            ProductCreateDto dto = new ProductCreateDto();
            dto.Name = product.Name;
            dto.Price = product.Price;
            dto.CategorySelectListItems = _context.Categories
                .Select(c => new SelectListItem()
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
                .ToList();
            dto.CategorySelectedOptionValue = product.CategoryId;
            return dto;

        }
        public Product MapFrom(ProductCreateDto productCreateDto)
        {
            Product product = new Product()
            {
                Name = productCreateDto.Name,
                Price = productCreateDto.Price,
                CategoryId = productCreateDto.CategorySelectedOptionValue
            };
            return product;
        }


    }
}
