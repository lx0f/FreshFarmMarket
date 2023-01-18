using FreshFarmMarket.Util;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace FreshFarmMarket.Services;

public class SmsService
{
    private readonly TwilioConfig _config;

    public SmsService(IOptions<TwilioConfig> config)
    {
        _config = config.Value;
    }

    public Task SendSms(string number, string message)
    {
        TwilioClient.Init(_config.Identification, _config.Token);
        return MessageResource.CreateAsync(
            to: new PhoneNumber(number),
              from: new PhoneNumber(_config.From),
              body: message
        );
    }

    public Task SendEmail(string email, string message)
    {
        return Task.Run(() => { });
    }
}