using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Data;
using SP_Shopping.Dtos;
using SP_Shopping.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SP_Shopping.Controllers;

public class CartController(ApplicationDbContext context, IMapper mapper) : Controller
{

    private readonly ApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    [Authorize]
    public IActionResult Index()
    {
        string? userName = User.FindFirstValue(ClaimTypes.Name);
        if (userName == null)
        {
            ViewBag.Message = "You need to log in to see your cart";
        }
        else
        {
            ViewBag.Message = $"Welcome {userName}";
        }
        
        IEnumerable<CartItem> cartItems = _context.CartItems.Where(c => c.User.UserName == userName);
        var cidtos = _mapper.Map<IEnumerable<CartItem>,IEnumerable<CartItemDetailsDto>>(cartItems);

        return View(cidtos);
    }
    [Route($"Cart/Index/{{{nameof(id)}}}")]
    public IActionResult Index(string? id)
    {
        if (!string.IsNullOrEmpty(id) && Regex.IsMatch(id, @"[sS]elf", RegexOptions.Compiled))
        {
            return View(nameof(Index));
        }
        return View();
    }

    [HttpPost()]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int productId)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool userExists = _context.Users.Any(u => u.Id == userId);
        bool productExists = _context.Products.Any(p => p.Id == productId);
        // Check that the keys are valid.
        if (!userExists || !productExists)
        {
            return NotFound("Product and user specified does not exist..");
        }
        CartItem cartItem = new()
        {
            UserId = userId!,
            ProductId = productId
        };
        await _context.CartItems.AddAsync(cartItem);
        int savedNum = await _context.SaveChangesAsync();

        return View(nameof(Index));
    }
}
