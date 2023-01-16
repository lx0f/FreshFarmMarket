using System.ComponentModel.DataAnnotations;

namespace FreshFarmMarket.Models;

public class Address
{
    [Key]
    public int Id { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
}