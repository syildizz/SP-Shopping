using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Models;
using SP_Shopping.Repository;

namespace SP_Shopping.Controllers;
public class UserController
(
    IRepository<ApplicationUser> userRepository,
	ILogger<UserController> logger,
	IMapper mapper
) : Controller
{

    private readonly IRepository<ApplicationUser> _userRepository = userRepository;
    private readonly ILogger<UserController> _logger = logger;
    private readonly IMapper _mapper = mapper;

    public async Task<IActionResult> Index(string id)
    {

        _logger.LogInformation("GET: Entering User/Index");
        ApplicationUser? user = await _userRepository.GetSingleAsync(q => q
            .Where(u => u.Id == id)
            .Select(u => new ApplicationUser()
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Products = (u.Products == null)
                    ? null 
                    : (List<Product>)u.Products.Select(p => new Product
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        Category = new Category() { Name = p.Category.Name }
                    })
            })
        );


        /*
        UserDetailsDto:
            user.Id
            user.UserName
            user.Email
            user.ProductDetails: List<UserProductDetailsDto>
        UserProductDetailsDto:
            product.Id
            product.Name
            product.Price
            product.Category
        */

        if (user is null)
        {
            _logger.LogError("The user with the id of \"{Id}\" does not exist", id);
            return NotFound("The user does not exist");
        }

        return View(user);
    }
}
