using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Dtos;
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
