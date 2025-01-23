using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentBE.DataAccess;
using PaymentBE.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentBE.Controllers
{
    public class UserController : Controller
    {
        private readonly DatabaseContext _context;

        public UserController(DatabaseContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Deposit(decimal amount)
        {
            Console.WriteLine($"Hello {amount}");
            if (amount <= 0)
            {
                return BadRequest("Deposit amount must be greater than 0.");
            }

            var user = await _context.Users.FindAsync(GetAuthUserId());
            Console.WriteLine($"Username: {user.FirstName}");
            if (user == null)
            {
                return NotFound("User not found.");
            }

            TransactionRequest transactionRequest = new TransactionRequest
            {
                Type = TransactionType.Credit,
                Amount = amount,
                User = user
                
            };
            TransactionRequest transactionRequestSystem = new TransactionRequest
            {
                Type = TransactionType.Debit,
                Amount = amount,
                User = new User { Id = 2 }

            };           
            

            Deposits deposits = new Deposits
            {
                TransactionId = Credit(transactionRequest).Id,
                SourceId = "None for now"
            };
            Transfers transfers = new Transfers
            {
                CreditId = deposits.TransactionId,
                DebitId = Debit(transactionRequestSystem).Id
            };
            _context.Deposits.Add(deposits);
            _context.Transfers.Add(transfers);
            _context.SaveChanges();
            user.Balance += amount;

            return Ok(new { message = "Deposit successful.", balance = user.Balance });
        }


        [HttpGet]
        [Authorize]
        public IActionResult GetUser(string username)
        {
            try
            {
                User output = _context.Users.Where(u => u.Username == username).First();
                output.PasswordHash = "";
                output.Salt = "";
                return Json(output);
            }
            catch (Exception ex)
            {
                return BadRequest("Username doesn't exist");
            }
        }



        [Authorize]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
        {
            var receiver = _context.Users.Where(i => i.Username == request.To).First();
            var sender = await _context.Users.FindAsync(GetAuthUserId());


            if (sender == null || receiver == null)
            {
                return NotFound("Sender or receiver not found.");
            }
            if (request.Amount <= 0)
            {
                return BadRequest("Transfer amount must be greater than 0.");
            }

            if (receiver.Id == GetAuthUserId())
            {
                return BadRequest("Cannot transfer to the same user.");
            }

            

            if (sender.Balance < request.Amount)
            {
                return BadRequest("Insufficient balance.");
            }

            var transferReq = new TransactionRequest 
            {
                Amount = request.Amount,
                Type = TransactionType.Debit,
                User = sender
            };
            var transaction = Debit(transferReq);

            Transfers transfer = new Transfers
            {
                DebitId = transaction.Id
            };

            transferReq.User = receiver;
            transferReq.Type = TransactionType.Credit;       

            transfer.CreditId = Credit(transferReq).Id;

            _context.Transfers.Add(transfer);
            _context.SaveChanges();

            return Ok(new { message = "Transfer successful.", transaction});
        }



        [Authorize]
        private Transaction Credit([FromBody] TransactionRequest request)
        {
            int count = _context.Money.Where(i => i.UserId == 2).Count();
            if((count * 50) < request.Amount)
            {
                throw new InvalidOperationException("The system cant accomodate this transaction");
            }
            foreach (Money money in GetUserMoney(2, request.Amount))
            {
                money.UserId = request.User.Id;
            }
            Transaction output = new Transaction
            {
                Amount = request.Amount,
                Type = Models.TransactionType.Credit,
                UserId= request.User.Id               

            };
            _context.Transactions.Add(output);
            _context.SaveChanges();

            return output;
        }



        private IQueryable<Money> GetUserMoney(int userid, decimal amount)
        {
            int count = 0;
            if ((amount % 50) != 0)
            {
                throw new InvalidOperationException();
            }
            count = Convert.ToInt32(amount) / 50;
            return _context.Money.Where(i => i.UserId == userid).Take(count);
        }



        private Transaction Debit([FromBody] TransactionRequest request)
        {
            int count = _context.Money.Where(i => i.UserId == request.User.Id).Count();
            if ((count * 50) < request.Amount)
            {
                throw new InvalidOperationException("There is no enough balance");
            }
            foreach (Money money in GetUserMoney(request.User.Id, request.Amount))
            {
                money.UserId = 2;
            }
            Transaction output = new Transaction
            {
                Amount = request.Amount,
                Type = Models.TransactionType.Debit,
                UserId = request.User.Id

            };
            _context.Transactions.Add(output);
            _context.SaveChanges();

            return output;
        }
        

        private int GetAuthUserId()
        {
            return Convert.ToInt32(User.FindFirst("id")?.Value);
        }



        public async Task<IActionResult> Withdraw(decimal amount)
        {
            if (amount <= 0)
            {
                return BadRequest("Withdraw amount must be greater than 0.");
            }

            var user = await _context.Users.FindAsync(GetAuthUserId());
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.Balance < amount)
            {
                return BadRequest("Insufficient balance.");
            }

            TransactionRequest transactionRequest = new TransactionRequest
            {
                Type = TransactionType.Debit,
                Amount = amount,
                User = user

            };
            TransactionRequest transactionRequestSystem = new TransactionRequest
            {
                Type = TransactionType.Debit,
                Amount = amount,
                User = new User { Id = 2 }

            };

            Withdraws withdraw = new Withdraws
            {
                TransactionId = Debit(transactionRequest).Id,
                DestinationId = "None for now"
               
            };
            Transfers transfers = new Transfers
            {
                DebitId = withdraw.TransactionId,
                CreditId = Credit(transactionRequestSystem).Id
            };
            _context.Withdraws.Add(withdraw);
            _context.Transfers.Add(transfers);
            _context.SaveChanges();
            user.Balance += amount;

            return Ok(new { message = "Withdrawal successful.", balance = user.Balance });
        }

        public async Task<IActionResult> Transactions()
        {
            List<IDisplayable> output = new();
            User user = _context.Users.Where(i => i.Id == GetAuthUserId()).First();            

            List<Deposits> deposits = _context.Deposits.Include(i => i.Transaction).Where(i => i.Transaction.User.Id == user.Id).ToList();
            if (deposits.Any())
            {
                foreach (var deposit in deposits)
                {
                    output.Add(new TransactionDataAdapter(deposit));
                }
            }
            
            List<Transfers> transfers = _context.Transfers.Where(i => (i.Credit.User.Id == user.Id || i.Debit.User.Id == user.Id)).Include(i => i.Credit).ThenInclude(i => i.User).Include(i => i.Debit).ThenInclude(i => i.User).ToList();
            if (transfers.Any())
            {
                foreach (var transfer in transfers)
                {
                    output.Add(new TransactionDataAdapter(transfer));
                }
            }
            
            //List<Transfers> outgoingTransfers = _context.Transfers.Include(i => i.Debit).Include(i => i.Credit).Where(i => i.Debit.User.Id == user.Id).ToList();
            //if (outgoingTransfers.Any())
            //{
            //    foreach (var transfer in outgoingTransfers)
            //    {
            //        output.Add(new TransactionDataAdapter(transfer));
            //    }
            //}            
            List<Withdraws> withdraws = _context.Withdraws.Include(i => i.Transaction).Where(i => i.Transaction.User.Id == user.Id).ToList();
            if (withdraws.Any())
            {
                foreach (var withdraw in withdraws)
                {
                    output.Add(new TransactionDataAdapter(withdraw));
                }
            }            
            return Json(output);
        }

    }

    public class TransactionRequest
    {
        public User User { get; set; }
        public decimal Amount { get; set; } 
        public TransactionType Type { get; set; }
    }
    public class TransferRequest
    {
        public string To { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
    public enum TransactionType
    {
        Debit,
        Credit
    }

}