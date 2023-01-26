using System.ComponentModel.DataAnnotations;
using FreshFarmMarket.Models;
using FreshFarmMarket.Util;
using FreshFarmMarket.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;

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

    public IList<AuthenticationScheme> ExternalLogins { get; set; }

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

    public async Task OnGet()
    {
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }
    public IActionResult OnPostGoogle()
    {
        return Challenge(_signInManager.ConfigureExternalAuthenticationProperties("Google", "/GoogleLogin"), "Google");
    }
    public async Task<IActionResult> OnPostInHouse()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var success = await _googleService.Verify(Token);

        if (success == GoogleReCaptchaResult.FAIL)
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

        if (success == GoogleReCaptchaResult.SUSPICIOUS)
        {
            return RedirectToPage("/Verify/Tfa", new { RememberMe });
        }

        var result = await _signInManager.PasswordSignInAsync(user, Password, RememberMe, true);
        if (result.Succeeded)
        {
            if (!user.PhoneNumberConfirmed)
            {
                return Redirect("/Verify/Phone");
            }
            if (!user.EmailConfirmed)
            {
                return Redirect("/Verify/Email");
            }

            return Redirect("/Index");
        }
        else if (result.IsLockedOut)
        {
            return Redirect("/LockedOut");
        }
        else if (result.IsNotAllowed)
        {
            return Forbid();
        }
        else if (result.RequiresTwoFactor)
        {
            return RedirectToPage("/Verify/Tfa", new { RememberMe });
        }
        else
        {
            ModelState.AddModelError(nameof(UserName), "Credentials are incorrect.");
            ModelState.AddModelError(nameof(Password), "Credentials are incorrect.");
            return Page();
        }
    }
}