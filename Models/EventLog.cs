using System.ComponentModel.DataAnnotations;

namespace FreshFarmMarket.Models;

public class EventLog
{
    [Key]
    public int Id { get; set; }
    public string? Event { get; set; }
    public User? User { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.Now;
}