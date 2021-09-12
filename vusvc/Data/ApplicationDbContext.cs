using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using vusvc.Models;

namespace vusvc.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Player> Players { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
