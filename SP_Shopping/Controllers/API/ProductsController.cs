using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Dtos.Product;
using SP_Shopping.Service;
using SP_Shopping.Utilities.Filters;

namespace SP_Shopping.Controllers.API;

[Route("api/[controller]/[action]")]
public class ProductsController
(
    ILogger<ProductsController> logger,
    IMapper mapper,
    IShoppingServices shoppingServices
) : Controller
{
    private readonly ILogger<ProductsController> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IShoppingServices _shoppingServices = shoppingServices;

    [IfArgNullBadRequestFilter(nameof(id))]
    [HttpGet("{id?}")]
    public async Task<IActionResult> ProductCard(int? id)
    {
        _logger.LogInformation($"GET: Entering API/Products/ProductCard.");

        var pdto = await _shoppingServices.Product.GetSingleAsync(q => 
            _mapper.ProjectTo<ProductDetailsDto>(q
                .Where(p => p.Id == id)
            )
        );

        if (pdto is null)
        {
            return NotFound("Not Found");
        }

        return PartialView("_ProductCardPartial", pdto);
    }

}
