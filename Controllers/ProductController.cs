using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedisCacheDemo.Common;
using RedisCacheDemo.Models;
using RedisCacheDemo.Services;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;

namespace RedisCacheDemo.Controllers
{
    [Route("api/product/[action]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly RedisCacheService _redisCacheService;

        public ProductController(AppDbContext dbContext, RedisCacheService redisCacheService)
        {
            _dbContext = dbContext;
            _redisCacheService = redisCacheService;
        }

        // Lấy tất cả danh sách sản phẩm
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var stopwatch = Stopwatch.StartNew();
            string cacheKey = "products";

            // Kiểm tra cache trước
            var cachedProducts = await _redisCacheService.GetAsync<List<Product>>(cacheKey);
            if (cachedProducts != null)
            {
                stopwatch.Stop();
                return Ok(new ApiResponse<List<Product>>(cachedProducts, stopwatch.ElapsedMilliseconds, "Cache"));
            }

            // Nếu không có cache, lấy từ database
            var products = await _dbContext.Products.ToListAsync();

            // Lưu vào cache
            await _redisCacheService.SetAsync(cacheKey, products, TimeSpan.FromMinutes(1));

            stopwatch.Stop();
            return Ok(new ApiResponse<List<Product>>(products, stopwatch.ElapsedMilliseconds, "Database"));
        }

        // Lấy sản phẩm theo ID
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var stopwatch = Stopwatch.StartNew();
            string cacheKey = $"product_{id}";

            var cachedProduct = await _redisCacheService.GetAsync<Product>(cacheKey);
            if (cachedProduct != null)
            {
                stopwatch.Stop();
                return Ok(new ApiResponse<Product>(cachedProduct, stopwatch.ElapsedMilliseconds, "Cache"));
            }

            var product = await _dbContext.Products.FindAsync(id);
            if (product == null) return NotFound(new ApiResponse<string>(null, stopwatch.ElapsedMilliseconds, "Database", "Product not found", false));

            await _redisCacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(1));

            stopwatch.Stop();
            return Ok(new ApiResponse<Product>(product, stopwatch.ElapsedMilliseconds, "Database"));
        }

        // Thêm sản phẩm (Xóa cache sau khi thêm)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            var stopwatch = Stopwatch.StartNew();
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            // Xóa cache vì dữ liệu đã thay đổi
            await _redisCacheService.RemoveAsync("products");

            stopwatch.Stop();
            return Ok(new ApiResponse<Product>(product, stopwatch.ElapsedMilliseconds, "Database", "Product created"));
        }

        // Cập nhật sản phẩm (Xóa cache sau khi cập nhật)
        [HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] Product productDto)
        {
            var stopwatch = Stopwatch.StartNew();
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null) return NotFound(new ApiResponse<string>(null, stopwatch.ElapsedMilliseconds, "Database", "Product not found", false));

            product.Name = productDto.Name;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            await _dbContext.SaveChangesAsync();

            // Xóa cache vì dữ liệu đã thay đổi
            await _redisCacheService.RemoveAsync("products"); // Xóa cache danh sách
            await _redisCacheService.RemoveAsync($"product_{id}"); // Xóa cache sản phẩm

            stopwatch.Stop();
            return Ok(new ApiResponse<Product>(product, stopwatch.ElapsedMilliseconds, "Database", "Product updated"));
        }

        // Xóa sản phẩm (Xóa cache sau khi xóa)
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var stopwatch = Stopwatch.StartNew();
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null) return NotFound(new ApiResponse<string>(null, stopwatch.ElapsedMilliseconds, "Database", "Product not found", false));

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            // Xóa cache vì dữ liệu đã thay đổi
            await _redisCacheService.RemoveAsync("products"); // Xóa cache danh sách
            await _redisCacheService.RemoveAsync($"product_{id}"); // Xóa cache sản phẩm

            stopwatch.Stop();
            return Ok(new ApiResponse<string>("Deleted", stopwatch.ElapsedMilliseconds, "Database", "Product deleted"));
        }
    }
}
