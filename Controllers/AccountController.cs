using VB_api.Data;
using VB_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace VB_api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly BankDbContext _dbContext;
        public AccountController(BankDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> DisplayAllAccounts()
        {
            var account = await _dbContext.Account.ToListAsync();
            return Ok(new {account});
        }

        [HttpGet("{customerID}")]
        public async Task<IActionResult> DisplayAccounts([FromRoute] long customerID)
        {
            var customer = await _dbContext.Customer.FindAsync(customerID);
            if (customer == null)
            {
                return NotFound(new { message = "Customer could not be found." });
            }

            var account = await _dbContext.Account.Where(x => x.CustomerID == customerID).ToListAsync();
            return Ok(new {account});
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] AccountRequest request)
        {
            var customer = await _dbContext.Customer.FindAsync(request.CustomerID);
            if (customer == null)
            {
                return NotFound(new { message = "Customer could not be found." });
            }

            var account = new Account()
            {
                CustomerID = request.CustomerID,  
                AccountName = request.AccountName,
                CurrencyType = "TL",
                AccountBalance = 0
            };

            await _dbContext.Account.AddAsync(account);
            await _dbContext.SaveChangesAsync();

            return Ok(new {message = "Account Created", accountID = account.AccountID });
        }

        [HttpPut("{accountID}")]
        public async Task<IActionResult> UpdateAccount(long accountID, AccountRequest request)
        {
            var customer = await _dbContext.Customer.FindAsync(request.CustomerID);
            var account = await _dbContext.Account.FindAsync(accountID);

            if (customer == null)
            {
                return NotFound(new { message = "Customer could not be found." });
            }
            if (account == null)
            {
                return NotFound(new { message = "Account could not be found." });
            }

            account.CustomerID = request.CustomerID;
            account.AccountName = request.AccountName;

            await _dbContext.SaveChangesAsync();
            return Ok(new {message = "Account Updated", accountID = account.AccountID});
        }

        [HttpDelete("{accountID}")]
        public async Task<IActionResult> DeleteAccount(long accountID)
        {
            var account = await _dbContext.Account.FindAsync(accountID);

            if (account == null)
            {
                return NotFound(new { message = "Account could not be found." });
            }

            _dbContext.Remove(account);
            await _dbContext.SaveChangesAsync();
            return Ok(new {message = "Account Deleted", accountID = account.AccountID});
        }
    }
}
