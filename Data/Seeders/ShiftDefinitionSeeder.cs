using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Models;
using System;

namespace AspnetCoreMvcFull.Data.Seeders
{
  public static class ShiftDefinitionSeeder
  {
    public static void Seed(ModelBuilder modelBuilder)
    {
      // Seed data untuk shift definitions
      modelBuilder.Entity<ShiftDefinition>().HasData(
          new ShiftDefinition
          {
            Id = 1,
            Name = "Morning Shift",
            StartTime = new TimeSpan(7, 0, 0),  // 07:00 AM
            EndTime = new TimeSpan(15, 0, 0),   // 03:00 PM
            IsActive = true
          },
          new ShiftDefinition
          {
            Id = 2,
            Name = "Evening Shift",
            StartTime = new TimeSpan(15, 0, 0), // 03:00 PM
            EndTime = new TimeSpan(23, 0, 0),   // 11:00 PM
            IsActive = true
          },
          new ShiftDefinition
          {
            Id = 3,
            Name = "Night Shift",
            StartTime = new TimeSpan(23, 0, 0), // 11:00 PM
            EndTime = new TimeSpan(7, 0, 0),    // 07:00 AM
            IsActive = true
          },
          new ShiftDefinition
          {
            Id = 4,
            Name = "First Half Day",
            StartTime = new TimeSpan(7, 0, 0),  // 07:00 AM
            EndTime = new TimeSpan(11, 0, 0),   // 11:00 AM
            IsActive = true
          },
          new ShiftDefinition
          {
            Id = 5,
            Name = "Second Half Day",
            StartTime = new TimeSpan(13, 0, 0), // 01:00 PM
            EndTime = new TimeSpan(17, 0, 0),   // 05:00 PM
            IsActive = true
          }
      );
    }
  }
}
