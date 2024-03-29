using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using FreshFarmMarket.Models;
using FreshFarmMarket.Services;
using Twilio.Exceptions;

namespace FreshFarmMarket.Pages.Verify;

public class PhoneModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly CommunicationService _smsService;
    private readonly EventLogService<PhoneModel> _logger;

    public PhoneModel(UserManager<User> userManager, CommunicationService smsService, SignInManager<User> signInManager, EventLogService<PhoneModel> logger)
    {
        _userManager = userManager;
        _smsService = smsService;
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public string? PhoneNumber { get; set; }
    [BindProperty]
    public string? PhoneNumberCode { get; set; }
    public bool PhoneNumberCodeSent { get; set; } = false;

    public async Task<IActionResult> OnPostSend()
    {
        if (PhoneNumber is null)
        {
            ModelState.AddModelError(nameof(PhoneNumber), "The field PhoneNumber is required.");
            return Page();
        }
        if (!PhoneNumber.Contains("+65"))
        {
            PhoneNumber = "+65" + PhoneNumber;
        }
        Console.WriteLine(PhoneNumber);
        var user = await _userManager.GetUserAsync(User);
        var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, PhoneNumber);
        Console.WriteLine("Token " + token);

        try
        {
            await _smsService.SendSms(PhoneNumber, token);
            PhoneNumberCodeSent = true;
        }
        catch (ApiException)
        {
            ModelState.AddModelError(nameof(PhoneNumber), "Phone number is not valid.");
        }

        HttpContext.Session.SetString("PhoneNumber", PhoneNumber);

        return Page();
    }

    public async Task<IActionResult> OnPostVerify()
    {
        PhoneNumber = HttpContext.Session.GetString("PhoneNumber");
        Console.WriteLine(PhoneNumber);

        if (PhoneNumberCode is null)
        {
            ModelState.AddModelError(nameof(PhoneNumberCode), "The field PhoneNumberCode is required.");
            return Page();
        }

        Console.WriteLine("Phone Number " + PhoneNumber);
        var user = await _userManager.GetUserAsync(User);
        var result = await _userManager.ChangePhoneNumberAsync(user, PhoneNumber, PhoneNumberCode);

        if (result.Succeeded)
        {
            await _logger.Log(Event.VERIFY_PHONE_NUMBER, $"{user.UserName} verified phone number at {DateTime.Now}", user);
            return Redirect("/Logout");
        }

        foreach (var err in result.Errors)
        {
            Console.WriteLine("Error");
            Console.WriteLine(err.Description);
            ModelState.AddModelError(nameof(PhoneNumberCode), err.Description);
        }

        return Page();
    }
}