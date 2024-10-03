using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Data;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SP_Shopping.Controllers;

public class CartController(ApplicationDbContext context) : Controller
{

    private readonly ApplicationDbContext _context = context;

    [Authorize]
    public IActionResult Index()
    {
        string? userName = User.FindFirstValue(ClaimTypes.Name);
        string message = String.Empty;
        if (userName == null)
        {
            message = "You need to log in to see your cart";
        }
        else
        {
            message = $"Welcome {userName}";
        }
        return View(model: message);
    }
}
