using Blog_Website.Models.Entities;

namespace Blog_Website.Services.IServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task AddAsync(OTP otp);
        Task UpdateAsync(OTP otp);
        Task<OTP> FindByEmailAsync(string email);
    }
}
