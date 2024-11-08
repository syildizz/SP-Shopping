using System.ComponentModel;

namespace SP_Shopping.Areas.Admin.Dtos.User;

public class AdminUserDetailsDto
{
    public string Id { get; set; }
    [DisplayName("User Name")]
    public string UserName { get; set; }
    public string Email { get; set; }
    [DisplayName("Phone Number")]
    public string PhoneNumber { get; set; }
    public List<string> Roles { get; set; }
    public string Description { get; set; }
    [DisplayName("Join Date")]
    public DateTime InsertionDate { get; set; }

}
