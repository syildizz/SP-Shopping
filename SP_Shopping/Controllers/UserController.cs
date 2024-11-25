using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Dtos.User;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.Filter;
using SP_Shopping.Utilities.MessageHandler;

namespace SP_Shopping.Controllers;
public class UserController
(
    IRepository<ApplicationUser> userRepository,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IMessageHandler messageHander,
	ILogger<UserController> logger,
	IMapper mapper
) : Controller
{

    private readonly IRepository<ApplicationUser> _userRepository = userRepository;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IMessageHandler _messageHandler = messageHander;
    private readonly ILogger<UserController> _logger = logger;
    private readonly IMapper _mapper = mapper;

    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Index(string? id)
    {
        _logger.LogInformation("GET: Entering User/Index");

        if (id is null)
        {
            _logger.LogError("Id is null");
            return BadRequest("Id is null");
        }

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

}
