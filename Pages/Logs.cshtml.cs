using FreshFarmMarket.Data;
using FreshFarmMarket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FreshFarmMarket.Pages;

[Authorize(Roles = "Admin")]
public class LogsModel : PageModel
{
    private readonly DataContext _context;

    public LogsModel(DataContext context)
    {
        _context = context;
    }

    public List<EventLog> EventLogs { get; set; }

    public async Task OnGetAsync()
    {
        EventLogs = await _context.EventLogs
            .Include(e => e.User)
            .ToListAsync();
    }
}