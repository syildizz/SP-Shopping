using SP_Shopping.ServiceDtos.Product;

namespace SP_Shopping.ServiceDtos.Category;

public class CategoryGetDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public List<ProductGetDto> Products { get; set; } = [];
}
