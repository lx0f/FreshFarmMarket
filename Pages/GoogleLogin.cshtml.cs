using System.Security.Claims;
using FreshFarmMarket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshFarmMarket.Pages;

public class GoogleLogin : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger _logger;

    public GoogleLogin(SignInManager<User> signInManager, ILogger<GoogleLogin> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet(bool isPersistent)
    {
        var loginInfo = await _signInManager.GetExternalLoginInfoAsync();
        var result = await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, isPersistent, true);

        if (result.Succeeded)
        {
            return Redirect("/Index");
        }

        var email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email);
        if (email is not null)
        {
            var user = await _signInManager.UserManager.FindByEmailAsync(email);
            if (user is null)
            {
                // create user if not exist
                user = new User
                {
                    UserName = loginInfo.Principal.FindFirstValue(ClaimTypes.Email),
                    Email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email),
                };
                await _signInManager.UserManager.CreateAsync(user);
                _logger.LogInformation(Event.EXTERNAL_REGISTER, "{username} registered with external provider at {datetime}", user.UserName, DateTime.Now);
            }
            await _signInManager.UserManager.AddLoginAsync(user, loginInfo);
            await _signInManager.SignInAsync(user, isPersistent);
            _logger.LogInformation(Event.EXTERNAL_SIGNIN, "{username} sign in with external provider at {datetime}", user.UserName, DateTime.Now);
        }
        return Redirect("/");
    }
}