using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace InventoryManagement.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var smtpClient = new SmtpClient(smtpSettings["Host"])
                {
                    Port = int.Parse(smtpSettings["Port"]),
                    Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromEmail"]),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);

                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    // Log the error but don't expose it to the user
                    Console.WriteLine($"Error sending email: {ex.Message}");
                    // For development, just complete the task instead of throwing
                    #if DEBUG
                        return;
                    #else
                        throw;
                    #endif
                }
            }
            catch
            {
                // If SMTP settings are missing or invalid, just complete the task in development
                #if DEBUG
                    return;
                #else
                    throw;
                #endif
            }
        }
    }
} 