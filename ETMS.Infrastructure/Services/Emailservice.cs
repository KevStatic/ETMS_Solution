using System.Net;
using System.Net.Mail;
using ETMS.Domain.Interfaces;

namespace ETMS.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _fromEmail;

        public EmailService(string host, int port, string username, string password, string fromEmail)
        {
            _host = host;
            _port = port;
            _username = username;
            _password = password;
            _fromEmail = fromEmail;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("=== SMTP DEBUG ===");
                Console.WriteLine($"Host:      {_host}");
                Console.WriteLine($"Port:      {_port}");
                Console.WriteLine($"Username:  {_username}");
                Console.WriteLine($"Password:  {_password.Length} chars");
                Console.ResetColor();

                using var client = new SmtpClient(_host, _port)
                {
                    Credentials = new NetworkCredential(_username, _password),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000   // 30 second timeout
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(_fromEmail, "ETMS Support"),
                    Subject = "Your ETMS Password Reset OTP",
                    Body = BuildHtmlEmail(otpCode),
                    IsBodyHtml = true
                };
                mail.To.Add(toEmail);

                await client.SendMailAsync(mail);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ EMAIL SENT SUCCESSFULLY!");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ SMTP ERROR:  {ex.Message}");
                Console.WriteLine($"❌ INNER ERROR: {ex.InnerException?.Message}");
                Console.ResetColor();
                throw;
            }
        }

        private static string BuildHtmlEmail(string otp) => $@"
        <div style='font-family:Segoe UI,sans-serif;max-width:480px;margin:auto;'>
          <div style='background:#2a56c6;padding:24px;border-radius:12px 12px 0 0;text-align:center;'>
            <h2 style='color:#fff;margin:0;'>ETMS - Password Reset</h2>
          </div>
          <div style='background:#f9fafc;padding:32px;border-radius:0 0 12px 12px;border:1px solid #dde3f0;'>
            <p style='color:#333;font-size:15px;'>
              Your OTP code is below. It expires in <strong>2 minutes</strong>.
            </p>
            <div style='text-align:center;margin:28px 0;'>
              <span style='
                display:inline-block;background:#fff;
                border:2px dashed #2a56c6;border-radius:10px;
                padding:14px 32px;font-size:32px;font-weight:700;
                letter-spacing:10px;color:#2a56c6;'>{otp}</span>
            </div>
            <p style='color:#999;font-size:13px;text-align:center;'>
              If you did not request this, please ignore this email.
            </p>
          </div>
        </div>";
    }
}