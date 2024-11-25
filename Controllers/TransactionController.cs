using VB_api.Data;
using VB_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VB_api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly BankDbContext _dbContext;
        public TransactionController(BankDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> AllTransactionHistory()
        {
            return Ok(await _dbContext.Transaction.ToListAsync());
        }

        [HttpGet("{accountID}")]
        public async Task<IActionResult> AccountTransactionHistory([FromRoute] long accountID)
        {
            var account = await _dbContext.Account.FindAsync(accountID);
            if (account == null)
            {
                return BadRequest("No account found");
            }

            var transaction = await _dbContext.Transaction.Where(x => x.AccountID == accountID).ToListAsync();
            return Ok(transaction);
        }

        [HttpGet("{accountID}")]
        public async Task<IActionResult> CheckBalance([FromRoute] long accountID)
        {
            var account = await _dbContext.Account.FindAsync(accountID);
            
            if (account == null)
            {
                return BadRequest("No account found");
            }

            var accountBalance = account.AccountBalance;
            return Ok("Account Balance: " + accountBalance);
        }

        [HttpPost]
        public async Task<IActionResult> Deposit([FromBody] DepositWithdraw request)
        {
            var account = await _dbContext.Account.FindAsync(request.AccountID);
            if (account == null)
            {
                return BadRequest("No account found");
            }

            account.AccountBalance += request.Amount;

            var transaction = new Transaction()
            {
                AccountID = request.AccountID,
                TransactionTypeName = "Deposit",
                TransactionDate = DateTime.Now,
                TransactionExplanation = request.TransactionExplanation
            };

            await _dbContext.Transaction.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();
            return Ok(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw([FromBody] DepositWithdraw request)
        {
            var account = await _dbContext.Account.FindAsync(request.AccountID);
            if (account == null)
            {
                return BadRequest("No account found");
            }
            if (account.AccountBalance < request.Amount)
            {
                return BadRequest("There is not enough balance in the account.");
            }

            account.AccountBalance -= request.Amount;

            var transaction = new Transaction()
            {
                AccountID = request.AccountID,
                TransactionTypeName = "Withdraw",
                TransactionDate = DateTime.Now,
                TransactionExplanation = request.TransactionExplanation
            };

            await _dbContext.Transaction.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();
            return Ok(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> TransferMoney([FromBody] TransferRequest request)
        {
            // Transaction başlat
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var sourceAccount = await _dbContext.Account.FindAsync(request.SourceAccountID);
                if (sourceAccount == null)
                {
                    return BadRequest("No source account found.");
                }

                var targetAccount = await _dbContext.Account.FindAsync(request.TargetAccountID);
                if (targetAccount == null)
                {
                    return BadRequest("No target account found.");
                }

                if (sourceAccount.AccountBalance < request.Amount)
                {
                    return BadRequest("There is not enough balance in the source account.");
                }

                sourceAccount.AccountBalance -= request.Amount;
                targetAccount.AccountBalance += request.Amount;

                var transactionEntity = new Transaction
                {
                    AccountID = request.SourceAccountID,
                    TransactionTypeName = "Transfer",
                    TransactionAmount = request.Amount,
                    TransactionDate = DateTime.Now,
                    TransactionExplanation = request.Explanation
                };

                await _dbContext.Transaction.AddAsync(transactionEntity);
                await _dbContext.SaveChangesAsync();

                var transfer = new Transfer
                {
                    TransactionID = transactionEntity.TransactionID,
                    SourceAccountID = request.SourceAccountID,
                    TargetAccountID = request.TargetAccountID,
                };

                await _dbContext.Transfer.AddAsync(transfer);
                await _dbContext.SaveChangesAsync();

                // Transaction commit
                await transaction.CommitAsync();

                return Ok(new { transactionEntity, transfer });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"An error occurred during the operation: {ex.Message}");
            }
        }

    }
}
