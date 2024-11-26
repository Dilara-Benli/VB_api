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
            var customer = await _dbContext.Customer.ToListAsync();
            return Ok(new {customer});
        }

        [HttpGet("{customerID}")]
        public async Task<IActionResult> DisplayCustomer([FromRoute] long customerID) 
        {
            var customer = await _dbContext.Customer.FindAsync(customerID);
            if (customer == null)
            {
                return NotFound(new { message = "Customer could not be found." });
            }

            return Ok(new {customer});
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerRequest request) 
        {
            if (_dbContext.Customer.Any(c => c.CustomerIdentityNumber == request.CustomerIdentityNumber))
            {
                return Conflict(new { message = "Customer with the given identity number already exists." });
                //return BadRequest(new { message = "There is not enough balance in the account." });
            }

            var customer = new Customer()
            {
                CustomerName = request.CustomerName,
                CustomerLastName = request.CustomerLastName,
                CustomerBirthDate = request.CustomerBirthDate,
                CustomerIdentityNumber = request.CustomerIdentityNumber
            };

            await _dbContext.Customer.AddAsync(customer);
            await _dbContext.SaveChangesAsync();

            return Ok(new {message = "Customer Created", customerID = customer.CustomerID});
        }

        [HttpPut("{customerID}")]
        public async Task<IActionResult> UpdateCustomer(long customerID, CustomerRequest request)
        {
            var customer = await _dbContext.Customer.FindAsync(customerID);

            if (customer == null)
            {
                return NotFound(new { message = "Customer could not be found." });
            }

            customer.CustomerName = request.CustomerName;
            customer.CustomerLastName = request.CustomerLastName;
            customer.CustomerBirthDate = request.CustomerBirthDate;
            customer.CustomerIdentityNumber = request.CustomerIdentityNumber;

            await _dbContext.SaveChangesAsync();
            return Ok(new {message = "Customer Updated", customerID = customer.CustomerID});
        }

        [HttpDelete("{customerID}")]
        public async Task<IActionResult> DeleteCustomer(long customerID)
        {
            var customer = await _dbContext.Customer.FindAsync(customerID);

            if (customer == null)
            {
                return NotFound(new { message = "Customer could not be found." });
            }

            _dbContext.Remove(customer);
            await _dbContext.SaveChangesAsync();
            return Ok(new {message = "Customer Deleted", customerID = customer.CustomerID});
        }
    }
}
