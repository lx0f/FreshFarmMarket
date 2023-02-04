using FreshFarmMarket.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FreshFarmMarket.Data;

#pragma warning disable CS8618

public class DataContext : IdentityDbContext<User>
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Address> Addresses { get; set; }
    public DbSet<PasswordHash> PasswordHashes { get; set; }
    public DbSet<EventLog> EventLogs { get; set; }
}