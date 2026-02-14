using Application.Common.Interfaces.Products;
using Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService employeeService)
        {
            _productService = employeeService;
        }
        [HttpGet("GetAllProduct/{StatusId}")]
        public async Task<IActionResult> GetAllProduct(int StatusId)
        {
            var result = await _productService.GetAllProduct(StatusId);
            return Ok(result);
        }
        [HttpGet("GetHSCode")]
        public async Task<IActionResult> GetHSCode()
        {
            var result = await _productService.GetHSCode();
            return Ok(result);
        }
        [HttpGet("GetSaleType")]
        public async Task<IActionResult> GetSaleType()
        {
            var result = await _productService.GetSaleType();
            return Ok(result);
        }
        [HttpGet("GetProductCount")]
        public async Task<IActionResult> GetProductCount()
        {
            var result = await _productService.GetProductCount();
            return Ok(result);
        }
        [HttpGet("GetUnitType")]
        public async Task<IActionResult> GetUnitType()
        {
            var result = await _productService.GetUnitType();
            return Ok(result);
        }
        [HttpGet("GetProductById/{ItemId}")]
        public async Task<IActionResult> GetProductById(int ItemId)
        {
            var result = await _productService.GetProductById(ItemId);
            return Ok(result);
        }
        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct([FromBody] ProductDTO dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Invalid data" });

            var result = await _productService.AddProduct(dto);
            return Ok(result);
        }
        [HttpPost("AddProductStockIn")]
        public async Task<IActionResult> AddProductStockIn([FromBody] ProductStockDTO dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Invalid data" });

            var result = await _productService.AddProductStockIn(dto);
            return Ok(result);
        }
        [HttpPost("ProductStockOut")]
        public async Task<IActionResult> ProductStockOut([FromBody] ProductStockDTO dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Invalid data" });

            var result = await _productService.ProductStockOut(dto);
            return Ok(result);
        }
        [HttpPost("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductDTO dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Invalid data" });

            var result = await _productService.UpdateProduct(dto);
            return Ok(result);
        }

        [HttpDelete("DeleteProduct/{ItemId}")]
        public async Task<IActionResult> DeleteProduct(int ItemId)
        {

            var result = await _productService.DeleteProduct(ItemId);
            return Ok(result);
        }
        [HttpGet("UpdateArchivedById/{ItemId}")]
        public async Task<IActionResult> UpdateArchivedById(int ItemId)
        {
            var result = await _productService.UpdateArchivedById(ItemId);
            return Ok(result);
        }

        [HttpGet("GetArchivedList")]
        public async Task<IActionResult> GetArchivedList()
        {
            var result = await _productService.GetArchivedList();
            return Ok(result);
        }
        [HttpPost("SaveAllProducts")]
        public async Task<IActionResult> SaveAllProducts(List<ProductDTO> list)
        {
            var res = await _productService.SaveAllProducts(list);
            return Ok(res);
        }
    }
}
