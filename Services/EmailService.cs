using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

using MailKit.Net.Smtp;
using MailKit.Security;

using MimeKit;

using BugTracker.Models;

namespace BugTracker.Services;

public class EmailService : IEmailSender
{
  private readonly EmailSettings _emailSettings;

  public EmailService(IOptions<EmailSettings> emailSettings) => _emailSettings = emailSettings.Value;

  public async Task SendEmailAsync(string email, string subject, string htmlMessage)
  {
    var emailAddress  = _emailSettings.EmailAddress  ?? Environment.GetEnvironmentVariable("EmailAddress");
    var emailPassword = _emailSettings.EmailPassword ?? Environment.GetEnvironmentVariable("EmailPassword");
    var emailHost     = _emailSettings.EmailHost     ?? Environment.GetEnvironmentVariable("EmailHost");
    var emailPort     = _emailSettings.EmailPort != 0 ? _emailSettings.EmailPort : int.Parse(Environment.GetEnvironmentVariable("EmailPort")!);

    var newEmail = new MimeMessage();

    newEmail.Sender  = MailboxAddress.Parse(emailAddress);
    newEmail.Subject = subject;
    newEmail.Body    = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

    foreach (var address in email.Split(";")) 
      newEmail.To.Add(MailboxAddress.Parse(address));

    using var smtpClient = new SmtpClient();

    await smtpClient.ConnectAsync     (emailHost,    emailPort, SecureSocketOptions.StartTls);
    await smtpClient.AuthenticateAsync(emailAddress, emailPassword);
    await smtpClient.SendAsync        (newEmail);

    await smtpClient.DisconnectAsync(true);
  }
}