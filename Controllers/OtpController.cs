using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentBE.DataAccess;
using PaymentBE.Models;
using PaymentBE.Services;

namespace PaymentBE.Controllers
{
    [Authorize]
    public class OtpController : Controller
    {
        private readonly OTPService _otpService;
        private readonly DatabaseContext _databaseContext;
        public OtpController(OTPService otpService, DatabaseContext databaseContext) 
        {
            _otpService = otpService;
            _databaseContext = databaseContext;
        }
        public async Task<IActionResult> Make()
        {
            User user = _databaseContext.Users.Where(i => i.Id == GetAuthUserId()).First();
            OTP output = await _otpService.SendOTP(user);
            return Json(output);
        }
        private int GetAuthUserId()
        {
            return Convert.ToInt32(User.FindFirst("id")?.Value);
        }
    }
}
