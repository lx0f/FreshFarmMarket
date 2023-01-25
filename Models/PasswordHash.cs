using System.ComponentModel.DataAnnotations;

namespace FreshFarmMarket.Models;

public class PasswordHash
{
    [Key]
    public int Id { get; set; }
    public User? User { get; set; }
    public string? Hash { get; set; }
    public DateTime DateTime { get; set; } = DateTime.Now;
}