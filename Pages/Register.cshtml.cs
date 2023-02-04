using System.ComponentModel.DataAnnotations;
using FreshFarmMarket.Models;
using FreshFarmMarket.Services;
using FreshFarmMarket.Util;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace FreshFarmMarket.Pages;

public class RegisterModel : PageModel
{
    private readonly IHostEnvironment _env;
    private readonly EventLogService<RegisterModel> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IOptions<GoogleReCaptchaConfig> _googleConfig;
    private readonly GoogleReCaptchaService _googleService;
    private readonly PasswordHistoryValidator _phValidator;

    public RegisterModel(
        IHostEnvironment env,
        EventLogService<RegisterModel> logger,
        UserManager<User> userManager,
        IOptions<GoogleReCaptchaConfig> googleConfig,
        GoogleReCaptchaService googleService,
        PasswordHistoryValidator phValidator)
    {
        _env = env;
        _logger = logger;
        _userManager = userManager;
        _googleConfig = googleConfig;
        _googleService = googleService;
        _phValidator = phValidator;
    }

    public string GetSiteKey() => _googleConfig.Value.SiteKey;

    [BindProperty]
    [Required]
    public string Token { get; set; }

    [BindProperty]
    [Required]
    public string? UserName { get; set; }

    [Required]
    [BindProperty]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [BindProperty]
    public string About { get; set; }

    [Required]
    [BindProperty]
    public Gender Gender { get; set; }

    [BindProperty]
    [Required]
    public string? Street { get; set; }

    [BindProperty]
    [Required]
    [DataType(DataType.PostalCode)]
    public string? PostalCode { get; set; }

    [BindProperty]
    [Required]
    [Phone]
    public string? PhoneNumber { get; set; }

    [BindProperty]
    [Required]
    [CreditCard]
    public string? CreditCard { get; set; }

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords doesn't match.")]
    public string? ConfirmPassword { get; set; }

    [BindProperty]
    [Required]
    public IFormFile? Image { get; set; }

    public IActionResult OnGet()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return Redirect("/Index");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var success = await _googleService.Verify(Token);

        if (success == GoogleReCaptchaResult.FAIL)
        {
            return Page();
        }

        var userWithSameEmail = await _userManager.FindByEmailAsync(Email);
        if (userWithSameEmail is not null)
        {
            ModelState.AddModelError(nameof(Email), "Please select another email.");
            return Page();
        }

        var deliveryAddress = new Address()
        {
            Street = Street,
            PostalCode = PostalCode
        };
        var newUser = new User()
        {
            About = About,
            CreditCard = CreditCard,
            DeliveryAddress = deliveryAddress,
            Gender = Gender,
            UserName = UserName,
            TwoFactorEnabled = true,
            Email = Email,
            PhoneNumber = PhoneNumber
        };

        var result = await _userManager.CreateAsync(newUser, Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            var errors = string.Join(';', result.Errors.Select(e => e.Code));

            return Page();
        }

        var fileName = Guid.NewGuid().ToString() + ".jpg";
        var filePath = Path.Combine(_env.ContentRootPath, "wwwroot/Uploads", fileName);
        using var fs = new FileStream(filePath, FileMode.Create);
        await Image!.CopyToAsync(fs);

        var user = await _userManager.FindByNameAsync(UserName);
        user.ImageFilePath = fileName;
        await _userManager.UpdateAsync(user);

        await _phValidator.AddPasswordHash(user, user.PasswordHash);

        await _logger.Log(Event.REGISTER, $"{user.UserName} registered at {DateTime.Now}", user);

        return Redirect("/Login");
    }
}