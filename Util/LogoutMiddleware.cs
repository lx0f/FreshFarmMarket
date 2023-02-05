using FreshFarmMarket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace FreshFarmMarket.Util;

public class LogoutMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TimeSpan _expireTime = TimeSpan.FromMinutes(1);
    private const string _sessionExpiryString = "ExpiryDate";

    public LogoutMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, SignInManager<User> signInManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var user = await signInManager.UserManager.GetUserAsync(context.User);
            // check expiration time
            var expiryString = context.Session.GetString(_sessionExpiryString);
            var success = DateTime.TryParse(expiryString, out var expiry);
            Console.WriteLine("Compare");
            Console.WriteLine(DateTime.Now);
            Console.WriteLine(expiry);
            Console.WriteLine(expiry + _expireTime);
            if (success && (DateTime.Now > (expiry + _expireTime)))
            {
                Console.WriteLine("This bitch got logged out");
                context.Session.Clear();
                user.IsLoggedIn = false;
                await signInManager.UserManager.UpdateAsync(user);
                await signInManager.SignOutAsync();
                context.Response.Redirect("/Login");
            }
            else
            {
                Console.WriteLine("Not Expired!!!");
                context.Session.SetString(_sessionExpiryString, (DateTime.Now + _expireTime).ToString());
            }
        }
        await _next(context);
    }
}