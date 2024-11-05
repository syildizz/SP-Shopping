﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Dtos;
using SP_Shopping.Models;
using SP_Shopping.Repository;
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
    public async Task<IActionResult> Create(CartItemCreateDto cidto)
    {
        _logger.LogInformation("POST: Cart/Create.");

        if (ModelState.IsValid)
        {
            CartItem cartItem = _mapper.Map<CartItemCreateDto, CartItem>(cidto);
            cartItem.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Check that the keys are valid.

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

        return Redirect(nameof(Index));
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(CartItemDetailsDto cidto)
    {
        _logger.LogInformation("POST: Cart/Delete.");

        if (ModelState.IsValid)
        {
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
        }
        else
        {
            _logger.LogError("ModeState is invalid");
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Failed to remove product from cart" });
        }

        return Redirect(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CartItemCreateDto cidto)
    {
        _logger.LogInformation("POST: Cart/Edit.");

        if (ModelState.IsValid)
        {
            var cartItem = _mapper.Map<CartItemCreateDto, CartItem>(cidto);
            cartItem.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

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

        return Redirect(nameof(Index));
    }

}
