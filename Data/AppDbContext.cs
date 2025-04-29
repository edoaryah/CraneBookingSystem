using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Models.Auth;
using AspnetCoreMvcFull.Models.Role;
using AspnetCoreMvcFull.Data.Seeders;
using AspnetCoreMvcFull.Models;


namespace AspnetCoreMvcFull.Data
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Crane> Cranes { get; set; }
    public DbSet<Breakdown> Breakdowns { get; set; }

    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // Configure Enum conversions for CraneStatus
      modelBuilder.Entity<Crane>()
          .Property(c => c.Status)
          .HasConversion<string>();

      // Configure Enum conversions for CraneOwnership
      modelBuilder.Entity<Crane>()
          .Property(c => c.Ownership)
          .HasConversion<string>();

      // Relasi Crane dan Breakdown
      modelBuilder.Entity<Breakdown>()
          .HasOne(u => u.Crane)
          .WithMany(c => c.Breakdowns)
          .HasForeignKey(u => u.CraneId)
          .OnDelete(DeleteBehavior.Cascade);

      // Configure UserRole entity
      modelBuilder.Entity<UserRole>(entity =>
      {
        entity.ToTable("UserRoles");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.LdapUser).IsRequired().HasMaxLength(50);
        entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
        entity.Property(e => e.Notes).HasMaxLength(255);
        entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
        entity.Property(e => e.UpdatedBy).HasMaxLength(100);

        // Create a unique index on LdapUser + RoleName
        entity.HasIndex(e => new { e.LdapUser, e.RoleName }).IsUnique();
      });

      RoleSeeder.Seed(modelBuilder);
    }
  }
}
