using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Dtos.User;
using SP_Shopping.Service;
using SP_Shopping.ServiceDtos.Product;
using SP_Shopping.Utilities.Filters;
using System.Linq;
using System.Linq.Expressions;

namespace SP_Shopping.Controllers;
public class UserController
(
	ILogger<UserController> logger,
	IMapper mapper,
    IShoppingServices shoppingServices
) : Controller
{
    private readonly ILogger<UserController> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IShoppingServices _shoppingServices = shoppingServices;

    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Index(string? id)
    {
        _logger.LogInformation("GET: Entering User/Index");

        UserPageDto? udto = await _shoppingServices.User.GetByIdAsync(id!, u => new UserPageDto
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            Description = u.Description,
            InsertionDate = u.InsertionDate,
            RoleNames = u.Roles.Select(r => r.ToString()).ToList(),
            ProductDetails = u.Products == null ? null : u.Products.Select(p => new UserPageDto.UserPageProductDto
            {
                Id = p.Id,
                CategoryName = p.Category.Name,
                Name = p.Name,
                Price = p.Price
            })
        });

        if (udto is null)
        {
            _logger.LogError("The user with the id of \"{Id}\" does not exist", id);
            return NotFound("The user does not exist");
        }

        return View(udto);
    }

}
