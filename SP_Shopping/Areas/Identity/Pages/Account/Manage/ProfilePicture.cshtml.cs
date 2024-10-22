// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using SP_Shopping.Models;
using SP_Shopping.Utilities;
using static SP_Shopping.Utilities.FileSignatureResolver;

namespace SP_Shopping.Areas.Identity.Pages.Account.Manage;

public class ProfilePictureModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDefaultingImageHandler<IdentityUser> _userImageHandler;
    private const int MAX_FILESIZE_BYTE = 1_500_000;

    public ProfilePictureModel
    (
        UserManager<ApplicationUser> userManager,
        IDefaultingImageHandler<IdentityUser> userImageHandler
    )
    {
        _userManager = userManager;
        _userImageHandler = userImageHandler;
    }

    [TempData]
    public string StatusMessage { get; set; }

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
    [RequestSizeLimit(MAX_FILESIZE_BYTE)]
    public class InputModel
    {
        /// <summary>
        ///     My code that I added to ASP.NET Core Identity Scaffold.
        /// </summary>
        [Display(Name = "New Profile Picture")]
        [DataType(DataType.Upload)]
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

    public async Task<IActionResult> OnPostAsync()
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

        if (Input.NewProfilePicture is null)
        {
            _userImageHandler.DeleteImage(user);
            StatusMessage = "Your profile picture has been reset to the default.";
            return RedirectToPage();
        }

        if (!Input.NewProfilePicture.ContentType.Contains("image"))
        {
            StatusMessage = "File has to be an image.";
            return BadRequest("File has to be an image.");
        }

        if (Input.NewProfilePicture.Length > MAX_FILESIZE_BYTE)
        {
            StatusMessage = $"Cannot upload images larger than {MAX_FILESIZE_BYTE} bytes to the database.";
            return BadRequest($"Cannot upload images larger than {MAX_FILESIZE_BYTE} bytes to the database.");   
        }

        using var imageStream = Input.NewProfilePicture.OpenReadStream();

        if (!await _userImageHandler.SetImageAsync(user, imageStream))
        {
            StatusMessage = "Image is not of valid format.";
            return BadRequest("Image is not of valid format.");
        }

        StatusMessage = "Your account description has been updated.";
        return RedirectToPage();
    }

}
