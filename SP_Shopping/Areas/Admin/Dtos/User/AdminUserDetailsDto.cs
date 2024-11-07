namespace SP_Shopping.Areas.Admin.Dtos.User;

public class AdminUserDetailsDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public List<string> Roles { get; set; }
    public string Description { get; set; }
    public DateTime InsertionDate { get; set; }

}
