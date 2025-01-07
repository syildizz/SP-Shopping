using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Areas.Admin.Dtos.Cart;
using SP_Shopping.Models;
using SP_Shopping.Service;
using SP_Shopping.Utilities;
using SP_Shopping.Utilities.MessageHandler;

namespace SP_Shopping.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
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

    public async Task<IActionResult> Index(string? query, string? type, [FromQuery] bool? sort)
    {
        _logger.LogInformation("GET: Entering Admin/Products.");

        Func<IQueryable<CartItem>, IQueryable<CartItem>> queryFilter = q => q;
        Func<IQueryable<CartItem>, IQueryable<CartItem>> sortFilter = q => q
            .OrderByDescending(p => p.User.UserName)
            .ThenByDescending(p => p.Product.Name);

        try
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                if (!string.IsNullOrWhiteSpace(query))
                {
                    queryFilter = type switch
                    {
                        nameof(AdminCartItemDetailsDto.ProductId) => 
                            int.TryParse(query, out var queryNumber) 
                                ? q => q.Where(c => c.ProductId == queryNumber) 
                                : q => q,
                        nameof(AdminCartItemDetailsDto.ProductName) => 
                            q => q.Where(c => c.Product.Name.Contains(query)),
                        nameof(AdminCartItemDetailsDto.UserId) => 
                            q => q.Where(c => c.User.UserName != null 
                                      && c.User.UserName.Contains(query)),
                        nameof(AdminCartItemDetailsDto.UserName) => 
                            q => q.Where(c => c.User.UserName != null 
                                           && c.User.UserName.Contains(query)),
                        nameof(AdminCartItemDetailsDto.SubmitterId) => 
                            q => q.Where(c => c.Product.SubmitterId != null 
                                           && c.Product.SubmitterId.Contains(query)),
                        nameof(AdminCartItemDetailsDto.SubmitterName) => 
                            q => q.Where(c => c.Product.Submitter != null 
						                   && c.Product.Submitter.UserName != null 
                                           && c.Product.Submitter.UserName.Contains(query)),
                        nameof(AdminCartItemDetailsDto.Count) => 
                            int.TryParse(query, out var queryNumber) 
                                ? q => q.Where(c => c.Count == queryNumber) 
                                : q => q,
                        _ => throw new NotImplementedException($"{type} is invalid")
                    };
                }

                sort ??= false;
                sortFilter = type switch
                {
                    nameof(AdminCartItemDetailsDto.ProductId) => (bool)sort 
						? q => q.OrderBy(c => c.ProductId) 
						: q => q.OrderByDescending(c => c.ProductId),
                    nameof(AdminCartItemDetailsDto.ProductName) => (bool)sort 
						? q => q.OrderBy(c => c.Product.Name) 
						: q => q.OrderByDescending(c => c.Product.Name),
                    nameof(AdminCartItemDetailsDto.UserId) => (bool)sort 
						? q => q.OrderBy(c => c.UserId) 
						: q => q.OrderByDescending(c => c.UserId),
                    nameof(AdminCartItemDetailsDto.UserName) => (bool)sort 
						? q => q.OrderBy(c => c.User.UserName) 
						: q => q.OrderByDescending(c => c.User.UserName),
                    nameof(AdminCartItemDetailsDto.SubmitterId) => (bool)sort 
						? q => q.OrderBy(c => c.Product.SubmitterId) 
						: q => q.OrderByDescending(c => c.Product.SubmitterId),
                    nameof(AdminCartItemDetailsDto.SubmitterName) => (bool)sort 
						? q => q.OrderBy(c => c.Product.Submitter.UserName) 
						: q => q.OrderByDescending(c => c.Product.Submitter.UserName),
                    nameof(AdminCartItemDetailsDto.Count) => (bool)sort 
						? q => q.OrderBy(c => c.Count) 
						: q => q.OrderByDescending(c => c.Count),
                    _ => throw new NotImplementedException($"{type} is invalid")
                };

            }
        }
        catch (NotImplementedException ex)
        {
            return BadRequest(ex.Message);
        }


        _logger.LogDebug("Fetching product information matching search term.");
        var cidtoList = await _shoppingServices.CartItem.GetAllAsync(q =>
            _mapper.ProjectTo<AdminCartItemDetailsDto>(q
                ._(queryFilter)
                ._(sortFilter)
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

        if (!ModelState.IsValid)
        {
            _logger.LogError("ModeState is invalid");
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Failed to add product to cart" });

            return RedirectToAction(nameof(Index));
        }

        CartItem cartItem = _mapper.Map<CartItem>(cidto);

        // Check that the keys are valid.

        _logger.LogDebug("Checking if user with \"{userId}\" exists in database.", cartItem.UserId);
        bool userExists = await _shoppingServices.User.ExistsAsync(cartItem.UserId);
        if (!userExists)
        {
            _logger.LogError("The user id of \"{UserId}\" does not exist in the database.", cartItem.UserId);
            return BadRequest("Invalid user id specified.");
        }

        _logger.LogDebug("Checking if product with \"{ProductId}\" exists in database.", cartItem.ProductId);
        bool productExists = await _shoppingServices.Product.ExistsAsync(cartItem.ProductId);
        if (!productExists)
        {
            _logger.LogError("The product id of \"{ProductId}\" does not exist in the database.", cartItem.ProductId);
            return BadRequest("Invalid product id specified.");
        }

        _logger.LogDebug("Create CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", cartItem.UserId, cartItem.ProductId);
        if (!(await _shoppingServices.CartItem.TryCreateAsync(cartItem)).TryOut(out var errMsgs))
        {
            _messageHandler.Add(TempData, errMsgs!);
            return View(cidto);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminCartItemCreateDto cidto)
    {
        _logger.LogInformation("POST: Admin/Cart/Edit.");

        if (!ModelState.IsValid)
        {
            _logger.LogError("ModeState is invalid");
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Failed to change count of product in cart" });

            return RedirectToAction(nameof(Index));
        }

        var cartItem = _mapper.Map<CartItem>(cidto);

        _logger.LogDebug("Update CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", cartItem.UserId, cartItem.ProductId);
        if (!(await _shoppingServices.CartItem.TryUpdateAsync(cartItem)).TryOut(out var errMsgs))
        {
            _messageHandler.Add(TempData, errMsgs!);
            return View(cidto);
        }

        return RedirectToAction(nameof(Index));

    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(AdminCartItemCreateDto cidto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogError("ModeState is invalid");
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Failed to remove product from cart" });

            return RedirectToAction(nameof(Index));
        }

        _logger.LogInformation("POST: Admin/Cart/Delete.");

        var cartItem = _mapper.Map<CartItem>(cidto);

        _logger.LogDebug("Delete CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", cartItem.UserId, cartItem.ProductId);
        if (!(await _shoppingServices.CartItem.TryDeleteAsync(cartItem)).TryOut(out var errMsgs))
        {
            _messageHandler.Add(TempData, errMsgs!);
            return View(cidto);
        }

        return RedirectToAction(nameof(Index));
    }

}
