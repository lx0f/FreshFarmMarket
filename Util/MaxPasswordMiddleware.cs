
using FreshFarmMarket.Data;
using FreshFarmMarket.Models;
using Microsoft.AspNetCore.Identity;

namespace FreshFarmMarket.Util;

public class MaxPasswordMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TimeSpan MaxPasswordAge = TimeSpan.FromMinutes(1);

    public MaxPasswordMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, DataContext dataContext, UserManager<User> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // check if user password expired already
            var user = await userManager.GetUserAsync(context.User);
            var latestPasswordDate = dataContext.PasswordHashes
                .Where(p => p.User == user)
                .OrderByDescending(p => p.DateTime)
                .Select(p => p.DateTime)
                .FirstOrDefault();

            Console.WriteLine(latestPasswordDate);
            Console.WriteLine(DateTime.Now > (latestPasswordDate + MaxPasswordAge));

            if (latestPasswordDate != null && (DateTime.Now > (latestPasswordDate + MaxPasswordAge)) && context.Request.Path != "/ChangePassword")
            {
                Console.WriteLine("You need to go change password!");
                context.Response.Redirect("/ChangePassword");
            }
        }

        await _next(context);
    }
}