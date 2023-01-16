using System.ComponentModel.DataAnnotations;
using FreshFarmMarket.Util;

using Microsoft.AspNetCore.Identity;

namespace FreshFarmMarket.Models;

public class User : IdentityUser
{
    public string? About { get; set; }
    public Gender Gender { get; set; }
    public string? ImageFilePath { get; set; }
    public Address? DeliveryAddress { get; set; }
    public byte[]? CreditCardBytes { get; private set; }
    [CreditCard]
    public string? CreditCard
    {
        get => CreditCardBytes is not null
            ? EncryptionProvider.Decrypt(CreditCardBytes)
            : null;
        set => CreditCardBytes = EncryptionProvider.Encrypt(value!);
    }
}