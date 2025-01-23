using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentBE.DataAccess;
using PaymentBE.Models;
using PaymentBE.Services;
using System.Security.Claims;
using System.Security.Cryptography;

namespace PaymentBE.Controllers
{
    public class AuthController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        private readonly EmailService _emailService;
        private const int SaltSize = 16; // 16 bytes = 128 bits
        private const int HashSize = 32; // 32 bytes = 256 bits
        private const int Iterations = 100_000; // Number of iterations (adjust for security)
        public AuthController(DatabaseContext databaseContext, EmailService emailService)
        {
            _databaseContext = databaseContext;
            _emailService = emailService;
        }
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            User user;
            if (!isVerified(email))
            {
                //return Unauthorized("Verify email please");
            }
            try
            {
                user = Authenticate(email, password);
            }
            catch (Exception ex)
            {
                return BadRequest("Wrong credentials");
            }

            List<Claim> claims = new List<Claim>
                {
                    new Claim("username", user.Username),
                    new Claim("id", user.Id.ToString()),
                };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "CookieScheme");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("CookieScheme", claimsPrincipal);

            return Json(user);
        }
        [HttpPost]
        public async Task<IActionResult> LoginWithUsername([FromBody]User user)
        {
            string email = _databaseContext.Users.Where(i => i.Username == user.Username).First().Email;
            return await Login(email, user.PasswordHash);            
        }
        private bool DoesUserExist(string email)
        {
            try
            {
                User user = _databaseContext.Users.First(i => i.Email == email);

            }
            catch (InvalidOperationException ex)
            {
                return false;
            }

            return true;
        }
        private bool isVerified(string email)
        {
            try
            {
                User user = _databaseContext.Users.First(i => i.Email == email && i.isVerified == true);
            }
            catch (InvalidOperationException ex)
            {
                return false;
            }
            return true;
        }
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieScheme");
            return Ok();
        }

        private (string hash, string salt) Hash(string input)
        {
            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Generate the hash
            using (var pbkdf2 = new Rfc2898DeriveBytes(input, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                string saltArray = Convert.ToBase64String(salt);
                string hashArray = Convert.ToBase64String(hash);

                // Convert to Base64 for storage
                return (hashArray, saltArray);
            }
        }
        public void Verify(string email, string hashCode)
        {
            int userid = _databaseContext.Users.First(i => i.Email == email).Id;
            EmailVerification temp = _databaseContext.EmailVerifications.First(i => i.UserId == userid && i.HashCode == hashCode);
            if (temp != null)
            {
                User user = _databaseContext.Users.First(i => i.Id == userid);
                user.isVerified = true;
                user.VerifiedAt = DateTime.UtcNow;

                _databaseContext.EmailVerifications.Remove(temp);

                _databaseContext.SaveChanges();
            }
        }
        private string VerifyPassword(string password, string storedHash, string storedSalt)
        {
            // Convert the Base64-encoded salt back to a byte array
            byte[] saltBytes = Convert.FromBase64String(storedSalt);

            // Hash the input password using the stored salt
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = pbkdf2.GetBytes(HashSize);
                string hash = Convert.ToBase64String(hashBytes);

                // Compare the new hash with the stored hash
                if (hash != storedHash)
                {
                    throw new InvalidOperationException("Couldn't verify");
                }
                return hash;
            }
        }
        private User Authenticate(string email, string password)
        {
            User user;
            try
            {
                user = _databaseContext.Users.First(i => i.Email == email);
                VerifyPassword(password, user.PasswordHash, user.Salt);
                user.PasswordHash = "";
                user.Salt = "";

            }
            catch (InvalidOperationException ex)
            {
                return null;
            }

            return user;
        }
        private string GenerateSecureRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);

                char[] stringChars = new char[length];
                for (int i = 0; i < length; i++)
                {
                    stringChars[i] = chars[randomBytes[i] % chars.Length];
                }

                return new string(stringChars);
            }
        }
        private void SendEmailVerification(string email, string code)
        {
            string body = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n   " +
                " <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n  " +
                "  <title>Email Verification</title>\r\n    <style>\r\n        body {\r\n         " +
                "   font-family: Arial, sans-serif;\r\n            background-color: #f4f4f4;\r\n            margin: 0;\r\n         " +
                "   padding: 0;\r\n        }\r\n        .container {\r\n            width: 100%;\r\n            max-width: 600px;\r\n       " +
                "     margin: 50px auto;\r\n            background-color: #fff;\r\n            padding: 30px;\r\n        " +
                "    border-radius: 8px;\r\n            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);\r\n        }\r\n        h1 {\r\n       " +
                "" +
                "     color: #333;\r\n            text-align: center;\r\n        }\r\n        p {\r\n            font-size: 16px;\r\n         " +
                "   color: #555;\r\n            line-height: 1.5;\r\n        }\r\n        .button {\r\n            display: block;\r\n        " +
                "    width: 100%;\r\n            max-width: 200px;\r\n            padding: 15px;\r\n            margin: 20px auto;\r\n          " +
                "  background-color: #007BFF;\r\n            color: #fff;\r\n            text-align: center;\r\n            border-radius: 5px;\r\n   " +
                "         text-decoration: none;\r\n            font-size: 18px;\r\n        }\r\n        .button:hover {\r\n         " +
                "   background-color: #0056b3;\r\n        }\r\n        .footer {\r\n            text-align: center;\r\n          " +
                "  font-size: 14px;\r\n            color: #888;\r\n            margin-top: 30px;\r\n        }\r\n   " +
                " </style>\r\n</head>\r\n<body>\r\n    <div class=\"container\">\r\n        <h1>Email Verification</h1>\r\n     " +
                $"   <p>Hi {email},</p>\r\n        <p>Thank you for signing up! Please verify your email address by clicking the button below:</p>\r\n" +
                $"        <a href=\"https://localhost:7199/auth/verify?email={email}&hashCode={code}\" class=\"button\">Verify Email</a>\r\n     " +
                "   <p>If you didn't create an account, please ignore this email.</p>\r\n        <p>Best regards,<br>Your Company Team</p>\r\n  " +
                "  </div>\r\n    <div class=\"footer\">\r\n        <p>&copy; 2024 Your Company. All rights reserved.</p>\r\n  " +
                "  </div>\r\n</body>\r\n</html>\r\n";
            _emailService.SendEmailAsync(email, "Email Verification", body);
        }
        [HttpPost]

        public IActionResult SignUp([FromBody] User user)
        {
            if (!DoesUserExist(user.Email))
            {
                string temp = user.PasswordHash;
                (user.PasswordHash, user.Salt) = Hash(temp);
                _databaseContext.Users.Add(user);

                _databaseContext.SaveChanges();

                int id = _databaseContext.Users.First(i => i.Email == user.Email).Id;
                string code = GenerateSecureRandomString(30);
                _databaseContext.EmailVerifications.Add(new EmailVerification
                {
                    UserId = id,
                    HashCode = code
                });

                _databaseContext.SaveChanges();

                //SendEmailVerification(user.Email, code);
                return Ok("Go verify email");

            }
            return BadRequest("User already exists");
        }
        public void SendEmail(string email, string code)
        {
            SendEmailVerification(email, code);
        }
    }
}
