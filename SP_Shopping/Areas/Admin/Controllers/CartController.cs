using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Areas.Admin.Dtos.Cart;
using SP_Shopping.Dtos.Cart;
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

    public async Task<IActionResult> Index(string? query, string? type)
    {
        _logger.LogInformation("GET: Entering Admin/Products.");

        Func<IQueryable<AdminCartItemDetailsDto>, IQueryable<AdminCartItemDetailsDto>>? userNameFilter;
        if (!string.IsNullOrWhiteSpace(query) && !string.IsNullOrWhiteSpace(type))
        {
            userNameFilter = type switch
            {
                nameof(AdminCartItemDetailsDto.ProductId) => int.TryParse(query, out var queryNumber) ? q => q.Where(c => c.ProductId == queryNumber) : q => q,
                nameof(AdminCartItemDetailsDto.ProductName) => q => q.Where(c => c.ProductName.Contains(query)),
                nameof(AdminCartItemDetailsDto.UserId) => q => q.Where(c => c.UserName.Contains(query)),
                nameof(AdminCartItemDetailsDto.UserName) => q => q.Where(c => c.UserName.Contains(query)),
                nameof(AdminCartItemDetailsDto.SubmitterId) => q => q.Where(c => c.SubmitterName.Contains(query)),
                nameof(AdminCartItemDetailsDto.SubmitterName) => q => q.Where(c => c.SubmitterName.Contains(query)),
                nameof(AdminCartItemDetailsDto.Count) => int.TryParse(query, out var queryNumber) ? q => q.Where(c => c.Count == queryNumber) : q => q,
                _ => null
            };
        }
        else
        {
            userNameFilter = q => q;
        }

        if (userNameFilter is null)
        {
            return BadRequest("Invalid query");
        }

        _logger.LogDebug("Fetching product information matching search term.");
        var cidtoList = await _cartItemRepository.GetAllAsync(q =>
            userNameFilter(_mapper.ProjectTo<AdminCartItemDetailsDto>(q)
                .OrderByDescending(p => p.UserName)
                .ThenByDescending(p => p.ProductName)
                .Take(20)
            )
        );

        return View(cidtoList);
        
    }

    public IActionResult Create()
    {
        _logger.LogInformation("GET: Admin/Cart/Create.");

        return View(new AdminCartItemCreateDto());
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
    public async Task<IActionResult> Delete(AdminCartItemCreateDto cidto)
    {
        if (ModelState.IsValid)
        {
            _logger.LogInformation("POST: Admin/Cart/Delete.");

            var cartItem = _mapper.Map<CartItem>(cidto);

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
