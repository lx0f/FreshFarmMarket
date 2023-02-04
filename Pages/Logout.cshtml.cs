using FreshFarmMarket.Models;
using FreshFarmMarket.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FreshFarmMarket.Pages;

public class LogoutModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly EventLogService<LogoutModel> _logger;

    public LogoutModel(SignInManager<User> signInManager, EventLogService<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet()
    {
        var user = await _signInManager.UserManager.GetUserAsync(User);
        await _signInManager.SignOutAsync();
        HttpContext.Session.Clear();
        await _logger.Log(Event.LOGOUT, $"{user.UserName} logged out at {DateTime.Now}", user);
        return Redirect("/Login");
    }
}