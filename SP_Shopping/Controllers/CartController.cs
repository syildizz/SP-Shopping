﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Dtos.Cart;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Service;
using SP_Shopping.Utilities;
using SP_Shopping.Utilities.MessageHandler;
using System.Security.Claims;

namespace SP_Shopping.Controllers;

[Authorize]
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
    private readonly CartItemService _cartItemService = new CartItemService(cartItemRepository);

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int id)
    {
        _logger.LogInformation("POST: Cart/Create.");

        _logger.LogDebug("Checking if product with \"{ProductId}\" exists in database.", id);
        bool productExists = await _productRepository.ExistsAsync(q => q.Where(p => p.Id == id));

        if (!productExists)
        {
            _logger.LogError("The product id of \"{ProductId}\" does not exist in the database.", id);
            return BadRequest("Invalid product id specified.");
        }

        CartItem cartItem = new()
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            ProductId = id,
            Count = 0
        };

        _logger.LogDebug("Create CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", cartItem.UserId, cartItem.ProductId);
        if (!(await _cartItemService.TryCreateAsync(cartItem)).TryOut(out var errMsgs))
        {
            _messageHandler.Add(TempData, errMsgs!);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("POST: Cart/Delete.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        CartItem cartItem = new() { ProductId = id, UserId = userId };
        _logger.LogDebug("Delete CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", userId, id);
        if (!(await _cartItemService.TryDeleteAsync(cartItem)).TryOut(out var errMsgs))
        {
            _messageHandler.Add(TempData, errMsgs!);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CartItemCreateDto cidto)
    {
        _logger.LogInformation("POST: Cart/Edit.");

        if (ModelState.IsValid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            CartItem cartItem = new() { ProductId = id, UserId = userId, Count = cidto.Count };
            _logger.LogDebug("Update CartItem in the database for user of id \"{UserId}\" and for product of id \"{ProductId}\".", userId, id);
            if (!(await _cartItemService.TryUpdateAsync(cartItem)).TryOut(out var errMsgs))
            {
                _messageHandler.Add(TempData, errMsgs!);
            }
        }
        else
        {
            _logger.LogError("ModelState is invalid");
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Failed to change count of product in cart" });
        }

        return RedirectToAction(nameof(Index));
    }

}
