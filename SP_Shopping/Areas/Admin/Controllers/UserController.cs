using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Areas.Admin.Dtos.User;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Service;
using SP_Shopping.Utilities;
using SP_Shopping.Utilities.Filter;
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
    IRepository<ApplicationUser> userRepository,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IMessageHandler messageHandler,
	ILogger<UserController> logger,
	IMapper mapper,
    IImageHandlerDefaulting<UserProfileImageKey> profileImageHandler,
    UserService userService
) : Controller
{

    private readonly IRepository<ApplicationUser> _userRepository = userRepository;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IMessageHandler _messageHandler = messageHandler;
    private readonly ILogger<UserController> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IImageHandlerDefaulting<UserProfileImageKey> _profileImageHandler = profileImageHandler;
    private readonly UserService _userService = userService;

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

    [IfArgNullBadRequestFilter(nameof(id))]
    [ImportModelState]
    public async Task<IActionResult> Edit(string? id)
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
    [ExportModelState]
    public async Task<IActionResult> Edit(AdminUserEditDto udto)
    {
        _logger.LogInformation("POST: Entering Admin/Edit.");

        if (ModelState.IsValid)
        {
            var user = _mapper.Map<ApplicationUser>(udto);

            if (user is null)
            {
                return NotFound("The user is not found");
            }


            if (!(await _userService.TryUpdateAsync(user, udto.ProfilePicture, udto.Roles?.Split(' '))).TryOut(out var errMsgs))
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
        var user = await _userRepository.GetSingleAsync(q => q
            .Where(u => u.Id == id)
        );

        if (user is not null)
        {
            if (!(await _userService.TryDeleteAsync(user)).TryOut(out var errMsgs))
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

        return RedirectToAction("Index", "User", new { id, area = "" });

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Unadminize(string? id)
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
        return RedirectToAction("Index", "User", new { id, area = "" });
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> ResetImage(string? id)
    {
        _logger.LogInformation($"POST: Entering Admin/User/ResetImage.");
        _logger.LogDebug("Fetching user for id \"{Id}\".", id);
        var userExists = await _userRepository.ExistsAsync(q => q
            .Where(p => p.Id == id)
        );

        if (!userExists)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound($"The product with the passed id of \"{id}\" does not exist.");
        }

        _logger.LogDebug("Deleting image for product with id \"{Id}\"", id);
        _profileImageHandler.DeleteImage(new(id!));

        return RedirectToAction(nameof(Edit), new { id });
        
    }

}
