using System.ComponentModel.DataAnnotations;
using FreshFarmMarket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshFarmMarket.Pages;

public class ChangePasswordModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public ChangePasswordModel(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }
    [BindProperty]
    [DataType(DataType.Password)]
    public string? OldPassword { get; set; }

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