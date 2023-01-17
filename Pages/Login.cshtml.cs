using System.ComponentModel.DataAnnotations;
using FreshFarmMarket.Models;
using FreshFarmMarket.Util;
using FreshFarmMarket.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace FreshFarmMarket.Pages;

public class LoginModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly GoogleReCaptchaService _googleService;
    private readonly IOptions<GoogleReCaptchaConfig> _config;
    public LoginModel(SignInManager<User> signInManager, UserManager<User> userManager, IOptions<GoogleReCaptchaConfig> config, GoogleReCaptchaService googleService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _config = config;
        _googleService = googleService;
    }

    [BindProperty]
    [Required]
    public string UserName { get; set; } = default!;

    [BindProperty]
    [DataType(DataType.Password)]
    [Required]
    public string Password { get; set; } = default!;

    [BindProperty]
    public bool RememberMe { get; set; }

    [BindProperty]
    public string Token { get; set; }

    public string GetSiteKey() => _config.Value.SiteKey;

    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var success = await _googleService.Verify(Token);

        if (!success)
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

        var result = await _signInManager.PasswordSignInAsync(user, Password, RememberMe, true);
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