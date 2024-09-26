using SP_Shopping.Data;
using SP_Shopping.Models;

namespace SP_Shopping.Dtos
{
    public class ProductDetailsDtoMapper : IMapper<ProductDetailsDto, Product>
    {
        private ApplicationDbContext _context;
        public ProductDetailsDtoMapper(ApplicationDbContext context)
        {
            _context = context;
        }
        public Product Map(ProductDetailsDto productCreateDto)
        {
            Product product = new Product()
            {
                Name = productCreateDto.Name,
                Price = productCreateDto.Price,
                CategoryId = _context.Categories.Where(c => c.Name == productCreateDto.CategoryName).First().Id,
                InsertionDate = productCreateDto.InsertionDate,
                ModificationDate = productCreateDto.ModificationDate
            };
            return product;
        }

        public ProductDetailsDto Map(Product product)
        {
            ProductDetailsDto dto = new ProductDetailsDto();
            dto.Id = product.Id;
            dto.Name = product.Name;
            dto.Price = product.Price;
            if (product.Category != null)
            {
                dto.CategoryName = product.Category.Name;
            }
            else
            {
                dto.CategoryName = _context.Categories.Where(c => c.Id == product.CategoryId).FirstOrDefault()?.Name;
            }
            dto.InsertionDate = product.InsertionDate;
            dto.ModificationDate = product.ModificationDate;
            return dto;
        }
    }
}
