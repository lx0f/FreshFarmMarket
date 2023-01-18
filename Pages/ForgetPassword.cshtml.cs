using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FreshFarmMarket.Models;
using FreshFarmMarket.Services;

namespace FreshFarmMarket.Pages;

public class ForgetPasswordModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly CommunicationService _communicationService;

    public ForgetPasswordModel(CommunicationService communicationService, UserManager<User> userManager)
    {
        _communicationService = communicationService;
        _userManager = userManager;
    }

    [BindProperty]
    public string? UserName { get; set; }

    [BindProperty]
    public string? Email { get; set; }

    [BindProperty]
    public string? Code { get; set; }

    [BindProperty]
    public string? NewPassword { get; set; }
    public bool CodeSent { get; set; } = false;

    public async Task<IActionResult> OnPostSend()
    {
        if (UserName is null)
        {
            ModelState.AddModelError(nameof(UserName), "The UserName field is required.");
            return Page();
        }

        if (Email is null)
        {
            ModelState.AddModelError(nameof(Email), "The Email field is required.");
            return Page();
        }

        var user = await _userManager.FindByNameAsync(UserName);

        if (user is null)
        {
            ModelState.AddModelError(nameof(UserName), "User with this username doesn't exist.");
            return Page();
        }

        if (user.Email != Email)
        {
            ModelState.AddModelError(nameof(Email), "Wrong email.");
            return Page();
        }

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _communicationService.SendEmail(Email, "Reset password code", $"Code: {code}");

        CodeSent = true;

        TempData["UserName"] = UserName;

        return Page();
    }

    public async Task<IActionResult> OnPostVerify()
    {
        if (NewPassword is null)
        {
            ModelState.AddModelError(nameof(NewPassword), "The NewPassword field is required.");
            return Page();
        }
        if (Code is null)
        {
            ModelState.AddModelError(nameof(Code), "The Code field is required.");
            return Page();
        }

        var user = await _userManager.FindByNameAsync(UserName);

        var result = await _userManager.ResetPasswordAsync(user, Code, NewPassword);

        if (result.Succeeded)
        {
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