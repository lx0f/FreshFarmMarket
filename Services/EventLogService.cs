using System.Reflection;
using FreshFarmMarket.Data;
using FreshFarmMarket.Models;

namespace FreshFarmMarket.Services;

public class EventLogService<T> where T : class
{
    private readonly ILogger _logger;
    private readonly DataContext _context;

    public EventLogService(ILogger<T> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task Log(string eventStr, string message, User? user = null)
    {
        if (!Event.IsValid(eventStr))
        {
            throw new ArgumentException(
                "Invalid eventString value. Check Event class to see valid eventString values.",
                nameof(eventStr));
        }
        _logger.LogInformation("Event: {e}. Message: {message}", eventStr, message);
        _context.EventLogs.Add(new()
        {
            Event = eventStr,
            User = user
        });
        await _context.SaveChangesAsync();
    }
}