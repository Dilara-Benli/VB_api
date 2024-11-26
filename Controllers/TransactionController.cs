using VB_api.Data;
using VB_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Security.Principal;

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
            var transaction = await _dbContext.Transaction.ToListAsync();
            return Ok(new {transaction});
        }

        [HttpGet("{accountID}")]
        public async Task<IActionResult> AccountTransactionHistory([FromRoute] long accountID)
        {
            var account = await _dbContext.Account.FindAsync(accountID);
            if (account == null)
            {
                return NotFound(new { message = "Account could not be found." });
            }

            var transaction = await _dbContext.Transaction.Where(x => x.AccountID == accountID).ToListAsync();
            return Ok(new {transaction});
        }

        [HttpGet("{accountID}")]
        public async Task<IActionResult> CheckBalance([FromRoute] long accountID)
        {
            var account = await _dbContext.Account.FindAsync(accountID);
            
            if (account == null)
            {
                return NotFound(new { message = "Account could not be found." });
            }

            var accountBalance = account.AccountBalance;
            return Ok(new {balance = accountBalance});
        }

        [HttpPost]
        public async Task<IActionResult> Deposit([FromBody] TransactionRequest request)
        {
            var account = await _dbContext.Account.FindAsync(request.AccountID);
            if (account == null)
            {
                return NotFound(new { message = "Account could not be found." });
            }

            account.AccountBalance += request.Amount;

            var transaction = new Transaction()
            {
                AccountID = request.AccountID,
                TransactionTypeName = "Deposit",
                TransactionAmount = request.Amount,
                TransactionDate = DateTime.Now,
                TransactionExplanation = request.Explanation
            };

            await _dbContext.Transaction.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();
            return Ok(new {message = "Deposit Successful", transactionId = transaction.TransactionID, 
                newBalance = account.AccountBalance });
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw([FromBody] TransactionRequest request)
        {
            var account = await _dbContext.Account.FindAsync(request.AccountID);
            if (account == null)
            {
                return NotFound(new { message = "Account could not be found." });
            }
            if (account.AccountBalance < request.Amount)
            {
                return BadRequest(new { message = "There is not enough balance in the account." });
            }

            account.AccountBalance -= request.Amount;

            var transaction = new Transaction()
            {
                AccountID = request.AccountID,
                TransactionTypeName = "Withdraw",
                TransactionAmount = request.Amount,
                TransactionDate = DateTime.Now,
                TransactionExplanation = request.Explanation
            };

            await _dbContext.Transaction.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();

            return Ok(new {message = "Withdraw Successful", transactionId = transaction.TransactionID, 
                newBalance = account.AccountBalance});
        }

        [HttpPost]
        public IActionResult TransferMoney(TransferRequest request)
        {
            if (request.SourceAccountID == request.TargetAccountID)
            {
                return BadRequest(new { message = "Source and target accounts cannot be the same." });
            }

            //var sourceAccount = await _dbContext.Account.FindAsync(request.SourceAccountID);
            var sourceAccount = _dbContext.Account.SingleOrDefault(a => a.AccountID == request.SourceAccountID);
            var targetAccount = _dbContext.Account.SingleOrDefault(a => a.AccountID == request.TargetAccountID);

            if (sourceAccount == null || targetAccount == null)
            {
                return NotFound(new { message = "One or both accounts could not be found." });
            }

            if (sourceAccount.AccountBalance < request.Amount)
            {
                return BadRequest(new { message = "There is not enough balance in the source account." });
            }

            using var transaction = _dbContext.Database.BeginTransaction();
            try
            {
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

                _dbContext.Transaction.Add(transactionEntity);
                _dbContext.SaveChanges();

                var transfer = new Transfer
                {
                    TransactionID = transactionEntity.TransactionID,
                    SourceAccountID = request.SourceAccountID,
                    TargetAccountID = request.TargetAccountID,
                };

                _dbContext.Transfer.Add(transfer);
                _dbContext.SaveChanges();

                transaction.Commit();

                return Ok(new {message = "Transfer successful.", transferID = transfer.TransferID, 
                    fromAccountNewBalance = sourceAccount.AccountBalance, toAccountNewBalance = targetAccount.AccountBalance});
            }
            catch
            {
                transaction.Rollback();
                return StatusCode(500, new { message = "An error occurred during the transfer." });
            }
        }

        /*
        [HttpPost]
        public async Task<IActionResult> TransferMoney1([FromBody] TransferRequest request)
        {
            // Transaction başlat
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.SourceAccountID == request.TargetAccountID)
                {
                    return BadRequest(new { message = "Source and target accounts cannot be the same." });
                }

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
        */
    }
}
