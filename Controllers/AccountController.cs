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
            return Ok(await _dbContext.Account.ToListAsync());
        }

        [HttpGet("{customerID}")]
        public async Task<IActionResult> DisplayAccounts([FromRoute] long customerID)
        {
            var customer = await _dbContext.Customer.FindAsync(customerID);
            if (customer == null)
            {
                return BadRequest("No customer found");
            }

            var account = await _dbContext.Account.Where(x => x.CustomerID == customerID).ToListAsync();
            return Ok(account);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] AddUpdateAccount request)
        {
            var customer = await _dbContext.Customer.FindAsync(request.CustomerID);
            if (customer == null)
            {
                return BadRequest("No customer found");
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

            return Ok(account);
        }

        [HttpPut("{accountID}")]
        public async Task<IActionResult> UpdateAccount(long accountID, AddUpdateAccount request)
        {
            var customer = await _dbContext.Customer.FindAsync(request.CustomerID);
            var account = await _dbContext.Account.FindAsync(accountID);

            if (customer == null)
            {
                return BadRequest("No customer found");
            }
            if (account == null)
            {
                return BadRequest("No account found");
            }

            account.CustomerID = request.CustomerID;
            account.AccountName = request.AccountName;

            await _dbContext.SaveChangesAsync();
            return Ok(account);
        }

        [HttpDelete("{accountID}")]
        public async Task<IActionResult> DeleteAccount(long accountID)
        {
            var account = await _dbContext.Account.FindAsync(accountID);

            if (account == null)
            {
                return BadRequest("No account found");
            }

            _dbContext.Remove(account);
            await _dbContext.SaveChangesAsync();
            return Ok(account);
        }
    }
}
