using Microsoft.EntityFrameworkCore;
using RDPTimeWebApp.Models.OrionPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDPTimeWebApp.DbContexts
{
    public partial class OrionContext : DbContext
    {
        public OrionContext()
        {
        }

        public OrionContext(DbContextOptions<OrionContext> options)
            : base(options)
        {
        }

        public virtual DbSet<PList> PList { get; set; }
        public virtual DbSet<PLogData> PLogData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PList>(entity =>
            {
                entity.ToTable("pList");

                entity.HasIndex(e => e.Name)
                    .HasName("Ibx_pList_Name");

                entity.HasIndex(e => e.TabNumber)
                    .HasName("Ibx_pList_TabNumber");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.MidName)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.TabNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PLogData>(entity =>
            {
                entity.HasKey(e => e.Guid)
                    .HasName("PK__pLogData__15B69B8E3D2915A8");

                entity.ToTable("pLogData");

                entity.Property(e => e.Guid)
                    .HasColumnName("GUID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Remark)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TimeVal).HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
