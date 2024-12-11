using Microsoft.AspNetCore.Identity;

namespace SP_Shopping.Models;

public class ApplicationRole : IdentityRole
{
    
    public ApplicationRole() : base() { }

    public ApplicationRole(string roleName) : base(roleName) { }

    public List<ApplicationUser> Users { get; set; } = [];

}

