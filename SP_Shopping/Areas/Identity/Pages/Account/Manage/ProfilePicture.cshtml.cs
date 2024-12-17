// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SP_Shopping.Models;
using SP_Shopping.Utilities.Attributes;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;

namespace SP_Shopping.Areas.Identity.Pages.Account.Manage;

public class ProfilePictureModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IImageHandlerDefaulting<UserProfileImageKey> _userProfileImageHandler;
    private readonly IMessageHandler _messageHandler;

    public ProfilePictureModel
    (
        UserManager<ApplicationUser> userManager,
        IImageHandlerDefaulting<UserProfileImageKey> userProfileImageHandler,
        IMessageHandler messageHandler
    )
    {
        _userManager = userManager;
        _userProfileImageHandler = userProfileImageHandler;
        _messageHandler = messageHandler;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     My code that I added to ASP.NET Core Identity Scaffold.
        /// </summary>
        [Display(Name = "New Profile Picture")]
        [DataType(DataType.Upload)]
        [IsImageFile]
        public IFormFile NewProfilePicture { get; set; }
    }

    //private async Task LoadAsync(ApplicationUser user)
    //{ }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        //await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string button)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (button == "Reset")
        {
            _userProfileImageHandler.DeleteImage(new(user.Id));
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Success, Content = "Your profile picture has been reset to the default." });;
            return RedirectToPage();
        }

        if (Input.NewProfilePicture is null)
        {
            return RedirectToPage();
        }

        using var imageStream = Input.NewProfilePicture.OpenReadStream();

        if (!await _userProfileImageHandler.SetImageAsync(new(user.Id), imageStream))
        {
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content =  "Image is not of valid format." });;
            return RedirectToPage();
        }

        _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Success, Content =  "Your account description has been updated." });;
        return RedirectToPage();
    }

}
