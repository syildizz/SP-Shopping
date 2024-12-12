using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Dtos.User;
using SP_Shopping.Service;
using SP_Shopping.Utilities.Filter;

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

        UserPageDto? udto = await _shoppingServices.User.GetSingleAsync(q => 
            _mapper.ProjectTo<UserPageDto>(q
                .Where(u => u.Id == id)
            )
        );

        if (udto is null)
        {
            _logger.LogError("The user with the id of \"{Id}\" does not exist", id);
            return NotFound("The user does not exist");
        }

        return View(udto);
    }

}
