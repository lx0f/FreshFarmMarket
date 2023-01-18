using FreshFarmMarket.Util;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace FreshFarmMarket.Services;

public class CommunicationService
{
    private readonly TwilioConfig _twilioConfig;
    private readonly SendGridConfig _sendGridConfig;

    public CommunicationService(IOptions<TwilioConfig> twilioConfig, IOptions<SendGridConfig> sendGridConfig)
    {
        _twilioConfig = twilioConfig.Value;
        _sendGridConfig = sendGridConfig.Value;
    }

    public Task SendSms(string number, string message)
    {
        TwilioClient.Init(_twilioConfig.Identification, _twilioConfig.Token);
        return MessageResource.CreateAsync(
            to: new PhoneNumber(number),
              from: new PhoneNumber(_twilioConfig.From),
              body: message
        );
    }

    public Task SendEmail(string email, string subject, string message)
    {
        var client = new SendGridClient(_sendGridConfig.SecretKey);

        var from = new EmailAddress(_sendGridConfig.From, "FFM_Admin");
        var to = new EmailAddress(email, "User");

        var htmlContent = $"{message}";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, message, htmlContent);
        return client.SendEmailAsync(msg);
    }
}