using FreshFarmMarket.Data;
using FreshFarmMarket.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Google;

using FreshFarmMarket.Util;
using FreshFarmMarket.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Configure<GoogleReCaptchaConfig>(builder.Configuration.GetSection("GoogleReCaptcha"));
builder.Services.Configure<TwilioConfig>(builder.Configuration.GetSection("Twilio"));
builder.Services.Configure<SendGridConfig>(builder.Configuration.GetSection("SendGrid"));
builder.Services.Configure<GoogleOAuthConfig>(builder.Configuration.GetSection("GoogleOAuth"));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "FreshFarmMarketCookie";
});
builder.Services.AddDbContext<DataContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default");
    options.UseSqlite(connectionString);
});
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Tokens.PasswordResetTokenProvider = "Phone";
})
    .AddTokenProvider<PhoneNumberTokenProvider<User>>("Phone")
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<DataContext>()
    .AddPasswordValidator<PasswordHistoryValidator>();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.Password.RequiredLength = 12;
});
builder.Services.AddAuthentication()
.AddGoogle(options =>
{
    var config = builder.Configuration.GetSection("GoogleOAuth").Get<GoogleOAuthConfig>();
    options.ClientId = config.ClientID;
    options.ClientSecret = config.ClientSecret;
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.AccessDeniedPath = "/Errors/403";
});
builder.Services.AddScoped<CommunicationService>();
builder.Services.AddScoped<PasswordHistoryValidator>();
builder.Services.AddScoped(typeof(EventLogService<>));
builder.Services.AddTransient<GoogleReCaptchaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseStatusCodePagesWithReExecute("/Errors/{0}");

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.UseMiddleware<LogoutMiddleware>();
app.UseMiddleware<MaxPasswordMiddleware>();

app.MapRazorPages();

app.Run();
