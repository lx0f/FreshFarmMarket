using System.Security.Cryptography;
using System.Text;

namespace FreshFarmMarket.Util;

public class EncryptionProvider
{
    private static readonly Aes _cipher = Aes.Create();
    private static readonly byte[] key = new byte[] { 0x49, 0xd4, 0xcd, 0x57, 0x5b, 0xde, 0x8d, 0x8d, 0xf0, 0x69, 0x4d, 0xcd, 0x13, 0x8b, 0x91, 0x8b };
    private static readonly byte[] iv = new byte[] { 0x7c, 0x4d, 0xf7, 0xd0, 0x29, 0xd3, 0x6c, 0x32, 0x9c, 0x27, 0x75, 0xba, 0x1e, 0xcf, 0x40, 0xed };
    private static readonly ICryptoTransform _encryptor = _cipher.CreateEncryptor(key, iv);
    private static readonly ICryptoTransform _decryptor = _cipher.CreateDecryptor(key, iv);
    public static string Decrypt(byte[] cipherText)
    {
        var plainText = _decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
        return Encoding.UTF8.GetString(plainText);
    }
    public static byte[] Encrypt(string plainText)
    {
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherText = _encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);
        return cipherText;
    }
}