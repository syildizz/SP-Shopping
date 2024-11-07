using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using SP_Shopping.Areas.Admin.Dtos.User;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.ImageValidator;
using SP_Shopping.Utilities.MessageHandler;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
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
	IMapper mapper,
    IImageHandlerDefaulting<UserProfileImageKey> profileImageHandler
) : Controller
{

    private readonly IRepository<ApplicationUser> _userRepository = userRepository;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IMessageHandler _messageHandler = messageHander;
    private readonly ILogger<UserController> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IImageHandlerDefaulting<UserProfileImageKey> _profileImageHandler = profileImageHandler;
    private readonly IImageValidator _imageValidator = new ImageValidator();

    public async Task<IActionResult> Index(string? query, string? type)
    {
        _logger.LogInformation("GET: Entering Admin/Products.");

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
                nameof(AdminUserDetailsDto.Description) => q => q.Where(u => u.Description.Contains(query)),
                nameof(AdminUserDetailsDto.InsertionDate) => q => q.Where(u => u.InsertionDate.ToString().Contains(query)),
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

    public async Task<IActionResult> Edit(string id)
    {
        _logger.LogInformation("GET: Entering Admin/Edit.");

        var udto = await _userRepository.GetSingleAsync(q => 
            _mapper.ProjectTo<AdminUserEditDto>(q
                .Where(u => u.Id == id)
            )
        );
        return View(udto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, AdminUserEditDto udto)
    {
        _logger.LogInformation("POST: Entering Admin/Edit.");

        var user = await _userRepository.GetSingleAsync(q => q.Where(u => u.Id == id));

        var transactionSucceeded = await _userRepository.DoInTransactionAsync(async () =>
        {
            IdentityResult result;

            result = await _userManager.SetUserNameAsync(user, udto.UserName);
            if (!result.Succeeded)
            {
                _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Failed to set username" });
                return false;
            }

            result = await _userManager.SetEmailAsync(user, udto.Email);
            if (!result.Succeeded)
            {
                _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Failed to set email" });
                return false;
            }

            result = await _userManager.SetPhoneNumberAsync(user, udto.PhoneNumber);
            if (!result.Succeeded)
            {
                _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Failed to set phone number" });
                return false;
            }

            int result2 = await _userRepository.UpdateCertainFieldsAsync(q => q
                .Where(u => u.Id == udto.Id),
                s => s
                    .SetProperty(u => u.Description, udto.Description)
            );

            if (result2 < 1)
            {
                _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Failed to set description" });
                return false;
            }

            if (udto.ProfilePicture is not null)
            {
                var ivResult = _imageValidator.Validate(udto.ProfilePicture);
                if (ivResult.Type is not ImageValidatorResultType.Success)
                {
                    _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Failed to set profile picture" });
                    return false;
                }
                using var imageStream = udto.ProfilePicture.OpenReadStream();
                if (!await _profileImageHandler.SetImageAsync(new(udto.Id), imageStream))
                {
                    _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Failed to set profile picture" });
                    return false;
                }

                return true;
            }

            return true;
        });

       if (transactionSucceeded)
       {
           return RedirectToAction(nameof(Index));
       }
       else
       {
           return RedirectToAction("Edit", new { id = udto.Id });
       }

    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userRepository.GetSingleAsync(q => q
            .Where(u => u.Id == id)
        );

        if (user is not null)
        {
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _profileImageHandler.DeleteImage(new(id));
            }
            else
            {
                _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Failed to remove user" }); 
            }
        }

        return RedirectToAction("Index");
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
