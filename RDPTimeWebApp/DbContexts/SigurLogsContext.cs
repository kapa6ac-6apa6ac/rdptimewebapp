using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RDPTimeWebApp.Models.Sigur;

namespace RDPTimeWebApp.DbContexts
{
    public partial class SigurLogsContext : DbContext
    {
        public SigurLogsContext()
        {
        }

        public SigurLogsContext(DbContextOptions<SigurLogsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Logs> Logs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Logs>(entity =>
            {
                entity.ToTable("logs");

                entity.HasIndex(e => e.DevHint)
                    .HasName("DEVHINT");

                entity.HasIndex(e => e.EmpHint)
                    .HasName("EMPHINT");

                entity.HasIndex(e => e.LogTime)
                    .HasName("LOGTIME");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Area)
                    .HasColumnName("AREA")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DevHint)
                    .HasColumnName("DEVHINT")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'-1'");

                entity.Property(e => e.EmpHint)
                    .HasColumnName("EMPHINT")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'-1'");

                entity.Property(e => e.Framets)
                    .HasColumnName("FRAMETS")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.LogData)
                    .HasColumnName("LOGDATA")
                    .HasColumnType("tinyblob");

                entity.Property(e => e.LogTime)
                    .HasColumnName("LOGTIME")
                    .HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
