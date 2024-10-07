﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using NuGet.Common;
using SP_Shopping.Data;
using SP_Shopping.Dtos;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SP_Shopping.Controllers;

public static class Glob
{
    public static readonly Dictionary<string, CancellationTokenSource> userCancellationTokens = [];
    public static CancellationTokenSource? detailsCancellationToken = new(new TimeSpan(6,0,0));
}

public class CartController
(
    IMapper mapper,
    IRepository<CartItem> cartItemRepository,
    IRepository<ApplicationUser> userRepository,
    IRepository<Product> productRepository,
    IMemoryCache memoryCache
) : Controller
{

    private readonly IMapper _mapper = mapper;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly IRepository<CartItem> _cartItemRepository = cartItemRepository;
    private readonly IRepository<ApplicationUser> _userRepository = userRepository;
    private readonly IRepository<Product> _productRepository = productRepository;

    [Authorize]
    public async Task<IActionResult> Index()
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

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return View("Error");
        }
        string cacheKey = $"CartItemsIndex_{userId}";

        // Generate Cancellation Token
        if (!Glob.userCancellationTokens.TryGetValue(userId, out CancellationTokenSource? cts))
        {
            cts = new CancellationTokenSource(new TimeSpan(6,0,0));
            Glob.userCancellationTokens.Add(userId, cts);
        }

        IEnumerable<CartItem>? cartItems = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AddExpirationToken(new CancellationChangeToken(cts.Token));
            return await _cartItemRepository.GetAllAsync(q => q
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .Select(c => new CartItem()
                {
                    ProductId = c.ProductId,
                    UserId = c.User.Id,
                    Product = new Product()
                    {
                        Name = c.Product.Name
                    }
                })
            );
        });

        var cidtos = _mapper.Map<IEnumerable<CartItem>,IEnumerable<CartItemDetailsDto>>(cartItems);

        return View(cidtos);
    }
    [Route($"/Cart/Index/{{{nameof(id)}}}")]
    public async Task<IActionResult> Index(string? id)
    {
        if (!string.IsNullOrEmpty(id) && Regex.IsMatch(id, @"[sS]elf", RegexOptions.Compiled))
        {
            return Redirect($"/Cart/{nameof(Index)}");
        }
        if (string.IsNullOrWhiteSpace(id) || !await _userRepository.ExistsAsync(q => q.Where(u => u.Id == id)))
        {
            return NotFound("The user was not found");
        }

        // Generate Cancellation Token
        if (!Glob.userCancellationTokens.TryGetValue(id, out CancellationTokenSource? cts))
        {
            cts = new CancellationTokenSource(new TimeSpan(6,0,0));
            Glob.userCancellationTokens.Add(id, cts);
        }

        string cacheKey = $"CartItemsIndex_{id}";
        IEnumerable<CartItem>? cartItems = await _memoryCache.GetOrCreateAsync($"CartItemsIndex_{id}", async entry =>
        {
            entry.AddExpirationToken(new CancellationChangeToken(cts.Token));
            return await _cartItemRepository.GetAllAsync(q => q
                .Where(c => c.UserId == id)
                .Include(c => c.Product)
                .Select(c => new CartItem()
                {
                    ProductId = c.ProductId,
                    UserId = c.User.Id,
                    Product = new Product()
                    {
                        Name = c.Product.Name
                    }
                })
            );
        });

        IEnumerable<CartItemDetailsDto> cidto = _mapper.Map<IEnumerable<CartItem>, IEnumerable<CartItemDetailsDto>>(cartItems);

        ViewBag.Message = $"The shopping cart of {(await _userRepository.GetByKeyAsync(id))?.UserName ?? "User not found"}";

        return View(cidto);
    }

    public async Task<IActionResult> Details()
    {

        if (Glob.detailsCancellationToken == null)
        {
            Glob.detailsCancellationToken = new CancellationTokenSource(new TimeSpan(6,0,0));
        }
        IEnumerable<CartItem>? cartItems = await _memoryCache.GetOrCreateAsync($"CartItemDetails", async entry =>
        {
            entry.AddExpirationToken(new CancellationChangeToken(Glob.detailsCancellationToken.Token));
            entry.RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                Console.Error.WriteLine($"Cache invalidation due to {reason} - {key} : {value}");
            });
            return await _cartItemRepository.GetAllAsync(q => q
                .Include(c => c.Product)
                .Include(c => c.User)
                .Select(c => new CartItem()
                {
                    ProductId = c.ProductId,
                    UserId = c.User.Id,
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
        });

        IEnumerable<CartItemDetailsDto> cidto = _mapper.Map<IEnumerable<CartItem>, IEnumerable<CartItemDetailsDto>>(cartItems);


        ViewBag.Message = $"The shopping carts of all users";

        return View(cidto);
    }

    [HttpPost()]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CartItemCreateDto cidto, string? returnPath)
    {
        CartItem cartItem = _mapper.Map<CartItemCreateDto, CartItem>(cidto);
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool userExists = await _userRepository.ExistsAsync(q => q.Where(u => u.Id == userId));
        bool productExists = await _productRepository.ExistsAsync(q => q.Where(p => p.Id == cartItem.ProductId));
        // Check that the keys are valid.
        if (string.IsNullOrWhiteSpace(userId) || !userExists || !productExists)
        {
            return NotFound("Product and user specified does not exist..");
        }

        cartItem.UserId = userId;

        try
        {
            await _cartItemRepository.CreateAsync(cartItem);
        }
        catch (DbUpdateException)
        { }

        //Expire cache
        if (Glob.userCancellationTokens.TryGetValue(userId, out var token))
        {
            await token.CancelAsync();
            Glob.userCancellationTokens.Remove(userId);
        }
        await Glob.detailsCancellationToken!.CancelAsync();

        return Redirect(returnPath ?? nameof(Index));
    }

    [HttpPost()]
    [ActionName("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(CartItemDetailsDto cidto)
    {

        if (cidto.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return Unauthorized("You cannot only delete products from your own shopping cart");
        }

        var cartItem = _mapper.Map<CartItemDetailsDto, CartItem>(cidto);
        await _cartItemRepository.DeleteCertainEntriesAsync(q => q
            .Where(c => c.UserId == cartItem.UserId && c.ProductId == cartItem.ProductId)
        );

        if (Glob.userCancellationTokens.TryGetValue(cidto.UserId, out var token))
        {
            await token.CancelAsync();
            Glob.userCancellationTokens.Remove(cidto.UserId);
        }

        await Glob.detailsCancellationToken!.CancelAsync();

        return Redirect(nameof(Index));
    } 

}
