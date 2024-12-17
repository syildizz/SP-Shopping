

using SP_Shopping.Utilities.Attributes;

namespace SP_Shopping.Areas.Admin.Dtos.User;

public class AdminUserEditDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = [];

    public string? RoleString { 
        get => string.Join(", ", Roles); 
        set => Roles = !string.IsNullOrWhiteSpace(value) ? value.Split(", ").ToList() : []; 
    }
    public string? Description { get; set; }
    [IsImageFile]
    public IFormFile? ProfilePicture { get; set; }
}
