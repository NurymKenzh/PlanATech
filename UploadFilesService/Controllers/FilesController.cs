using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.FileIO;
using Npgsql;
using PlanATech.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using UploadFilesService.Models;

namespace UploadFilesService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly Models.ApplicationDbContext _context;
        private IServiceScopeFactory ServiceScopeFactory { get; set; }

        public FilesController(Models.ApplicationDbContext context,
            IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            ServiceScopeFactory = serviceScopeFactory;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UploadFile>>> GetUploadFile()
        {
            return await _context.UploadFile.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UploadFile>> GetUploadFile(int id)
        {
            var uploadFile = await _context.UploadFile.FindAsync(id);

            if (uploadFile == null)
            {
                return NotFound();
            }

            return uploadFile;
        }

        [HttpPost]
        [Route("UploadCategoryFile")]
        public async Task<ActionResult> PostCategoryFile()
        {
            try
            {
                var file = Request.Form.Files[0];
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    fileName = $"{DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fffffff")} {fileName}";
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine("Uploads", fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    UploadFile uploadFile = new UploadFile()
                    {
                        Name = fileName,
                        Finished = false
                    };
                    _context.UploadFile.Add(uploadFile);
                    await _context.SaveChangesAsync();

                    _ = Task.Run(() => ParseCategoryFileTask(uploadFile.Id));

                    string message = $"File \"{fileName}\" (Id - {uploadFile.Id}) uploaded successfully";
                    int UploadedFileId = uploadFile.Id;
                    return Ok(new { message, UploadedFileId });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        private void ParseCategoryFileTask(int id)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var _contextScoped = scope.ServiceProvider.GetRequiredService<Models.ApplicationDbContext>();
                UploadFile uploadFile = _contextScoped.UploadFile.Find(id);

                // to show
                //Thread.Sleep(new TimeSpan(0, 0, 30));

                string csv = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Uploads", uploadFile.Name));
                TextFieldParser parser = new TextFieldParser(new StringReader(csv));
                parser.HasFieldsEnclosedInQuotes = true;
                parser.SetDelimiters(",");

                var _contextPlanATechScoped = scope.ServiceProvider.GetRequiredService<ApplicationDbContextPlanATech>();
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    Category category = new Category()
                    {
                        Name = fields[0],
                        Description = fields[1]
                    };
                    _contextPlanATechScoped.Category.Add(category);
                }
                _contextPlanATechScoped.SaveChanges();
                parser.Close();

                uploadFile.Finished = true;
                _contextScoped.Entry(uploadFile).State = EntityState.Modified;
                _contextScoped.SaveChanges();
            }
        }

        [HttpPost]
        [Route("UploadProductFile")]
        public async Task<ActionResult> PostProductFile(int CategoryId)
        {
            try
            {
                var file = Request.Form.Files[0];
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    fileName = $"{DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fffffff")} {fileName}";
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine("Uploads", fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    UploadFile uploadFile = new UploadFile()
                    {
                        Name = fileName,
                        Finished = false
                    };
                    _context.UploadFile.Add(uploadFile);
                    await _context.SaveChangesAsync();

                    _ = Task.Run(() => ParseProductFileTask(uploadFile.Id, CategoryId));

                    string message = $"File \"{fileName}\" (Id - {uploadFile.Id}) uploaded successfully";
                    int UploadedFileId = uploadFile.Id;
                    return Ok(new { message, UploadedFileId });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        private void ParseProductFileTask(int id, int categoryId)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var _contextScoped = scope.ServiceProvider.GetRequiredService<Models.ApplicationDbContext>();
                UploadFile uploadFile = _contextScoped.UploadFile.Find(id);

                // to show
                //Thread.Sleep(new TimeSpan(0, 0, 30));

                string csv = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Uploads", uploadFile.Name));
                TextFieldParser parser = new TextFieldParser(new StringReader(csv));
                parser.HasFieldsEnclosedInQuotes = true;
                parser.SetDelimiters(",");

                var _contextPlanATechScoped = scope.ServiceProvider.GetRequiredService<ApplicationDbContextPlanATech>();
                Category category = _contextPlanATechScoped.Category.Find(categoryId);
                if (category == null)
                {
                    return;
                }
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    Product product = new Product()
                    {
                        CategoryId = categoryId,
                        Name = fields[0],
                        Description = fields[1],
                        Data = fields[2]
                    };
                    _contextPlanATechScoped.Product.Add(product);
                }
                _contextPlanATechScoped.SaveChanges();
                parser.Close();

                uploadFile.Finished = true;
                _contextScoped.Entry(uploadFile).State = EntityState.Modified;
                _contextScoped.SaveChanges();
            }
        }
    }
}
