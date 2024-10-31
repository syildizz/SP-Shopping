using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Dtos;
using SP_Shopping.Models;
using SP_Shopping.Repository;

namespace SP_Shopping.Controllers;
public class UserController
(
    IRepository<ApplicationUser> userRepository,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
	ILogger<UserController> logger,
	IMapper mapper
) : Controller
{

    private readonly IRepository<ApplicationUser> _userRepository = userRepository;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly ILogger<UserController> _logger = logger;
    private readonly IMapper _mapper = mapper;

    public async Task<IActionResult> Index(string id)
    {
        _logger.LogInformation("GET: Entering User/Index");
        UserPageDto? udto = await _userRepository.GetSingleAsync(q => 
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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adminize(string id)
    {
        var user = await _userRepository.GetByKeyAsync(id);

        if (user is null) return NotFound("User is not found");
        if (await _userManager.IsInRoleAsync(user, "Admin")) return Ok("Already admin");

        var result = await _userManager.AddToRoleAsync(user, "Admin");

        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status418ImATeapot, "Kur hata");
        }

        await _signInManager.RefreshSignInAsync(user);

        return Ok("Successfully authenticated");

    }

}
