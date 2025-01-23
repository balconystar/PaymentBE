using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PaymentBE.Models
{
    public class Deposits
    {
        [Key]
        public int Id { get; set; }  // Primary key for the Transaction entity

        public string SourceId { get; set; }
        public string? SourceName { get; set; }
        public int TransactionId { get; set; }
        public Transaction Transaction { get; set; }

    }
}
