
namespace PaymentBE.Models
{
    public class TransactionDataAdapter : IDisplayable
    {
        public TransactionDataAdapter(Deposits deposit)
        {
            User user = deposit.Transaction.User;
            From = deposit.SourceName;
            To = user.Username;
            Date = deposit.Transaction.Date;
            Amount = deposit.Transaction.Amount;
            Type = "Deposit";
        }
        public TransactionDataAdapter(Transfers transfers)
        {            
            From = transfers.Debit.User.Username;
            To = transfers.Credit.User.Username;
            Date = transfers.Credit.Date;
            Amount = transfers.Credit.Amount;
            Type = "Transfer";
        }
        public TransactionDataAdapter(Withdraws withdraw)
        {
            User user = withdraw.Transaction.User;
            From = "System";
            To = user.Username;
            Date = withdraw.Transaction.Date;
            Amount = withdraw.Transaction.Amount;
            Type = "Withdraw";
        }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
    }
}
