
namespace SP_Shopping.ServiceDtos;

public class ProductCreateDto : IDisposable
{
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public required int CategoryId { get; set; }
    public required string? Description { get; set; }
    public required string? SubmitterId { get; set; }

    public required Stream? Image { get; set; }
    public void Dispose() => (Image as IDisposable)?.Dispose();
}
