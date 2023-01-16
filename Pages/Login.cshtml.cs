using System.ComponentModel.DataAnnotations;
using FreshFarmMarket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshFarmMarket.Pages;

public class LoginModel : PageModel
{
    public readonly UserManager<User> _userManager;
    public readonly SignInManager<User> _signInManager;
    public LoginModel(SignInManager<User> signInManager, UserManager<User> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    [Required]
    public string UserName { get; set; } = default!;

    [BindProperty]
    [DataType(DataType.Password)]
    [Required]
    public string Password { get; set; } = default!;

    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByNameAsync(UserName);
        if (user is null)
        {
            ModelState.AddModelError(nameof(UserName), "Credentials are incorrect.");
            ModelState.AddModelError(nameof(Password), "Credentials are incorrect.");
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(user, Password, false, true);
        if (result.Succeeded)
        {
            return Redirect("/Index");
        }
        else
        {
            ModelState.AddModelError(nameof(UserName), "Credentials are incorrect.");
            ModelState.AddModelError(nameof(Password), "Credentials are incorrect.");
            return Page();
        }
    }
}