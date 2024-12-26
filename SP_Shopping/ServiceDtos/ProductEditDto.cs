using SP_Shopping.Utilities.ImageHandlerKeys;

namespace SP_Shopping.ServiceDtos;

public class ProductEditDto : IDisposable
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string? Description { get; set; }
    public string? SubmitterId { get; set; }

    public Stream? Image { get; set; }
    public void Dispose() => (Image as IDisposable)?.Dispose();
}
