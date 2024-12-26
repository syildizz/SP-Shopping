using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Dtos.Cart;
using SP_Shopping.Models;
using SP_Shopping.Service;
using SP_Shopping.Utilities;
using SP_Shopping.Utilities.Filters;
using SP_Shopping.Utilities.MessageHandler;
using System.Security.Claims;

namespace SP_Shopping.Controllers;

[Authorize]
public class CartController
(
    ILogger<CartController> logger,
    IMapper mapper,
    IShoppingServices shoppingServices,
    IMessageHandler messageHandler
) : Controller
{
    private readonly ILogger<CartController> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IShoppingServices _shoppingServices = shoppingServices;
    private readonly IMessageHandler _messageHandler = messageHandler;

    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("GET: Cart/Index");

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        IEnumerable<CartItemDetailsDto> cidtos = await _shoppingServices.CartItem.GetAllAsync(q => 
            _mapper.ProjectTo<CartItemDetailsDto>(q
               .Where(c => c.UserId == userId)
            )
       );

        ViewBag.TotalPrice = cidtos.Select(m => new { m.Price, m.Count }).Aggregate(0M, (acc, curr) => acc + curr.Price * curr.Count);
        return View(cidtos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Create(int? id)
    {
        _logger.LogInformation("POST: Cart/Create.");

        _logger.LogDebug("Checking if product with \"{ProductId}\" exists in database.", id);
        bool productExists = await _shoppingServices.Product.ExistsAsync((int)id!);

        if (!productExists)
        {
            _logger.LogError("The product id of \"{ProductId}\" does not exist in the database.", id);
            return NotFound("Product not found");
        }

        CartItem cartItem = new()
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            ProductId = (int)id!,
            Count = 1
        };

        _logger.LogDebug("Create CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", cartItem.UserId, cartItem.ProductId);
        if (!(await _shoppingServices.CartItem.TryCreateAsync(cartItem)).TryOut(out var errMsgs))
        {
            _messageHandler.Add(TempData, errMsgs!);
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Edit(int? id, CartItemCreateDto cidto)
    {
        _logger.LogInformation("POST: Cart/Edit.");

        if (!ModelState.IsValid)
        {
            _logger.LogError("ModelState is invalid");
            _messageHandler.Add(TempData, [.. ModelState.GetErrorMessages().Select(em => new Message
            {
                Type = Message.MessageType.Warning,
                Content = em
            })]);
            return RedirectToAction(nameof(Index));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        CartItem cartItem = new() 
        { 
            ProductId = (int)id, 
            UserId = userId, 
            Count = cidto.Count 
        };

        _logger.LogDebug("Update CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", userId, id);
        if (!(await _shoppingServices.CartItem.TryUpdateAsync(cartItem)).TryOut(out var errMsgs))
        {
            _messageHandler.Add(TempData, errMsgs!);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Delete(int? id)
    {
        _logger.LogInformation("POST: Cart/Delete.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        CartItem cartItem = new() { ProductId = (int)id, UserId = userId };
        _logger.LogDebug("Delete CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", userId, id);
        if (!(await _shoppingServices.CartItem.TryDeleteAsync(cartItem)).TryOut(out var errMsgs))
        {
            _messageHandler.Add(TempData, errMsgs!);
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
    }

}
