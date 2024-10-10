using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Dtos;
using SP_Shopping.Models;
using SP_Shopping.Repository;
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

    public IActionResult Index()
    {
        ViewBag.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewBag.UserName = User.FindFirstValue(ClaimTypes.Name);
        var products = _productRepository.GetAll(q => q
            .Include(p => p.Submitter)
            .Include(p => p.Category)
            .Select(p => new Product()
            {
                Name = p.Name,
                Price = p.Price,
                Submitter = new ApplicationUser()
                {
                    UserName = p.Submitter!.UserName,
                },
                Category = new Category()
                {
                    Name = p.Category.Name
                }
            })
            .Take(20)
        );
        var pdto = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductDetailsDto>>(products);
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
