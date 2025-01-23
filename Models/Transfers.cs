using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PaymentBE.Models
{
    public class Transfers
    {
        [Key]
        public int Id { get; set; }  // Primary key for the Transaction entity

        public Transaction Credit { get; set; } 
        public int CreditId { get; set; }
        public Transaction Debit { get; set; }
        public int DebitId { get; set; }
    }
}
