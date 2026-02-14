using Application.Common.Interfaces.Customers;
using Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _CustomerService;

        public CustomerController(ICustomerService employeeService)
        {
            _CustomerService = employeeService;
        }
        [HttpGet("GetAllCustomer/{custRegId}/{custTypeId}")]
        public async Task<IActionResult> GetAllCustomer(int custRegId, int custTypeId)
        {
            var result = await _CustomerService.GetAllCustomer(custRegId, custTypeId);
            return Ok(result);
        }
        [HttpGet("GetCustRegType")]
        public async Task<IActionResult> GetCustRegType()
        {
            var result = await _CustomerService.GetCustRegType();
            return Ok(result);
        }
        [HttpGet("GetCustomerType")]
        public async Task<IActionResult> GetCustomerType()
        {
            var result = await _CustomerService.GetCustomerType();
            return Ok(result);
        }
        [HttpGet("GetCustomerById/{CustId}")]
        public async Task<IActionResult> GetCustomerById(int CustId)
        {
            var result = await _CustomerService.GetCustomerById(CustId);
            return Ok(result);
        }
        [HttpPost("AddCustomer")]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerDTO dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Invalid data" });

            var result = await _CustomerService.AddCustomer(dto);
            return Ok(result);
        }
        [HttpPost("UpdateCustomer")]
        public async Task<IActionResult> UpdateCustomer([FromBody] CustomerDTO dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Invalid data" });

            var result = await _CustomerService.UpdateCustomer(dto);
            return Ok(result);
        }

        [HttpDelete("DeleteCustomer/{CustId}")]
        public async Task<IActionResult> DeleteCustomer(int CustId)
        {

            var result = await _CustomerService.DeleteCustomer(CustId);
            return Ok(result);
        }
        [HttpGet("UpdateArchivedById/{CustId}")]
        public async Task<IActionResult> UpdateArchivedById(int CustId)
        {
            var result = await _CustomerService.UpdateArchivedById(CustId);
            return Ok(result);
        }

        [HttpGet("GetArchivedList")]
        public async Task<IActionResult> GetArchivedList()
        {
            var result = await _CustomerService.GetArchivedList();
            return Ok(result);
        }
    }
}
