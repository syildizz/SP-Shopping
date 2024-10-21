﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SP_Shopping.Models;
using SP_Shopping.Utilities;

namespace SP_Shopping.Areas.Identity.Pages.Account.Manage;

public class ProfilePictureModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserImageHandler _userImageHandler;
    private const int MAX_FILESIZE_BYTE = 1_500_000;

    public ProfilePictureModel
    (
        UserManager<ApplicationUser> userManager,
        IUserImageHandler userImageHandler
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
            _userImageHandler.DeleteProfilePicture(user);
            StatusMessage = "Your profile picture has been reset to the default.";
            return RedirectToPage();
        }

        if (Input.NewProfilePicture.Length > MAX_FILESIZE_BYTE)
        {
            return BadRequest($"Cannot upload images larger than {MAX_FILESIZE_BYTE} bytes to the database.");   
        }

        var formImageData = new byte[Input.NewProfilePicture.Length];
        var imageStream = Input.NewProfilePicture.OpenReadStream();
        var readBytes = await imageStream.ReadAsync(formImageData, 0, (int)Input.NewProfilePicture.Length);

        await _userImageHandler.SetProfilePictureAsync(user, formImageData);

        StatusMessage = "Your account description has been updated.";
        return RedirectToPage();
    }

}
