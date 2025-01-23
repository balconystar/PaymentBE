using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PaymentBE.Models
{
    public class Money
    {       

        [Required]
        public decimal Amount { get; set; } = 50.00m;  // Fixed amount of 50 TZS

        [Key]
        public string MoneyToken { get; set; }  // Unique money token

        [ForeignKey("User")]
        public int UserId { get; set; }  // Foreign key to User

        public User Owner { get; set; }


        public long EpochTimeMs { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
