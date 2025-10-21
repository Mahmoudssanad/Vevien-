using System.Net.Mail;
using System.Net;
using Blog_Website.Models.Entities;
using Blog_Website.Models.Data;
using Microsoft.EntityFrameworkCore;
using Blog_Website.Services.IServices;

namespace Blog_Website.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public EmailService(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // IConfiguration عن طريق ال appsettings.json اللي في ملف MailSettings section بقراء البيانات اللي موجود في 
            var host = _config["MailSettings:Host"];
            var port = int.Parse(_config["MailSettings:Port"]);
            var displayName = _config["MailSettings:DisplayName"];
            var username = _config["MailSettings:Email"];
            var password = _config["MailSettings:Password"];

            // host => هو السيرفر اللي هنتعامل معاه 
            // SmtpClient => المسؤال عن ارسال الايميل من جهازك الي ايميل المستخدم 
            using var smtp = new SmtpClient(host)
            {
                Port = port, // اللي هنكلم السيرفر عليه port رقم ال 

                // بيانات تسجيل الدخول علي السيرفر. الايميل اللي هبعت من عليه والباسورد بتاعه بيشوفهم الاول صح ولا لا 
                Credentials = new NetworkCredential(username, password),

                // شغل التشفير عند الاتصال 
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(username),
                Subject = subject,
                Body = body,
                IsBodyHtml = true // HTML لو عايز الرسالة فيها
            };

            // بتحدد مين هيوصل ليه الايميل
            mail.To.Add(toEmail);

            // هيبعت الرساله للايميل دا عبرا الانترنت 
            await smtp.SendMailAsync(mail);
        }

        public async Task AddAsync(OTP otp)
        {
            var existing = await _context.OTPs.FirstOrDefaultAsync(x => x.Email == otp.Email);

            if (existing != null)
            {

                existing.Code = otp.Code;
                existing.ExpiryTime = otp.ExpiryTime;
                existing.IsUsed = false;
                existing.Email = otp.Email;
                _context.OTPs.Update(existing);
            }
            else
            {
                await _context.OTPs.AddAsync(otp);
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OTP otp)
        {
            var existing = await _context.OTPs.FirstOrDefaultAsync(x => x.Email == otp.Email);

            if(existing != null)
            {
                _context.OTPs.Update(existing);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<OTP> FindByEmailAsync(string email)
        {
            var otp = await _context.OTPs.FirstOrDefaultAsync(x => x.Email == email);

            return otp;
        }
    }
}
