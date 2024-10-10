using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Dtos;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SP_Shopping.Controllers;

public class CartController
(
    ILogger<CartController> logger,
    IMapper mapper,
    IRepository<CartItem> cartItemRepository,
    IRepository<ApplicationUser> userRepository,
    IRepository<Product> productRepository
) : Controller
{

    private readonly ILogger<CartController> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IRepository<CartItem> _cartItemRepository = cartItemRepository;
    private readonly IRepository<ApplicationUser> _userRepository = userRepository;
    private readonly IRepository<Product> _productRepository = productRepository;

    [Authorize]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("GET: Cart/Index");
        string? userName = User.FindFirstValue(ClaimTypes.Name);
        if (userName == null)
        {
            _logger.LogError("Anonymous user logged in.");
            ViewBag.Message = "You need to log in to see your cart";
        }
        else
        {
            ViewBag.Message = $"Welcome {userName}";
        }

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogError("UserId does not exist i.e. no user is logged in.");
            return View("Error");
        }

        IEnumerable<CartItem> cartItems = await _cartItemRepository.GetAllAsync(q => q
           .Where(c => c.UserId == userId)
           .Include(c => c.Product)
           .Select(c => new CartItem()
           {
               ProductId = c.ProductId,
               UserId = c.User.Id,
               Count = c.Count,
               Product = new Product()
               {
                   Name = c.Product.Name
               }
           })
       );

        var cidtos = _mapper.Map<IEnumerable<CartItem>,IEnumerable<CartItemDetailsDto>>(cartItems);

        return View(cidtos);
    }
    [Route($"/Cart/Index/{{{nameof(id)}}}")]
    public async Task<IActionResult> Index(string? id)
    {
        _logger.LogInformation("GET: Cart/Index/id (Cart/Index action overloaded).");
        if (!string.IsNullOrEmpty(id) && Regex.IsMatch(id, @"[sS]elf", RegexOptions.Compiled))
        {
            _logger.LogInformation("Redirecting to main Index since id is {Id}.", id);
            return Redirect($"/Cart/{nameof(Index)}");
        }
        if (string.IsNullOrWhiteSpace(id) || !await _userRepository.ExistsAsync(q => q.Where(u => u.Id == id)))
        {
            _logger.LogError("UserId does not exist for user with given id of \"{Id}\".", id);
            return NotFound("The user was not found");
        }

        IEnumerable<CartItem> cartItems = await _cartItemRepository.GetAllAsync(q => q
                .Where(c => c.UserId == id)
                .Include(c => c.Product)
                .Select(c => new CartItem()
                {
                    ProductId = c.ProductId,
                    UserId = c.User.Id,
                    Count = c.Count,
                    Product = new Product()
                    {
                        Name = c.Product.Name
                    }
                })
            );

        IEnumerable<CartItemDetailsDto> cidto = _mapper.Map<IEnumerable<CartItem>, IEnumerable<CartItemDetailsDto>>(cartItems);

        ViewBag.Message = $"The shopping cart of {(await _userRepository.GetByKeyAsync(id))?.UserName ?? "User not found"}";

        return View(cidto);
    }

    public async Task<IActionResult> Details()
    {
        _logger.LogInformation("GET: Cart/Details.");

        IEnumerable<CartItem> cartItems = await _cartItemRepository.GetAllAsync(q => q
                .Include(c => c.Product)
                .Include(c => c.User)
                .Select(c => new CartItem()
                {
                    ProductId = c.ProductId,
                    UserId = c.User.Id,
                    Count = c.Count,
                    Product = new Product()
                    {
                        Name = c.Product.Name
                    },
                    User = new ApplicationUser()
                    {
                        UserName = c.User.UserName
                    }
                })
            );

        IEnumerable<CartItemDetailsDto> cidto = _mapper.Map<IEnumerable<CartItem>, IEnumerable<CartItemDetailsDto>>(cartItems);

        ViewBag.Message = $"The shopping carts of all users";

        return View(cidto);
    }

    [HttpPost()]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CartItemCreateDto cidto, string? returnPath)
    {
        _logger.LogInformation("POST: Cart/Create.");

        CartItem cartItem = _mapper.Map<CartItemCreateDto, CartItem>(cidto);
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogDebug("Checking if user with \"{UserId}\" exists in database.", userId);
        bool userExists = await _userRepository.ExistsAsync(q => q.Where(u => u.Id == userId));
        _logger.LogDebug("Checking if procduct with \"{ProductId}\" exists in database.", userId);
        bool productExists = await _productRepository.ExistsAsync(q => q.Where(p => p.Id == cartItem.ProductId));
        // Check that the keys are valid.
        if (string.IsNullOrWhiteSpace(userId) || !userExists || !productExists)
        {
            _logger.LogError("The product id of \"{ProductId}\" and user Id of \"{UserId}\" do not exist in the database.", userId, cartItem.ProductId);
            return NotFound("Product and user specified does not exist..");
        }

        cartItem.UserId = userId;

        try
        {
            _logger.LogDebug("Create CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", cartItem.UserId, cartItem.ProductId);
            await _cartItemRepository.CreateAsync(cartItem);
        }
        catch (DbUpdateException)
        { }

        return Redirect(returnPath ?? nameof(Index));
    }

    [HttpPost()]
    [ActionName("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(CartItemDetailsDto cidto)
    {
        _logger.LogInformation("POST: Cart/Delete.");

        if (cidto.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            _logger.LogError("The user is not allowed to log from other user's cart.");
            return Unauthorized("You cannot only delete products from your own shopping cart");
        }

        var cartItem = _mapper.Map<CartItemDetailsDto, CartItem>(cidto);

        _logger.LogDebug("Delete CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", cartItem.UserId, cartItem.ProductId);
        await _cartItemRepository.DeleteCertainEntriesAsync(q => q
            .Where(c => c.UserId == cartItem.UserId && c.ProductId == cartItem.ProductId)
        );

        return Redirect(nameof(Index));
    }

    [HttpPost()]
    public async Task<IActionResult> Edit(CartItemCreateDto cidto)
    {
        _logger.LogInformation("POST: Cart/Delete.");

        if (cidto.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            _logger.LogError("The user is not allowed to log from other user's cart.");
            return Unauthorized("You cannot only delete products from your own shopping cart");
        }

        var cartItem = _mapper.Map<CartItemCreateDto, CartItem>(cidto);

        _logger.LogDebug("Update CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", cartItem.UserId, cartItem.ProductId);
        await _cartItemRepository.UpdateCertainFieldsAsync(cartItem, 
            q => q
                .Where(c => c.UserId == cartItem.UserId && c.ProductId == cartItem.ProductId), 
            s => s
                .SetProperty(c => c.Count, cartItem.Count)
        );

        return Redirect(nameof(Index));
        
    }

}
