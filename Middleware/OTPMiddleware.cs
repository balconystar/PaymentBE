using PaymentBE.Controllers;
using PaymentBE.Models;
using PaymentBE.Services;
using System.Globalization;

namespace PaymentBE.Middleware
{
    public class OTPMiddleware
    {
        private readonly RequestDelegate _next;

        public OTPMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string path = context.Request.Path.Value.ToLower();
            if (path == "/user/withdraw" || path == "/user/deposit" || path == "/user/transfer")
            {
                var otpinquery = context.Request.Query["otp"]; 
                
                var otpservice = context.RequestServices.GetRequiredService<OTPService>();

                OTP otp = new OTP();

                int userid = Convert.ToInt32(context.User.FindFirst("id").Value);
                otp.UserId = userid;
                otp.OtpHash = otpservice.HashStringWithSha256(otpinquery);

                Console.WriteLine("Hashed OTP "+ otp.OtpHash);
                bool success = otpservice.VerifyOTP(otp);
                if (!success)
                {
                    context.Response.StatusCode = 401; // Unauthorized
                    await context.Response.WriteAsync("Invalid OTP");
                    return;
                }                
            }
            await _next(context);
        }
    }
    public static class OTPMiddlewareExtensions
    {
        public static IApplicationBuilder UseOtp(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OTPMiddleware>();
        }
    }
}
