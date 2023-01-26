using FreshFarmMarket.Data;
using FreshFarmMarket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FreshFarmMarket.Util;

public class PasswordHistoryValidator : IPasswordValidator<User>
{
    private readonly DataContext _context;

    public PasswordHistoryValidator(DataContext context)
    {
        _context = context;
    }

    public async Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user, string password)
    {
        if (!await CanChangePassword(user))
        {
            return IdentityResult.Failed(new IdentityError()
            {
                Code = "PasswordChangedTooFast",
                Description = "You cannot change your password within 5 minutes of the last change."
            });
        }
        var passHistory = await _context.PasswordHashes
            .Where(p => p.User == user)
            .OrderByDescending(p => p.DateTime)
            .Take(2)
            // check if password isn't the same as the last two passwords
            // check if 5 minutes has passed since the last password change
            .Select(p => manager.PasswordHasher.VerifyHashedPassword(user, p.Hash, password) == PasswordVerificationResult.Success)
            .ToListAsync();

        if (passHistory.Any(p => p == true))
        {
            return IdentityResult.Failed(new IdentityError()
            {
                Code = "PasswordSameAsLastTwo",
                Description = "You cannot use a password used in your previous last two passwords"
            });
        }

        return IdentityResult.Success;
    }

    public async Task AddPasswordHash(User user, string passwordHash)
    {
        await _context.PasswordHashes.AddAsync(new PasswordHash
        {
            User = user,
            Hash = passwordHash
        });
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CanChangePassword(User user)
    {
        var passhash = await _context.PasswordHashes.Where(p => p.User == user).OrderByDescending(p => p.DateTime).FirstOrDefaultAsync();
        if (passhash is null)
            return true;

        Console.WriteLine(DateTime.Now);
        Console.WriteLine(passhash.DateTime);
        Console.WriteLine(passhash.DateTime.AddMinutes(5));
        Console.WriteLine(DateTime.Now > passhash.DateTime.AddMinutes(5));

        return DateTime.Now > passhash.DateTime.AddMinutes(5);
    }
}