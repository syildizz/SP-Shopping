using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Areas.Admin.Dtos;
using SP_Shopping.Dtos;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.MessageHandler;

namespace SP_Shopping.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
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

    public async Task<IActionResult> Index(string? userName)
    {
        _logger.LogInformation("GET: Admin/Cart/Index.");

        if (userName is not null && !await _userRepository.ExistsAsync(q => q.Where(u => u.UserName == userName)))
        {
            _logger.LogError("UserId does not exist for user with given id of \"{Id}\".", userName);
            return NotFound("The user was not found");
        }

        // Filter based on userName based on userName
        Func<IQueryable<CartItemDetailsDto>, IQueryable<CartItemDetailsDto>> userNameFilter;
        if (userName is not null)
        {
            userNameFilter = q => q.Where(u => u.UserName == userName);
            ViewBag.Message = $"The shopping cart of user {userName}";
        }
        else
        {
            userNameFilter = q => q;
            ViewBag.Message = $"The shopping carts of all users";
        }

        IEnumerable<CartItemDetailsDto> cdtos = await _cartItemRepository.GetAllAsync(q =>
            userNameFilter(
                _mapper.ProjectTo<CartItemDetailsDto>(q)
            )
        );

        return View("Index", cdtos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminCartItemCreateDto cidto)
    {
        _logger.LogInformation("POST: Admin/Cart/Create.");

        if (ModelState.IsValid)
        {

            CartItem cartItem = _mapper.Map<CartItem>(cidto);

            // Check that the keys are valid.

            _logger.LogDebug("Checking if user with \"{userId}\" exists in database.", cartItem.UserId);
            bool userExists = await _userRepository.ExistsAsync(q => q.Where(u => u.Id == cartItem.UserId));
            if (!userExists)
            {
                _logger.LogError("The user id of \"{UserId}\" does not exist in the database.", cartItem.UserId);
                return BadRequest("Invalid user id specified.");
            }

            _logger.LogDebug("Checking if product with \"{ProductId}\" exists in database.", cartItem.ProductId);
            bool productExists = await _productRepository.ExistsAsync(q => q.Where(p => p.Id == cartItem.ProductId));
            if (!productExists)
            {
                _logger.LogError("The product id of \"{ProductId}\" does not exist in the database.", cartItem.ProductId);
                return BadRequest("Invalid product id specified.");
            }

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
        }
        else
        {
            _logger.LogError("ModeState is invalid");
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Failed to add product to cart" });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(CartItemDetailsDto cidto)
    {
        if (ModelState.IsValid)
        {
            _logger.LogInformation("POST: Admin/Cart/Delete.");

            var cartItem = _mapper.Map<CartItemDetailsDto, CartItem>(cidto);

            _logger.LogDebug("Delete CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", cartItem.UserId, cartItem.ProductId);
            await _cartItemRepository.DeleteCertainEntriesAsync(q => q
                .Where(c => c.UserId == cartItem.UserId && c.ProductId == cartItem.ProductId)
            );
        }
        else
        {
            _logger.LogError("ModeState is invalid");
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Failed to remove product from cart" });
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminCartItemCreateDto cidto)
    {
        _logger.LogInformation("POST: Admin/Cart/Edit.");

        if (ModelState.IsValid)
        {
            var cartItem = _mapper.Map<CartItem>(cidto);

            _logger.LogDebug("Update CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", cartItem.UserId, cartItem.ProductId);
            await _cartItemRepository.UpdateCertainFieldsAsync(
                q => q
                    .Where(c => c.UserId == cartItem.UserId && c.ProductId == cartItem.ProductId), 
                s => s
                    .SetProperty(c => c.Count, cartItem.Count)
            );
        }
        else
        {
            _logger.LogError("ModeState is invalid");
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Failed to change count of product in cart" });
        }

        return RedirectToAction(nameof(Index));
        
    }

}
