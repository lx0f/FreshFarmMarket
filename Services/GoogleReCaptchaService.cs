using FreshFarmMarket.Util;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FreshFarmMarket.Services;

public class GoogleReCaptchaService
{
    private readonly IOptions<GoogleReCaptchaConfig> _config;

    public GoogleReCaptchaService(IOptions<GoogleReCaptchaConfig> config)
    {
        _config = config;
    }

    public async Task<bool> Verify(string token)
    {
        try
        {
            var secretKey = _config.Value.SecretKey;
            var url = $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}";
            using var client = new HttpClient();
            var httpResult = await client.GetAsync(url);
            if (httpResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return false;
            }
            var responseString = await httpResult.Content.ReadAsStringAsync();
            var googleResult = JsonConvert.DeserializeObject<GoogleReCaptchaResponse>(responseString);

            if (googleResult is null)
            {
                return false;
            }

            return googleResult.success && googleResult.score >= 0.5;
        }
        catch (Exception)
        {
            return false;
        }
    }
}