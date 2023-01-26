using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

using FreshFarmMarket.Models;
using FreshFarmMarket.Services;

namespace FreshFarmMarket.Pages.Verify;

public class TfaModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly CommunicationService _communicationService;
    private readonly ILogger _logger;

    public TfaModel(UserManager<User> userManager, CommunicationService communicationService, SignInManager<User> signInManager, ILogger<TfaModel> logger)
    {
        _userManager = userManager;
        _communicationService = communicationService;
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    [Required]
    public string Code { get; set; }

    [BindProperty]
    public bool RememberMe { get; set; }

    public async Task<IActionResult> OnGet(bool rememberMe)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        TempData["RememberMe"] = rememberMe;

        var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");
        await _communicationService.SendSms(user.PhoneNumber, code);

        return Page();
    }
    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        var result = await _signInManager.TwoFactorSignInAsync("Phone", Code, RememberMe, false);
        if (result.Succeeded)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            _logger.LogInformation(Event.VERIFY_TFA, "{username} verified 2fa successfully at {datetime}", user.UserName, DateTime.Now);
            return Redirect("/Index");
        }
        else if (result.IsLockedOut)
        {
            return Redirect("/LockedOut");
        }
        else
        {
            ModelState.AddModelError(nameof(Code), "Invalid authenticator code.");
            return Page();
        }
    }
}