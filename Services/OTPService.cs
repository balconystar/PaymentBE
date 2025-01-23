using Microsoft.AspNetCore.Mvc;
using PaymentBE.DataAccess;
using PaymentBE.Models;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace PaymentBE.Services
{
    public class OTPService
    {
        private readonly IConfiguration _configuration;
        private readonly DatabaseContext _databaseContext;
        private static Random _random = new Random();
        public OTPService(IConfiguration configuration, DatabaseContext databaseContext)
        {
            _configuration = configuration;
            _databaseContext = databaseContext; 
            
        }

        public async Task SendOTPAsync(string to, string subject, string body)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var fromOTP = smtpSettings.GetValue<string>("From");
            var smtpHost = smtpSettings.GetValue<string>("Host");
            var smtpPort = smtpSettings.GetValue<int>("Port");
            var smtpUsername = smtpSettings.GetValue<string>("Username");
            var smtpPassword = smtpSettings.GetValue<string>("Password");

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromOTP),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,  // Set to false if you don't want HTML content
            };

            mailMessage.To.Add(to);

            using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;  // Enable SSL for security
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
        private static int GenerateRandomOtp()
        {
            return _random.Next(100000, 1000000);  // Generate a number between 100000 and 999999
        }
        public string HashStringWithSha256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert the byte array to a hexadecimal string
                StringBuilder hashStringBuilder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    hashStringBuilder.Append(b.ToString("x2"));  // Convert each byte to a 2-digit hex value
                }

                return hashStringBuilder.ToString();  // Return the resulting hash as a string
            }
        }
        public OTP MakeOTP(User user, int code)
        {
            OTP output = new OTP
            {
                UserId = user.Id,
                ValidUntil = DateTime.UtcNow.AddMinutes(15),
                Status = OTPStatus.Pending,
                OtpHash = HashStringWithSha256(code.ToString())
            };
            _databaseContext.OTP.Add(output);
            _databaseContext.SaveChanges();
            return output;
        }
        public async Task<OTP> SendOTP(User user)
        {
            int otp = GenerateRandomOtp();

            OTP output = MakeOTP(user, otp);
            await SendOTPAsync(user.Email, "Your OTP", $"Your OTP is { otp }");
            return output;
        }

        public bool VerifyOTP(OTP otp)
        {
            
            Console.WriteLine("User otp: "+ otp.UserId);
            int count = _databaseContext.OTP.Where(i => i.OtpHash == otp.OtpHash).Where(i => i.ValidUntil >  DateTime.UtcNow).Where(i => i.UserId == otp.UserId).Count();
            if(count == 1)
            {
                return true;
            }
            return false;

        }
    }


}
