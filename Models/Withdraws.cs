using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PaymentBE.Models
{
    public class Withdraws
    {
        [Key]
        public int Id { get; set; }  // Primary key for the Transaction entity

        public string DestinationId { get; set; }

        public string? OrganizationName { get; set; }

        public Transaction Transaction { get; set; }
        public int TransactionId {  get; set; }
        
    }
    
}
