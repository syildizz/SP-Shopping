// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SP_Shopping.Models;

namespace SP_Shopping.Areas.Identity.Pages.Account.Manage;

public class DescriptionModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DescriptionModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
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
        [StringLength(1000, ErrorMessage = "The description can be at maximum 1000 characters long")]
        [Display(Name = "Description")]
        public string Description { get; set; }
    }

    private void Load(ApplicationUser user)
    {

        Input = new InputModel
        {
            Description = user.Description
        };

    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        Load(user);
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
            Load(user);
            return Page();
        }

        var description = user.Description;
        if (Input.Description != description)
        {
            user.Description = Input.Description;
            var setDescriptionResult = await _userManager.UpdateAsync(user);

            if (!setDescriptionResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to set account description.";
                return RedirectToPage();
            }
        }

        StatusMessage = "Your account description has been updated.";
        return RedirectToPage();
    }

}
