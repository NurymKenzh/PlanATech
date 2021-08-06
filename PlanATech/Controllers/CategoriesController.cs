using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LoggerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PlanATech.Models;

namespace PlanATech.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerManager _logger;
        private readonly IConfiguration _configuration;

        public CategoriesController(ApplicationDbContext context,
            ILoggerManager logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>
        /// Returns all categories
        /// </returns>
        // GET: api/Categories
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategory()
        {
            // to show
            //throw new AccessViolationException("Violation Exception while accessing the resource.");
            // to show
            //throw new Exception("Exception while fetching all categories.");
            _logger.LogInfo("Fetching all categories");
            var categories = await _context.Category.ToListAsync();
            _logger.LogInfo($"Returning {categories.Count} category (-es).");
            return categories;
        }

        /// <summary>
        /// Get one category
        /// </summary>
        /// <param name="id">
        /// Id of the category to be obtained
        /// </param>
        /// <returns>
        /// Category with provided Id
        /// </returns>
        // GET: api/Categories/5
        [HttpGet("{id}")]
        //[Authorize(Roles = "Administrator")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            _logger.LogInfo($"Fetching category with Id = {id}");
            var category = await _context.Category.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        /// <summary>
        /// Edit existing category
        /// </summary>
        /// <param name="id">
        /// Id of the category to be changed
        /// </param>
        /// <param name="category"></param>
        // PUT: api/Categories/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            _logger.LogInfo($"Change category with Id = {id}");
            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
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
        /// Create new category
        /// </summary>
        /// <param name="category"></param>
        /// <returns>
        /// Returns created category
        /// </returns>
        // POST: api/Categories
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            _logger.LogInfo($"Add category with name = {category.Name}");
            _context.Category.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }

        /// <summary>
        /// Delete existing category
        /// </summary>
        /// <param name="id">
        /// Id of the category to be deleted
        /// </param>
        /// <returns>
        /// Deleted category
        /// </returns>
        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<Category>> DeleteCategory(int id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _logger.LogInfo($"Delete category with Id = {category.Id}");
            _context.Category.Remove(category);
            await _context.SaveChangesAsync();

            return category;
        }

        private bool CategoryExists(int id)
        {
            return _context.Category.Any(e => e.Id == id);
        }

        [HttpPost]
        [Route("UploadCategoryFile")]
        public async Task<ActionResult<string>> UploadCategoryFile()
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
                    var result = client.PostAsync($"{_configuration.GetValue<string>("UploadFilesService")}Files/UploadCategoryFile", multipartContent).Result;
                    string message = await result.Content.ReadAsStringAsync();

                    _logger.LogInfo(message);

                    return Ok(new { message });
                }
            }
        }

        //[HttpGet("{id}")]
        //[Route("GetUploadFileStatus")]
        //public async Task<ActionResult<Category>> GetUploadFileStatus(int id)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        var result = client.GetAsync($"{_configuration.GetValue<string>("UploadFilesService")}Files/{id}").Result;
        //        string message = await result.Content.ReadAsStringAsync();

        //        _logger.LogInfo(message);

        //        return Ok(new { message });
        //    }
        //}

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
