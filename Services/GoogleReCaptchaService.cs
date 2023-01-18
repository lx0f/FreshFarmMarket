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

    public async Task<GoogleReCaptchaResult> Verify(string token)
    {
        try
        {
            var secretKey = _config.Value.SecretKey;
            var url = $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}";
            using var client = new HttpClient();
            var httpResult = await client.GetAsync(url);
            if (httpResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return GoogleReCaptchaResult.FAIL;
            }
            var responseString = await httpResult.Content.ReadAsStringAsync();
            var googleResult = JsonConvert.DeserializeObject<GoogleReCaptchaResponse>(responseString);

            if (googleResult is null)
            {
                return GoogleReCaptchaResult.FAIL;
            }

            if (!googleResult.success)
            {
                return GoogleReCaptchaResult.FAIL;
            }
            else if (googleResult.score >= 0.5)
            {
                return GoogleReCaptchaResult.SUCCESS;
            }
            else
            {
                return GoogleReCaptchaResult.SUSPICIOUS;
            }
        }
        catch (Exception)
        {
            return GoogleReCaptchaResult.FAIL;
        }
    }
}