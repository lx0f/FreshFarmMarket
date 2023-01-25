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
        var passHistory = await _context.PasswordHashes
            .Where(p => p.User == user)
            .OrderByDescending(p => p.DateTime)
            .Take(2)
            // check if password isn't the same as the last two passwords
            // check if 5 minutes has passed since the last password change
            .Select(p =>
            (manager.PasswordHasher.VerifyHashedPassword(user, p.Hash, password) == PasswordVerificationResult.Success)
            || (p.DateTime.AddMinutes(5) < DateTime.Now))
            .ToListAsync();

        if (passHistory.Contains(true))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordUsedWithinLastTwoChanges",
                Description = "You cannot use a password that has been used within the last two password changes/reset."
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
}