using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SP_Shopping.Areas.Admin.Dtos.User;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.MessageHandler;
using System.Security.Claims;

namespace SP_Shopping.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
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

    //public async Task<IActionResult> Index(string id)
    //{
    //    _logger.LogInformation("GET: Entering Admin/User/Index");
    //    UserPageDto? udto = await _userRepository.GetSingleAsync(q => 
    //        _mapper.ProjectTo<UserPageDto>(q
    //            .Where(u => u.Id == id)
    //        )
    //    );

    //    if (udto is null)
    //    {
    //        _logger.LogError("The user with the id of \"{Id}\" does not exist", id);
    //        return NotFound("The user does not exist");
    //    }

    //    return View(udto);
    //}

    public async Task<IActionResult> Search(string? query, string? type)
    {
        _logger.LogInformation("GET: Entering Admin/Products/Search.");

        Func<IQueryable<AdminUserDetailsDto>, IQueryable<AdminUserDetailsDto>>? userNameFilter;
        if (!string.IsNullOrWhiteSpace(query) && !string.IsNullOrWhiteSpace(type))
        {
            userNameFilter = type switch
            {
                nameof(AdminUserDetailsDto.Id) => q => q.Where(u => u.Id.Contains(query)),
                nameof(AdminUserDetailsDto.UserName) => q => q.Where(u => u.UserName.Contains(query)),
                nameof(AdminUserDetailsDto.PhoneNumber) => q => q.Where(u => u.PhoneNumber.Contains(query)),
                nameof(AdminUserDetailsDto.Email) => q => q.Where(u => u.Email.Contains(query)),
                nameof(AdminUserDetailsDto.Roles) => q => q.Where(u => u.Roles.Aggregate(" ", (acc, curr) => acc + curr).Contains(query)),
                _ => null
            };
        }
        else
        {
            userNameFilter = q => q;
        }

        if (userNameFilter is null)
        {
            return BadRequest("Invalid type");
        }

        _logger.LogDebug("Fetching product information matching search term.");
        var udtoList = await _userRepository.GetAllAsync(q =>
            userNameFilter(_mapper.ProjectTo<AdminUserDetailsDto>(q)
                .OrderByDescending(p => p.UserName)
                .ThenByDescending(p => p.Email)
                .Take(20)
            )
        );
        foreach (var udto in udtoList)
        {
            udto.Roles = (List<string>) await _userManager.GetRolesAsync(_mapper.Map<ApplicationUser>(udto));
        }


        return View(udtoList);
        
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adminize(string id)
    {
        _logger.LogInformation("GET: Entering Admin/User/Adminize");

        var user = await _userRepository.GetByKeyAsync(id);

        if (user is null) return NotFound("User is not found");
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Info, Content = "User is already admin" });
            return RedirectToAction(nameof(Index), new { id });
        }

        var result = await _userManager.AddToRoleAsync(user, "Admin");

        if (!result.Succeeded)
        {
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Failed to make user admin" });
            return RedirectToAction(nameof(Index), new { id });
        }

        if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == id)
        {
            await _signInManager.RefreshSignInAsync(user);
        }

        _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Success, Content = "Succesfully made admin" });

        return RedirectToAction(nameof(Index), new { id });

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unadminize(string id)
    {
        _logger.LogInformation("GET: Entering Admin/User/Unadminize");

        var user = await _userRepository.GetByKeyAsync(id);
        
        if (user is null) return NotFound("User is not found");
        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Info, Content = "User is already not admin" });
            return RedirectToAction(nameof(Index), new { id });
        }

        var result = await _userManager.RemoveFromRoleAsync(user, "Admin");

        if (!result.Succeeded)
        {
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Failed to make user not admin" });
            return RedirectToAction(nameof(Index), new { id });
        }

        if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == id)
        {
            await _signInManager.RefreshSignInAsync(user);
        }

        _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Success, Content = "Succesfully made not admin" });
        return RedirectToAction(nameof(Index), new { id });
    }

}
