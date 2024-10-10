﻿using Microsoft.AspNetCore.Identity;

namespace SP_Shopping.Models;

public class ApplicationUser : IdentityUser
{
    public List<CartItem> CartItem { get; set; }
}
