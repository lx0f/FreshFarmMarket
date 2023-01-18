using FreshFarmMarket.Models;
using FreshFarmMarket.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Twilio.Exceptions;

namespace FreshFarmMarket.Pages.Verify;

public class EmailModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly CommunicationService _communicationService;

    public EmailModel(UserManager<User> userManager, CommunicationService communicationService)
    {
        _userManager = userManager;
        _communicationService = communicationService;
    }

    [BindProperty]
    public string? Email { get; set; }
    [BindProperty]
    public string? EmailCode { get; set; }
    public bool EmailCodeSent { get; set; } = false;

    public async Task<IActionResult> OnPostSend()
    {
        if (Email is null)
        {
            ModelState.AddModelError(nameof(Email), "The field Email is required.");
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        var token = await _userManager.GenerateChangeEmailTokenAsync(user, Email);
        Console.WriteLine("Token " + token);

        try
        {
            await _communicationService.SendEmail(Email, "Verification Code", $"Code: {token}");
            EmailCodeSent = true;
        }
        catch (ApiException)
        {
            ModelState.AddModelError(nameof(Email), "Email is not valid.");
        }

        HttpContext.Session.SetString("Email", Email);

        return Page();
    }

    public async Task<IActionResult> OnPostVerify()
    {
        Email = HttpContext.Session.GetString("Email");
        Console.WriteLine(Email);

        if (EmailCode is null)
        {
            ModelState.AddModelError(nameof(EmailCode), "The field EmailCode is required.");
            return Page();
        }

        Console.WriteLine("Email " + Email);
        var user = await _userManager.GetUserAsync(User);
        var result = await _userManager.ChangeEmailAsync(user, Email, EmailCode);

        if (result.Succeeded)
        {
            Console.WriteLine("Successs");
            return Redirect("/Index");
        }

        foreach (var err in result.Errors)
        {
            Console.WriteLine("Error");
            Console.WriteLine(err.Description);
            ModelState.AddModelError(nameof(EmailCode), err.Description);
        }

        return Page();
    }
}