using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PaymentBE.Models
{
    public class User
    {       
        
        [Key]
        public int Id { get; set; }  // Primary key for the User entity

        public string FirstName { get; set; }  // User's first name

        public string MiddleName { get; set; }  // User's middle name

        public string LastName { get; set; }  // User's last name

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0.00m;  // Default balance

        public byte[]? ProfilePicture { get; set; }  // User's profile picture as byte array

        public string PasswordHash { get; set; }  // Password hash

        public string Salt { get; set; }  // Salt for password hashing

        public string Username { get; set; }  // User's username

        public string Email { get; set; }  // User's email

        [Phone]
        public string PhoneNumber { get; set; }
        public bool isVerified { get; set; }
        public DateTime VerifiedAt { get; set; }
    }
}
