using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Dtos;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.Message;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SP_Shopping.Controllers;

public class CartController
(
    ILogger<CartController> logger,
    IMapper mapper,
    IRepository<CartItem> cartItemRepository,
    IRepository<ApplicationUser> userRepository,
    IRepository<Product> productRepository,
    IMessageHandler messageHandler

) : Controller
{

    private readonly ILogger<CartController> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IRepository<CartItem> _cartItemRepository = cartItemRepository;
    private readonly IRepository<ApplicationUser> _userRepository = userRepository;
    private readonly IRepository<Product> _productRepository = productRepository;
    private readonly IMessageHandler _messageHandler = messageHandler;

    [Authorize]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("GET: Cart/Index");

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogError("UserId does not exist i.e. no user is logged in.");
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "You need to log in to see your cart" });
            return RedirectToAction("Index", "Home");
        }

        IEnumerable<CartItemDetailsDto> cidtos = await _cartItemRepository.GetAllAsync(q => 
            _mapper.ProjectTo<CartItemDetailsDto>(q
               .Where(c => c.UserId == userId)
            )
       );

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

        IEnumerable<CartItemDetailsDto> cidtos = await _cartItemRepository.GetAllAsync(q =>
            _mapper.ProjectTo<CartItemDetailsDto>(q
                .Where(c => c.UserId == id)
            )
        );


        ViewBag.Message = $"The shopping cart of {(await _userRepository.GetByKeyAsync(id))?.UserName ?? "User not found"}";

        return View(cidtos);
    }

    public async Task<IActionResult> Details()
    {
        _logger.LogInformation("GET: Cart/Details.");

        IEnumerable<CartItemDetailsDto> cidtos = await _cartItemRepository.GetAllAsync(q => 
            _mapper.ProjectTo<CartItemDetailsDto>(q

                .Include(c => c.Product)
                .ThenInclude(p => p.Submitter)
                .Include(c => c.User)
            )
        );

        ViewBag.Message = $"The shopping carts of all users";

        return View(cidtos);
    }

    [HttpPost()]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CartItemCreateDto cidto)
    {
        _logger.LogInformation("POST: Cart/Create.");

        CartItem cartItem = _mapper.Map<CartItemCreateDto, CartItem>(cidto);
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Check that the keys are valid.

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogError("The user id of \"{UserId}\" does not exist in the database.", userId);
            return BadRequest("Invalid user id specified.");
        }

        _logger.LogDebug("Checking if product with \"{ProductId}\" exists in database.", userId);
        bool productExists = await _productRepository.ExistsAsync(q => q.Where(p => p.Id == cartItem.ProductId));

        if (!productExists)
        {
            _logger.LogError("The product id of \"{ProductId}\" does not exist in the database.", cartItem.ProductId);
            return BadRequest("Invalid product id specified.");
        }

        cartItem.UserId = userId;

        try
        {
            _logger.LogDebug("Create CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", cartItem.UserId, cartItem.ProductId);
            await _cartItemRepository.CreateAsync(cartItem);
            await _cartItemRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Exception occurs when adding same product to same users cart.
            // This is a desired effect, therefore the below codoe is commented out.
            // TODO: Analyze update exception for the above mentioned exception and throw 
            //     otherwise
            //_logger.LogError("Failed to create CartItem in the database for user of id \"{UserId}\" and for product of \"{ProductId}\".", cartItem.UserId, cartItem.ProductId);
            //_messageHandler.AddMessages(TempData, [new Message { Type = Message.MessageType.Error, Content = "Error when adding product to cart" }]);
        }

        return Redirect(nameof(Index));
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
            return Unauthorized("You can only delete products from your own shopping cart");
        }

        var cartItem = _mapper.Map<CartItemDetailsDto, CartItem>(cidto);

        _logger.LogDebug("Delete CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", cartItem.UserId, cartItem.ProductId);
        await _cartItemRepository.DeleteCertainEntriesAsync(q => q
            .Where(c => c.UserId == cartItem.UserId && c.ProductId == cartItem.ProductId)
        );

        return Redirect(nameof(Index));
    }

    [HttpPost()]
    [Authorize]
    [ValidateAntiForgeryToken]
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
        await _cartItemRepository.UpdateCertainFieldsAsync(
            q => q
                .Where(c => c.UserId == cartItem.UserId && c.ProductId == cartItem.ProductId), 
            s => s
                .SetProperty(c => c.Count, cartItem.Count)
        );

        return Redirect(nameof(Index));
        
    }

}
