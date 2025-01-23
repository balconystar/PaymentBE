using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PaymentBE.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }  // Primary key for the Transaction entity

        public TransactionType Type { get; set; }  // Type of transaction (Deposit/Withdraw)

        public User User { get; set; }
        public int UserId {  get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
    public enum TransactionType
    {
        Debit,
        Credit
    }
}
