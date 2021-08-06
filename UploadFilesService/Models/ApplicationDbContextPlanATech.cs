using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UploadFilesService.Models
{
    public class ApplicationDbContextPlanATech : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContextPlanATech(
            DbContextOptions<ApplicationDbContextPlanATech> options) : base(options)
        {
        }

        public DbSet<PlanATech.Models.Category> Category { get; set; }
        public DbSet<PlanATech.Models.Product> Product { get; set; }
    }
}
