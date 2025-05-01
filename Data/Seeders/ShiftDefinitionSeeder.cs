using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Models;
using System;

namespace AspnetCoreMvcFull.Data.Seeders
{
  public static class ShiftDefinitionSeeder
  {
    public static void Seed(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<ShiftDefinition>().HasData(
          new ShiftDefinition
          {
            Id = 1,
            Name = "Pagi (06:00-12:00)",
            StartTime = new TimeSpan(6, 0, 0),
            EndTime = new TimeSpan(12, 0, 0),
            Category = "Day Shift",
            IsActive = true
          },
          new ShiftDefinition
          {
            Id = 2,
            Name = "Siang (12:00-18:00)",
            StartTime = new TimeSpan(12, 0, 0),
            EndTime = new TimeSpan(18, 0, 0),
            Category = "Day Shift",
            IsActive = true
          },
          new ShiftDefinition
          {
            Id = 3,
            Name = "Sore (18:00-00:00)",
            StartTime = new TimeSpan(18, 0, 0),
            EndTime = new TimeSpan(0, 0, 0),
            Category = "Night Shift",
            IsActive = true
          },
          new ShiftDefinition
          {
            Id = 4,
            Name = "Malam (00:00-06:00)",
            StartTime = new TimeSpan(0, 0, 0),
            EndTime = new TimeSpan(6, 0, 0),
            Category = "Night Shift",
            IsActive = true
          }
      );
    }
  }
}
