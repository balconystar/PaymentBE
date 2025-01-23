using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentBE.DataAccess;
using PaymentBE.Models;
using System.Security.Cryptography;

namespace PaymentBE.Controllers
{
    public class MoneyController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        public MoneyController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        
        public IActionResult Populate(double amount)
        {
            if ((amount % 50) != 0)
            {
                return BadRequest("The amount must be divisible into 50Tsh");
            }
            double turns = (amount / 50);
            for (int i = 0; i < turns; i++)
            {
                _databaseContext.Money.Add(new Money
                {
                    MoneyToken = Guid.NewGuid().ToString(),
                    Amount = 50,
                    UserId = 2
                });
            }
            _databaseContext.SaveChanges();

            return Ok();
        }
        
        public class UserRequest
        {
            public User User { get; set; }
            public double Amount { get; set; }
        }
    }
}
