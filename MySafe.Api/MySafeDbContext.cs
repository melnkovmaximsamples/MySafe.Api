using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MySafe.Api.Models;

namespace MySafe.Api
{
    public sealed class MySafeDbContext : DbContext
    {
        private DbSet<Test> Tests { get; set; }

        public MySafeDbContext(DbContextOptions<MySafeDbContext> dbContext)
            :base(dbContext)
        {
            Database.EnsureCreated();
            Tests.Add(new Test() { Name = "HELLO!" });
            SaveChanges();
        }
    }
}
