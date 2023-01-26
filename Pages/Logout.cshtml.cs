using FreshFarmMarket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshFarmMarket.Pages;

public class LogoutModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger _logger;

    public LogoutModel(SignInManager<User> signInManager, ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet()
    {
        var username = (await _signInManager.UserManager.GetUserAsync(User)).UserName;
        await _signInManager.SignOutAsync();
        HttpContext.Session.Clear();
        _logger.LogInformation(Event.LOGOUT, "{username} logged out at {datetime}", username, DateTime.Now);
        return Redirect("/Login");
    }
}