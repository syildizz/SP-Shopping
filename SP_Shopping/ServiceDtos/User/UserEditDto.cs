using SP_Shopping.Models;

namespace SP_Shopping.ServiceDtos.User;

public class UserEditDto : IDisposable
{
    public string UserName { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public List<ApplicationRole> Roles { get; set; } = [];
    public string? Description { get; set; }
    public Stream? Image { get; set; }

    public void Dispose()
    {
        if (Image is not null and var image) {
            image.Dispose();
        }
    }
}
