using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RDPTimeWebApp.Models.Sigur;

namespace RDPTimeWebApp.DbContexts
{
    public partial class SigurMainContext : DbContext
    {
        public SigurMainContext()
        {
        }

        public SigurMainContext(DbContextOptions<SigurMainContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Personal> Personal { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Personal>(entity =>
            {
                entity.ToTable("personal");

                entity.HasIndex(e => e.Codekey)
                    .HasName("CODEKEY");

                entity.HasIndex(e => e.Id)
                    .HasName("ID");

                entity.HasIndex(e => e.ParentId)
                    .HasName("PARENT_ID");

                entity.HasIndex(e => e.Status)
                    .HasName("STATUS");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Description)
                    .HasColumnName("DESCRIPTION")
                    .HasColumnType("mediumtext")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Name)
                    .HasColumnName("NAME")
                    .HasColumnType("varchar(200)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.ParentId)
                    .HasColumnName("PARENT_ID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Pos)
                    .HasColumnName("POS")
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Status)
                    .HasColumnName("STATUS")
                    .HasColumnType("enum('AVAILABLE','FIRED')")
                    .HasDefaultValueSql("'AVAILABLE'")
                    .HasCharSet("latin1")
                    .HasCollation("latin1_swedish_ci");

                entity.Property(e => e.TabId)
                    .HasColumnName("TABID")
                    .HasColumnType("mediumtext")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Type)
                    .HasColumnName("TYPE")
                    .HasColumnType("enum('DEP','EMP')")
                    .HasCharSet("latin1")
                    .HasCollation("latin1_swedish_ci");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
