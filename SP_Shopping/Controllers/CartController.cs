using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Data;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SP_Shopping.Controllers;

public class CartController(ApplicationDbContext context) : Controller
{

    private readonly ApplicationDbContext _context = context; 

    public IActionResult Index(string? id)
    {
        if (!string.IsNullOrEmpty(id) && Regex.IsMatch(id, @"[sS]elf"))
        {
            id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
        return View(model: id);
    }
}
