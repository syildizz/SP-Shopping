using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Dtos;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.Message;
using System.Security.Claims;

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
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            _messageHandler.AddMessages(TempData, [new Message { Type = Message.MessageType.Info, Content = "User is already admin" }]);
            return RedirectToAction(nameof(Index), new { id });
        }

        var result = await _userManager.AddToRoleAsync(user, "Admin");

        if (!result.Succeeded)
        {
            _messageHandler.AddMessages(TempData, [new Message { Type = Message.MessageType.Error, Content = "Failed to make user admin" }]);
            return RedirectToAction(nameof(Index), new { id });
        }

        if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == id)
        {
            await _signInManager.RefreshSignInAsync(user);
        }

        _messageHandler.AddMessages(TempData, [new Message { Type = Message.MessageType.Success, Content = "Succesfully adminized" }]);

        return RedirectToAction(nameof(Index), new { id });

    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unadminize(string id)
    {
        var user = await _userRepository.GetByKeyAsync(id);
        
        if (user is null) return NotFound("User is not found");
        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            _messageHandler.AddMessages(TempData, [new Message { Type = Message.MessageType.Info, Content = "User is already not admin" }]);
            return RedirectToAction(nameof(Index), new { id });
        }

        var result = await _userManager.RemoveFromRoleAsync(user, "Admin");

        if (!result.Succeeded)
        {
            _messageHandler.AddMessages(TempData, [new Message { Type = Message.MessageType.Error, Content = "Failed to make user not admin" }]);
            return RedirectToAction(nameof(Index), new { id });
        }

        if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == id)
        {
            await _signInManager.RefreshSignInAsync(user);
        }

        _messageHandler.AddMessages(TempData, [new Message { Type = Message.MessageType.Success, Content = "Succesfully unadminized" }]);
        return RedirectToAction(nameof(Index), new { id });
    }

}
