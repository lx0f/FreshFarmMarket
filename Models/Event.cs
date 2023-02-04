using System.Reflection;

namespace FreshFarmMarket.Models;

public static class Event
{
    public const string CHANGE_PASSWORD = "CHANGE_PASSWORD";
    public const string EXTERNAL_SIGNIN = "EXTERNAL_SIGNIN";
    public const string FORGET_PASSWORD = "FORGET_PASSWORD";
    public const string LOGIN = "LOGIN";
    public const string LOGOUT = "LOGOUT";
    public const string REGISTER = "REGISTER";
    public const string VERIFY_EMAIL_ADDRESS = "VERIFY_EMAIL_ADDRESS";
    public const string VERIFY_PHONE_NUMBER = "VERIFY_PHONE_NUMBER";
    public const string VERIFY_TFA = "VERIFY_TFA";
    public const string EXTERNAL_REGISTER = "EXTERNAL_REGISTER";

    public static bool IsValid(string eventStr)
    {
        Type type = typeof(Event);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        foreach (var field in fields)
        {
            if (field.FieldType == typeof(string)
                && eventStr == field.GetValue(null) as string)
            {
                return true;
            }
        }
        return false;
    }
}