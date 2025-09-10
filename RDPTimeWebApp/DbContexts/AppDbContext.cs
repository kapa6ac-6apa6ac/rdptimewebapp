using Microsoft.EntityFrameworkCore;
using RDPTimeWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.DbContexts
{
    public class AppDbContext : DbContext
    {
        public DbSet<UserModel> Users { get; set; }
        public DbSet<ConnectionModel> Connections { get; set; }
        public DbSet<ComputerModel> Computers { get; set; }
        public DbSet<VectorTimeModel> VectorTime { get; set; }

        public DbSet<CalendarDayModel> Calendar { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
#if RELEASE
            Database.Migrate();
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConnectionModel>().HasIndex(c => c.UserId);
            modelBuilder.Entity<ConnectionModel>().HasIndex(c => c.Date);
            modelBuilder.Entity<VectorTimeModel>().HasIndex(v => new { v.Year, v.Month });

            modelBuilder.Entity<CalendarDayModel>().HasIndex(d => d.Date).IsUnique(true);
        }
    }
}
