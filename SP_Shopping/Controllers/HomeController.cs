using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Dtos.Product;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.ImageHandler;
using System.Diagnostics;
using System.Security.Claims;

namespace SP_Shopping.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRepository<Product> _productRepository;
    private readonly IMapper _mapper;

    public HomeController(ILogger<HomeController> logger, IRepository<Product> productRepository, IMapper mapper)
    {
        _logger = logger;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewBag.UserName = User.FindFirstValue(ClaimTypes.Name);
        IEnumerable<ProductDetailsDto> pdto = await _productRepository.GetAllAsync(q => 
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
