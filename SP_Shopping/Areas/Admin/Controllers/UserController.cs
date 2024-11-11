using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Areas.Admin.Dtos.User;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.ImageValidator;
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

    public async Task<IActionResult> Index(string? query, string? type, [FromQuery] bool? sort)
    {
        _logger.LogInformation("GET: Entering Admin/Products.");

        Func<IQueryable<AdminUserDetailsDto>, IQueryable<AdminUserDetailsDto>> queryFilter = q => q;
        Func<IQueryable<AdminUserDetailsDto>, IQueryable<AdminUserDetailsDto>> sortFilter = q => q
            .OrderByDescending(p => p.InsertionDate);

        try
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                if (!string.IsNullOrWhiteSpace(query))
                {
                    queryFilter = type switch
                    {
                        nameof(AdminUserDetailsDto.Id) => q => q.Where(u => u.Id.Contains(query)),
                        nameof(AdminUserDetailsDto.UserName) => q => q.Where(u => u.UserName.Contains(query)),
                        nameof(AdminUserDetailsDto.PhoneNumber) => q => q.Where(u => u.PhoneNumber.Contains(query)),
                        nameof(AdminUserDetailsDto.Email) => q => q.Where(u => u.Email.Contains(query)),
                        nameof(AdminUserDetailsDto.Roles) => q => q.Where(u => u.Roles.Aggregate(" ", (acc, curr) => acc + curr).Contains(query)),
                        nameof(AdminUserDetailsDto.Description) => q => q.Where(u => u.Description.Contains(query)),
                        nameof(AdminUserDetailsDto.InsertionDate) => q => q.Where(u => u.InsertionDate.ToString().Contains(query)),
                        _ => throw new NotImplementedException($"{type} is invalid")
                    };
                }

                sort ??= false;
                sortFilter = type switch
                {
                    nameof(AdminUserDetailsDto.Id) => (bool)sort ? q => q.OrderBy(u => u.Id) : q => q.OrderByDescending(u => u.Id),
                    nameof(AdminUserDetailsDto.UserName) => (bool)sort ? q => q.OrderBy(u => u.UserName) : q => q.OrderByDescending(u => u.UserName),
                    nameof(AdminUserDetailsDto.PhoneNumber) => (bool)sort ? q => q.OrderBy(u => u.PhoneNumber) : q => q.OrderByDescending(u => u.PhoneNumber),
                    nameof(AdminUserDetailsDto.Email) => (bool)sort ? q => q.OrderBy(u => u.Email) : q => q.OrderByDescending(u => u.Email),
                    nameof(AdminUserDetailsDto.Roles) => (bool)sort ? q => q.OrderBy(u => u.Roles) : q => q.OrderByDescending(u => u.Roles),
                    nameof(AdminUserDetailsDto.Description) => (bool)sort ? q => q.OrderBy(u => u.Description) : q => q.OrderByDescending(u => u.Description),
                    nameof(AdminUserDetailsDto.InsertionDate) => (bool)sort ? q => q.OrderBy(u => u.InsertionDate) : q => q.OrderByDescending(u => u.InsertionDate),
                    _ => throw new NotImplementedException($"{type} is invalid")
                };
            }
        }
        catch (NotImplementedException ex)
        {
            return BadRequest(ex.Message);
        }

        _logger.LogDebug("Fetching product information matching search term.");
        var pdtoList = await _userRepository.GetAllAsync(q =>
            _mapper.ProjectTo<AdminUserDetailsDto>(q)
                .Take(20)
                ._(queryFilter)
                ._(sortFilter)
        );


        foreach (var pdto in pdtoList)
        {
            pdto.Roles = (List<string>) await _userManager.GetRolesAsync(_mapper.Map<ApplicationUser>(pdto));
        }


        return View(pdtoList);
        
    }

    public async Task<IActionResult> Edit(string id)
    {
        _logger.LogInformation("GET: Entering Admin/Edit.");

        var udto = await _userRepository.GetSingleAsync(q => 
            _mapper.ProjectTo<AdminUserEditDto>(q
                .Where(u => u.Id == id)
            )
        );

        if (udto is null)
        {
            return NotFound("The user is not found");
        }

        udto.Roles = (await _userManager.GetRolesAsync(_mapper.Map<ApplicationUser>(udto))).Aggregate("", (acc, curr) => acc + " " + curr, fin => fin.Trim());
        return View(udto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, AdminUserEditDto udto)
    {
        _logger.LogInformation("POST: Entering Admin/Edit.");

        var user = await _userRepository.GetSingleAsync(q => q.Where(u => u.Id == id));

        if (user is null)
        {
            return NotFound("The user is not found");
        }

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

            if (!string.IsNullOrWhiteSpace(udto.Roles))
            {
                var userRoles = (await _userManager.GetRolesAsync(user)).Select(s => s.ToUpperInvariant());
                var requestRoles = udto.Roles.Split(' ').Where(r => !string.IsNullOrWhiteSpace(r)).Select(s => s.ToUpperInvariant());
                var userDifferenceRequestRoles = userRoles.Except(requestRoles);
                var requestDifferenceUserRoles = requestRoles.Except(userRoles);

                var wentToCatch = false;
                var errorMessage = "Failed to set roles";
                try
                {
                    result = await _userManager.AddToRolesAsync(user, requestDifferenceUserRoles);
                }
                catch (InvalidOperationException ex) { wentToCatch = true; errorMessage = ex.Message; }
                if (wentToCatch || !result.Succeeded)
                {
                    _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = errorMessage });
                    return false;
                }

                wentToCatch = false;
                try
                {
                    result = await _userManager.RemoveFromRolesAsync(user, userDifferenceRequestRoles);
                }
                catch (InvalidOperationException ex) { wentToCatch = true; errorMessage = ex.Message; }
                if (wentToCatch || !result.Succeeded)
                {
                    _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = errorMessage });
                    return false;
                }
            }
            else
            {
                var wentToCatch = false;
                var errorMessage = "Failed to set roles";
                try
                {
                    result = await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));
                }
                catch (InvalidOperationException ex) { wentToCatch = true; errorMessage = ex.Message; }
                if (wentToCatch || !result.Succeeded)
                {
                    _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = errorMessage });
                    return false;
                }
            }

            int result2 = await _userRepository.UpdateCertainFieldsAsync(q => q
                .Where(u => u.Id == udto.Id),
                s => s
                    .SetProperty(u => u.Description, udto.Description)
                    // Confirm e-mail and phone number when changed by admin.
                    // Maybe change this functionality in the future idk.
                    // TODO: Add panels in admin panel to confirm / unconfirm user email / phone number
                    .SetProperty(u => u.EmailConfirmed, true)
                    .SetProperty(u => u.PhoneNumberConfirmed, true)
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
                    _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = ivResult.DefaultMessage });
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
