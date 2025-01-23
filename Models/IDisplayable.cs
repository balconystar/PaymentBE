namespace PaymentBE.Models
{
    public interface IDisplayable
    {
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
    }
}
