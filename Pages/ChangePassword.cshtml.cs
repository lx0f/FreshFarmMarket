using System.ComponentModel.DataAnnotations;
using FreshFarmMarket.Models;
using FreshFarmMarket.Util;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshFarmMarket.Pages;

public class ChangePasswordModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly PasswordHistoryValidator _phValidator;
    private readonly ILogger _logger;
    public ChangePasswordModel(UserManager<User> userManager, SignInManager<User> signInManager, PasswordHistoryValidator phValidator, ILogger<ChangePasswordModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _phValidator = phValidator;
        _logger = logger;
    }

    [BindProperty]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }
    [BindProperty]
    [DataType(DataType.Password)]
    public string? OldPassword { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var user = await _userManager.GetUserAsync(User);
        if (!await _phValidator.CanChangePassword(user))
        {
            return Redirect("/Index");
        }
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        var result = await _userManager.ChangePasswordAsync(user, OldPassword, NewPassword);
        if (result.Succeeded)
        {
            _logger.LogInformation(Event.CHANGE_PASSWORD, "{username} changed password at {datetime}", user.UserName, DateTime.Now);
            await _phValidator.AddPasswordHash(user, _userManager.PasswordHasher.HashPassword(user, NewPassword));
            await _signInManager.SignOutAsync();
            return Redirect("/Login");
        }
        else
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(nameof(NewPassword), err.Description);
            }
            return Page();
        }
    }
}