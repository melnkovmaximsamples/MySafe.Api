using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MySafe.Data.Entities;

namespace MySafe.Data.EF
{
    public sealed class ApplicationContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        private DbSet<ApplicationUser> Users { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> dbContext)
            :base(dbContext)
        {
            Database.EnsureCreated();
            SaveChanges();
        }
    }
}
