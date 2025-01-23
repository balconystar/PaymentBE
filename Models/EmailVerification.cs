namespace PaymentBE.Models
{
    public class EmailVerification
    {

        public int Id { get; set; }
        public int UserId { get; set; }
        public string HashCode { get; set; }

    }
}
