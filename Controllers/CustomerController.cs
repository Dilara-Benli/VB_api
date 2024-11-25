using VB_api.Data;
using VB_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VB_api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly BankDbContext _dbContext; 
        public CustomerController(BankDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> DisplayAllCustomers() 
        {
            return Ok(await _dbContext.Customer.ToListAsync());
        }

        [HttpGet("{customerID}")]
        public async Task<IActionResult> DisplayCustomer([FromRoute] long customerID) 
        {
            var customer = await _dbContext.Customer.FindAsync(customerID);
            if (customer == null)
            {
                return BadRequest("No customer found");
            }

            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] AddUpdateCustomer request) 
        {
            var customer = new Customer()
            {
                CustomerName = request.CustomerName,
                CustomerLastName = request.CustomerLastName,
                CustomerBirthDate = request.CustomerBirthDate,
                CustomerIdentityNumber = request.CustomerIdentityNumber
            };

            await _dbContext.Customer.AddAsync(customer);
            await _dbContext.SaveChangesAsync();

            return Ok(customer);
        }

        [HttpPut("{customerID}")]
        public async Task<IActionResult> UpdateCustomer(long customerID, AddUpdateCustomer request)
        {
            var customer = await _dbContext.Customer.FindAsync(customerID);

            if (customer == null)
            {
                return BadRequest("No customer found");
            }

            customer.CustomerName = request.CustomerName;
            customer.CustomerLastName = request.CustomerLastName;
            customer.CustomerBirthDate = request.CustomerBirthDate;
            customer.CustomerIdentityNumber = request.CustomerIdentityNumber;

            await _dbContext.SaveChangesAsync();
            return Ok(customer);
        }

        [HttpDelete("{customerID}")]
        public async Task<IActionResult> DeleteCustomer(long customerID)
        {
            var customer = await _dbContext.Customer.FindAsync(customerID);

            if (customer == null)
            {
                return BadRequest("No customer found");
            }

            _dbContext.Remove(customer);
            await _dbContext.SaveChangesAsync();
            return Ok(customer);
        }
    }
}
