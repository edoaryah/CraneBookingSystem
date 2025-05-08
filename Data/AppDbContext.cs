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

    // crane
    public DbSet<Crane> Cranes { get; set; }

    // shift
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingHazard> BookingHazards { get; set; }
    public DbSet<BookingItem> BookingItems { get; set; }
    public DbSet<BookingShift> BookingShifts { get; set; }

    // hazard & shift
    public DbSet<Hazard> Hazards { get; set; }
    public DbSet<ShiftDefinition> ShiftDefinitions { get; set; }

    // breakdown & maintenance
    public DbSet<Breakdown> Breakdowns { get; set; }
    public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
    public DbSet<MaintenanceScheduleShift> MaintenanceScheduleShifts { get; set; }

    // auth
    public DbSet<UserRole> UserRoles { get; set; }

    // crane usage records
    public DbSet<UsageSubcategory> UsageSubcategories { get; set; }
    // Tambahkan di class AppDbContext
    public DbSet<CraneUsageRecord> CraneUsageRecords { get; set; }

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

      // Configure Enum conversions for UsageCategory
      modelBuilder.Entity<CraneUsageRecord>()
          .Property(r => r.Category)
          .HasConversion<string>();

      modelBuilder.Entity<UsageSubcategory>()
          .Property(s => s.Category)
          .HasConversion<string>();

      // Relasi Crane dan Breakdown
      modelBuilder.Entity<Breakdown>()
          .HasOne(u => u.Crane)
          .WithMany(c => c.Breakdowns)
          .HasForeignKey(u => u.CraneId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relasi Crane dan Booking
      modelBuilder.Entity<Booking>()
          .HasOne(r => r.Crane)
          .WithMany()
          .HasForeignKey(r => r.CraneId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relasi Booking dan BookingShift
      modelBuilder.Entity<BookingShift>()
          .HasOne(rs => rs.Booking)
          .WithMany(r => r.BookingShifts)
          .HasForeignKey(rs => rs.BookingId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relasi BookingShift dan ShiftDefinition
      modelBuilder.Entity<BookingShift>()
          .HasOne(bs => bs.ShiftDefinition)
          .WithMany(sd => sd.BookingShifts)
          .HasForeignKey(bs => bs.ShiftDefinitionId)
          .OnDelete(DeleteBehavior.Restrict);

      // Relasi Booking dan BookingItem
      modelBuilder.Entity<BookingItem>()
          .HasOne(bi => bi.Booking)
          .WithMany(b => b.BookingItems)
          .HasForeignKey(bi => bi.BookingId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relasi BookingHazard
      modelBuilder.Entity<BookingHazard>()
          .HasOne(bh => bh.Booking)
          .WithMany(b => b.BookingHazards)
          .HasForeignKey(bh => bh.BookingId)
          .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<BookingHazard>()
          .HasOne(bh => bh.Hazard)
          .WithMany()
          .HasForeignKey(bh => bh.HazardId)
          .OnDelete(DeleteBehavior.Restrict);

      // Relasi MaintenanceSchedule dan Crane
      modelBuilder.Entity<MaintenanceSchedule>()
          .HasOne(ms => ms.Crane)
          .WithMany()
          .HasForeignKey(ms => ms.CraneId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relasi MaintenanceSchedule dan MaintenanceScheduleShift
      modelBuilder.Entity<MaintenanceScheduleShift>()
          .HasOne(mss => mss.MaintenanceSchedule)
          .WithMany(ms => ms.MaintenanceScheduleShifts)
          .HasForeignKey(mss => mss.MaintenanceScheduleId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relasi MaintenanceScheduleShift dan ShiftDefinition
      modelBuilder.Entity<MaintenanceScheduleShift>()
          .HasOne(mss => mss.ShiftDefinition)
          .WithMany()
          .HasForeignKey(mss => mss.ShiftDefinitionId)
          .OnDelete(DeleteBehavior.Restrict);

      // Tambahkan di OnModelCreating di AppDbContext
      // Relasi CraneUsageRecord dan Crane
      modelBuilder.Entity<CraneUsageRecord>()
          .HasOne(u => u.Crane)
          .WithMany()
          .HasForeignKey(u => u.CraneId)
          .OnDelete(DeleteBehavior.Cascade);

      // Relasi CraneUsageRecord dan Booking
      modelBuilder.Entity<CraneUsageRecord>()
          .HasOne(u => u.Booking)
          .WithMany()
          .HasForeignKey(u => u.BookingId)
          .OnDelete(DeleteBehavior.SetNull);

      // Relasi CraneUsageRecord dan MaintenanceSchedule
      modelBuilder.Entity<CraneUsageRecord>()
          .HasOne(u => u.MaintenanceSchedule)
          .WithMany()
          .HasForeignKey(u => u.MaintenanceScheduleId)
          .OnDelete(DeleteBehavior.SetNull);

      // Relasi CraneUsageRecord dan UsageSubcategory
      modelBuilder.Entity<CraneUsageRecord>()
          .HasOne(u => u.UsageSubcategory)
          .WithMany()
          .HasForeignKey(u => u.UsageSubcategoryId)
          .OnDelete(DeleteBehavior.Restrict);

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

      CraneSeeder.Seed(modelBuilder);
      RoleSeeder.Seed(modelBuilder);
      HazardSeeder.Seed(modelBuilder);
      ShiftDefinitionSeeder.Seed(modelBuilder);
      UsageSubcategorySeeder.Seed(modelBuilder);
    }
  }
}
