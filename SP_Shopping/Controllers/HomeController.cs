using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Dtos.Product;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Service;
using System.Diagnostics;
using System.Security.Claims;

namespace SP_Shopping.Controllers;

public class HomeController
(
    ILogger<HomeController> logger,
    IMapper mapper,
    IShoppingServices shoppingServices
) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IShoppingServices _shoppingServices = shoppingServices;

    public async Task<IActionResult> Index()
    {
        ViewBag.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewBag.UserName = User.FindFirstValue(ClaimTypes.Name);
        IEnumerable<ProductDetailsDto> pdto = await _shoppingServices.Product.GetAllAsync(q => 
            _mapper.ProjectTo<ProductDetailsDto>(q
                .OrderByDescending(p => p.InsertionDate)
                .ThenByDescending(p => p.ModificationDate)
                .Take(20)
            )
        );
        return View(pdto);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
