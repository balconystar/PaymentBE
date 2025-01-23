using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PaymentBE.Models
{
    public class OTP
    {
        [Key]
        public int Id { get; set; }  // Unique identifier for the OTP record

        public string OtpHash { get; set; }

        public int UserId { get; set; }  // Foreign key to the User

        public User User { get; set; }  // Navigation property to the User

        public int TransactionId { get; set; }

        public Transaction Transaction { get; set; }

        public DateTime ValidUntil { get; set; }  // The expiration date/time of the OTP

        public OTPStatus Status { get; set; }  // The status of the OTP (e.g., Pending, Used, Expired)
    }

    public enum OTPStatus
    {
        Pending,  // OTP is created and waiting for verification
        Used,     // OTP has been successfully used
        Expired   // OTP is expired
    }

}
