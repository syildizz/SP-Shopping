using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Areas.Admin.Dtos.User;
using SP_Shopping.Models;
using SP_Shopping.Service;
using SP_Shopping.ServiceDtos.User;
using SP_Shopping.Utilities;
using SP_Shopping.Utilities.Filters;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using SP_Shopping.Utilities.ModelStateHandler;
using System.Security.Claims;

namespace SP_Shopping.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UserController
(
	ILogger<UserController> logger,
	IMapper mapper,
    IShoppingServices shoppingServices,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IMessageHandler messageHandler,
    IImageHandlerDefaulting<UserProfileImageKey> profileImageHandler
) : Controller
{

    private readonly ILogger<UserController> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IShoppingServices _shoppingServices = shoppingServices;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IMessageHandler _messageHandler = messageHandler;
    private readonly IImageHandlerDefaulting<UserProfileImageKey> _profileImageHandler = profileImageHandler;

    public async Task<IActionResult> Index(string? query, string? type, [FromQuery] bool? sort)
    {
        _logger.LogInformation("GET: Entering Admin/User.");

        Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> queryFilter = q => q;
        Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> sortFilter = q => q
            .OrderByDescending(p => p.InsertionDate);

        try
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                if (!string.IsNullOrWhiteSpace(query))
                {
                    queryFilter = type switch
                    {
                        nameof(ApplicationUser.Id) => 
                            q => q.Where(u => u.Id.Contains(query)),
                        nameof(ApplicationUser.UserName) => 
                            q => q.Where(u => u.UserName != null 
                                           && u.UserName.Contains(query)),
                        nameof(ApplicationUser.PhoneNumber) => 
                            q => q.Where(u => u.PhoneNumber != null 
                                           && u.PhoneNumber.Contains(query)),
                        nameof(ApplicationUser.Email) => 
                            q => q.Where(u => u.Email != null 
                                           && u.Email.Contains(query)),
                        nameof(ApplicationUser.Roles) => 
                            q => q.Where(u => u.Roles.Select(r => r.Name)
                                  .Where(r => !string.IsNullOrWhiteSpace(r)).Contains(query)),
                        nameof(ApplicationUser.Description) => 
                            q => q.Where(u => u.Description != null 
                                           && u.Description.Contains(query)),
                        nameof(ApplicationUser.InsertionDate) => 
                            q => q.Where(u => u.InsertionDate.ToString().Contains(query)),
                        _ => throw new NotImplementedException($"{type} is invalid")
                    };
                }

                sort ??= false;
                sortFilter = type switch
                {
                    nameof(AdminUserDetailsDto.Id) => (bool)sort
                        ? q => q.OrderBy(u => u.Id)
                        : q => q.OrderByDescending(u => u.Id),
                    nameof(AdminUserDetailsDto.UserName) => (bool)sort
                        ? q => q.OrderBy(u => u.UserName)
                        : q => q.OrderByDescending(u => u.UserName),
                    nameof(AdminUserDetailsDto.PhoneNumber) => (bool)sort
                        ? q => q.OrderBy(u => u.PhoneNumber)
                        : q => q.OrderByDescending(u => u.PhoneNumber),
                    nameof(AdminUserDetailsDto.Email) => (bool)sort
                        ? q => q.OrderBy(u => u.Email)
                        : q => q.OrderByDescending(u => u.Email),
                    nameof(AdminUserDetailsDto.Roles) => 
                        sortFilter,
                    nameof(AdminUserDetailsDto.Description) => (bool)sort
                        ? q => q.OrderBy(u => u.Description)
                        : q => q.OrderByDescending(u => u.Description),
                    nameof(AdminUserDetailsDto.InsertionDate) => (bool)sort
                        ? q => q.OrderBy(u => u.InsertionDate)
                        : q => q.OrderByDescending(u => u.InsertionDate),
                    _ => throw new NotImplementedException($"{type} is invalid")
                };
            }
        }
        catch (NotImplementedException ex)
        {
            return BadRequest(ex.Message);
        }

        _logger.LogDebug("Fetching product information matching search term.");
        //var pdtoList = await _shoppingServices.User.GetAllAsync(q =>
        //    _mapper.ProjectTo<AdminUserDetailsDto>(q
        //        ._(queryFilter)
        //        ._(sortFilter)
        //        .Take(20)
        //    )
        //);
        //TODO: write actual method
        var pdtoList = (List<AdminUserDetailsDto>)[];

        return View(pdtoList);
        
    }

    [IfArgNullBadRequestFilter(nameof(id))]
    [ImportModelState]
    public async Task<IActionResult> Edit(string? id)
    {
        _logger.LogInformation("GET: Entering Admin/Edit.");

        var udto = await _shoppingServices.User.GetByIdAsync(id!);

        if (udto is null)
        {
            return NotFound("The user is not found");
        }

        return View(udto);
    }

    [HttpPost]
    [ExportModelState]
    public async Task<IActionResult> Edit(AdminUserEditDto udto)
    {
        _logger.LogInformation("POST: Entering Admin/Edit.");

        if (ModelState.IsValid)
        {
            using var user = new UserEditDto
            {
                UserName = udto.UserName,
                Password = null,
                Email = udto.Email,
                PhoneNumber = udto.PhoneNumber,
                Roles = await _shoppingServices.Role.GetAllAsync(q => q
                .Where(r => r.NormalizedName != null && udto.Roles.Contains(r.NormalizedName))),
                Description = udto.Description,
                Image = udto.ProfilePicture?.OpenReadStream()
            };

            if (user is null)
            {
                return NotFound("The user is not found");
            }

            if (!(await _shoppingServices.User.TryUpdateAsync(udto.Id, user)).TryOut(out var errMsgs))
            {
               _messageHandler.Add(TempData, errMsgs!); 
               return RedirectToAction("Edit", new { id = udto.Id });
            }
           return RedirectToAction(nameof(Index));
        }
        else
        {
            return RedirectToAction(nameof(Edit), new { id = udto.Id });
        }

    }

    [HttpPost]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Delete(string? id)
    {
        var user = await _shoppingServices.User.GetByIdAsync(id!);
        if (user is not null)
        {
            if (!(await _shoppingServices.User.TryDeleteAsync(id!)).TryOut(out var errMsgs))
            {
                _messageHandler.Add(TempData, errMsgs!);
               return RedirectToAction("Index");
            }
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Adminize(string? id)
    {
        _logger.LogInformation("GET: Entering Admin/User/Adminize");

        var udto = await _shoppingServices.User.GetByIdAsync(id!);

        if (udto is null) return NotFound("User is not found");

        var user = Utilities.Mappers.MapToApplicationUser.From(udto);

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

        return RedirectToAction("Index", "User", new { id, area = "" });

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Unadminize(string? id)
    {
        _logger.LogInformation("GET: Entering Admin/User/Unadminize");

        var udto = await _shoppingServices.User.GetByIdAsync(id!);

        if (udto is null) return NotFound("User is not found");

        var user = Utilities.Mappers.MapToApplicationUser.From(udto);
        
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
        return RedirectToAction("Index", "User", new { id, area = "" });
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> ResetImage(string? id)
    {
        _logger.LogInformation($"POST: Entering Admin/User/ResetImage.");
        _logger.LogDebug("Fetching user for id \"{Id}\".", id);
        var userExists = await _shoppingServices.User.ExistsAsync(id!);
        if (!userExists)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound($"The product with the passed id of \"{id}\" does not exist.");
        }

        _logger.LogDebug("Deleting image for product with id \"{Id}\"", id);
        try
        {
            _profileImageHandler.DeleteImage(new(id!));
        }
        catch (Exception ex)
        {
#if DEBUG
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Failed to reset image" + " " + ex.StackTrace });
#else
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Failed to reset image" });
#endif
            return RedirectToAction(nameof(Edit), new { id });
        }

        return RedirectToAction(nameof(Edit), new { id });
        
    }

}
