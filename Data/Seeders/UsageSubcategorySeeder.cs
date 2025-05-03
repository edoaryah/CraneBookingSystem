using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Data.Seeders
{
  public static class UsageSubcategorySeeder
  {
    public static void Seed(ModelBuilder modelBuilder)
    {
      // Operating subcategories
      modelBuilder.Entity<UsageSubcategory>().HasData(
          new UsageSubcategory { Id = 1, Category = UsageCategory.Operating, Name = "Production Lifting", Description = "Crane used for production lifting operations", IsActive = true },
          new UsageSubcategory { Id = 2, Category = UsageCategory.Operating, Name = "Equipment Installation", Description = "Crane used for equipment installation", IsActive = true },
          new UsageSubcategory { Id = 3, Category = UsageCategory.Operating, Name = "Material Handling", Description = "Crane used for general material handling", IsActive = true }
      );

      // Delay subcategories
      modelBuilder.Entity<UsageSubcategory>().HasData(
          new UsageSubcategory { Id = 4, Category = UsageCategory.Delay, Name = "Weather", Description = "Delay due to bad weather conditions", IsActive = true },
          new UsageSubcategory { Id = 5, Category = UsageCategory.Delay, Name = "Planning", Description = "Delay due to planning or coordination issues", IsActive = true },
          new UsageSubcategory { Id = 6, Category = UsageCategory.Delay, Name = "Operator Break", Description = "Scheduled operator break time", IsActive = true },
          new UsageSubcategory { Id = 7, Category = UsageCategory.Delay, Name = "Refueling", Description = "Delay for refueling operations", IsActive = true }
      );

      // Standby subcategories
      modelBuilder.Entity<UsageSubcategory>().HasData(
          new UsageSubcategory { Id = 8, Category = UsageCategory.Standby, Name = "On-site Standby", Description = "Crane on standby at work site", IsActive = true },
          new UsageSubcategory { Id = 9, Category = UsageCategory.Standby, Name = "No Work Assignment", Description = "Crane available but not assigned any tasks", IsActive = true },
          new UsageSubcategory { Id = 10, Category = UsageCategory.Standby, Name = "Scheduled Standby", Description = "Planned standby period", IsActive = true }
      );

      // Service subcategories
      modelBuilder.Entity<UsageSubcategory>().HasData(
          new UsageSubcategory { Id = 11, Category = UsageCategory.Service, Name = "Scheduled Maintenance", Description = "Regular scheduled maintenance", IsActive = true },
          new UsageSubcategory { Id = 12, Category = UsageCategory.Service, Name = "Inspection", Description = "Safety or regulatory inspection", IsActive = true },
          new UsageSubcategory { Id = 13, Category = UsageCategory.Service, Name = "Component Replacement", Description = "Planned replacement of components", IsActive = true },
          new UsageSubcategory { Id = 14, Category = UsageCategory.Service, Name = "Lubrication", Description = "Regular lubrication service", IsActive = true }
      );

      // Breakdown subcategories
      modelBuilder.Entity<UsageSubcategory>().HasData(
          new UsageSubcategory { Id = 15, Category = UsageCategory.Breakdown, Name = "Mechanical Failure", Description = "Breakdown due to mechanical problems", IsActive = true },
          new UsageSubcategory { Id = 16, Category = UsageCategory.Breakdown, Name = "Electrical Failure", Description = "Breakdown due to electrical problems", IsActive = true },
          new UsageSubcategory { Id = 17, Category = UsageCategory.Breakdown, Name = "Hydraulic Failure", Description = "Breakdown due to hydraulic system problems", IsActive = true },
          new UsageSubcategory { Id = 18, Category = UsageCategory.Breakdown, Name = "Control System Failure", Description = "Breakdown due to control system issues", IsActive = true },
          new UsageSubcategory { Id = 19, Category = UsageCategory.Breakdown, Name = "Accident", Description = "Breakdown due to accident or incident", IsActive = true }
      );
    }
  }
}
