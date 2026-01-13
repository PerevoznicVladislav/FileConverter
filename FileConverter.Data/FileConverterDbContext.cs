using System;
using System.Collections.Generic;
using FileConverter.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FileConverter.Data;

public partial class FileConverterDbContext : DbContext
{
    public FileConverterDbContext()
    {
    }

    public FileConverterDbContext(DbContextOptions<FileConverterDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Conversion> Conversions { get; set; }

    public virtual DbSet<MonthlyUsage> MonthlyUsages { get; set; }

    public virtual DbSet<Plan> Plans { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserPlan> UserPlans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-TD7LPCG;Database=FileConverter;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversion>(entity =>
        {
            entity.HasKey(e => e.ConversionId).HasName("PK__Conversi__E4E07B3F8969CC21");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.User).WithMany(p => p.Conversions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversions_Users");
        });

        modelBuilder.Entity<MonthlyUsage>(entity =>
        {
            entity.HasKey(e => e.MonthlyUsageId).HasName("PK__MonthlyU__BFE86465821FB67E");

            entity.HasOne(d => d.User).WithMany(p => p.MonthlyUsages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MonthlyUsage_Users");
        });

        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__Plans__BE9F8F1D0E389267");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370FBAE22FAA");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<UserPlan>(entity =>
        {
            entity.HasKey(e => e.UserPlanId).HasName("PK__UserPlan__7E75D75BB299C1AC");

            entity.Property(e => e.StartDate).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Plan).WithMany(p => p.UserPlans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserPlan_Plans");

            entity.HasOne(d => d.User).WithOne(p => p.UserPlan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserPlan_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
