// SMTP diagnostic test — credentials removed after testing.
// Re-run with: dotnet run -- <email> <appPassword>
// Example:     dotnet run -- user@gmail.com "xxxx yyyy zzzz wwww"

if (args.Length < 2)
{
    Console.WriteLine("Usage: dotnet run -- <senderEmail> <appPassword>");
    return;
}

var senderEmail = args[0];
var password = args[1];

Console.WriteLine($"Testing SMTP with sender: {senderEmail}");

using System.Net;
using System.Net.Mail;

try
{
    using var message = new MailMessage();
    message.From = new MailAddress(senderEmail, "SMTP Test");
    message.To.Add(new MailAddress(senderEmail));
    message.Subject = "SMTP Diagnostic Test";
    message.Body = "If you see this, SMTP is working.";

    using var client = new SmtpClient("smtp.gmail.com", 587);
    client.DeliveryMethod = SmtpDeliveryMethod.Network;
    client.Credentials = new NetworkCredential(senderEmail, password);
    client.EnableSsl = true;
    client.Timeout = 15000;

    await client.SendMailAsync(message);
    Console.WriteLine("SUCCESS: Email sent");
}
catch (SmtpException ex)
{
    Console.WriteLine($"SMTP FAILED: {ex.StatusCode} — {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"  INNER: {ex.InnerException.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"FAILED: {ex.GetType().Name}: {ex.Message}");
}
