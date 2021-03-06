using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LoggerService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PlanATech.Models;

namespace PlanATech.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerManager _logger;
        private readonly IConfiguration _configuration;

        public ProductsController(ApplicationDbContext context,
            ILoggerManager logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <returns>
        /// Returns all products
        /// </returns>
        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct()
        {
            _logger.LogInfo("Fetching all products");
            var products = await _context
                .Product
                .Include(p => p.Category)
                .ToListAsync();
            _logger.LogInfo($"Returning {products.Count} product (-s).");
            return products;
        }

        /// <summary>
        /// Get one product
        /// </summary>
        /// <param name="id">
        /// Id of the product to be obtained
        /// </param>
        /// <returns>
        /// Product with provided Id
        /// </returns>
        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            _logger.LogInfo($"Fetching product with Id = {id}");
            var product = await _context.Product.FindAsync(id);
            product.Category = await _context.Category.FindAsync(product.CategoryId);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        /// <summary>
        /// Edit existing product
        /// </summary>
        /// <param name="id">
        /// Id of the product to be changed
        /// </param>
        /// <param name="product"></param>
        /// <returns></returns>
        // PUT: api/Products/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _logger.LogInfo($"Change product with Id = {id}");
            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Create new product
        /// </summary>
        /// <param name="product"></param>
        /// <returns>
        /// Returns created product
        /// </returns>
        // POST: api/Products
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _logger.LogInfo($"Add product with name = {product.Name}");
            _context.Product.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        /// <summary>
        /// Delete existing product
        /// </summary>
        /// <param name="id">
        /// Id of the product to be deleted
        /// </param>
        /// <returns>
        /// Deleted product
        /// </returns>
        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _logger.LogInfo($"Delete product with Id = {product.Id}");
            _context.Product.Remove(product);
            await _context.SaveChangesAsync();

            return product;
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }

        [HttpPost]
        [Route("UploadProductFile")]
        public async Task<ActionResult<string>> UploadProductFile(int CategoryId)
        {
            var file = Request.Form.Files[0];
            var multipartContent = new MultipartFormDataContent();

            multipartContent.Add(new StreamContent(file.OpenReadStream()),
                "File",
                ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"'));

            using (var form = new MultipartFormDataContent())
            {
                form.Add(multipartContent);
                using (var client = new HttpClient())
                {
                    var result = client.PostAsync($"{_configuration.GetValue<string>("UploadFilesService")}Files/UploadProductFile?CategoryId={CategoryId}", multipartContent).Result;
                    string message = await result.Content.ReadAsStringAsync();

                    _logger.LogInfo(message);

                    return Ok(new { message });
                }
            }
        }

        [HttpGet]
        [Route("GetUploadFileStatus")]
        public async Task<ActionResult> GetUploadFileStatus(int fileId)
        {
            using (var client = new HttpClient())
            {
                var result = client.GetAsync($"{_configuration.GetValue<string>("UploadFilesService")}Files/{fileId}").Result;
                string message = await result.Content.ReadAsStringAsync();

                _logger.LogInfo(message);

                return Ok(new { message });
            }
        }
    }
}
