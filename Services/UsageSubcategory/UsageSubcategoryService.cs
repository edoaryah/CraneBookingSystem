using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.ViewModels.UsageManagement;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Services
{
  public class UsageSubcategoryService : IUsageSubcategoryService
  {
    private readonly AppDbContext _context;
    private readonly ILogger<UsageSubcategoryService> _logger;

    public UsageSubcategoryService(AppDbContext context, ILogger<UsageSubcategoryService> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<IEnumerable<UsageSubcategoryViewModel>> GetAllUsageSubcategoriesAsync()
    {
      var subcategories = await _context.UsageSubcategories
          .OrderBy(s => s.Category)
          .ThenBy(s => s.Name)
          .ToListAsync();

      return subcategories.Select(s => new UsageSubcategoryViewModel
      {
        Id = s.Id,
        Category = s.Category,
        Name = s.Name,
        Description = s.Description,
        IsActive = s.IsActive
      });
    }

    public async Task<UsageSubcategoryViewModel> GetUsageSubcategoryByIdAsync(int id)
    {
      var subcategory = await _context.UsageSubcategories.FindAsync(id);
      if (subcategory == null)
      {
        throw new KeyNotFoundException($"Usage subcategory with ID {id} not found");
      }

      return new UsageSubcategoryViewModel
      {
        Id = subcategory.Id,
        Category = subcategory.Category,
        Name = subcategory.Name,
        Description = subcategory.Description,
        IsActive = subcategory.IsActive
      };
    }

    public async Task<UsageSubcategoryViewModel> CreateUsageSubcategoryAsync(UsageSubcategoryCreateViewModel viewModel)
    {
      // Check if a subcategory with the same name already exists in the same category
      var existingSubcategory = await _context.UsageSubcategories
          .FirstOrDefaultAsync(s => s.Category == viewModel.Category && s.Name == viewModel.Name);

      if (existingSubcategory != null)
      {
        throw new InvalidOperationException($"A subcategory with the name '{viewModel.Name}' already exists in the {viewModel.Category} category");
      }

      var subcategory = new UsageSubcategory
      {
        Category = viewModel.Category,
        Name = viewModel.Name,
        Description = viewModel.Description,
        IsActive = viewModel.IsActive
      };

      _context.UsageSubcategories.Add(subcategory);
      await _context.SaveChangesAsync();

      return new UsageSubcategoryViewModel
      {
        Id = subcategory.Id,
        Category = subcategory.Category,
        Name = subcategory.Name,
        Description = subcategory.Description,
        IsActive = subcategory.IsActive
      };
    }

    public async Task<UsageSubcategoryViewModel> UpdateUsageSubcategoryAsync(int id, UsageSubcategoryUpdateViewModel viewModel)
    {
      var subcategory = await _context.UsageSubcategories.FindAsync(id);
      if (subcategory == null)
      {
        throw new KeyNotFoundException($"Usage subcategory with ID {id} not found");
      }

      // Check if a different subcategory with the same name already exists in the same category
      var existingSubcategory = await _context.UsageSubcategories
          .FirstOrDefaultAsync(s => s.Id != id && s.Category == viewModel.Category && s.Name == viewModel.Name);

      if (existingSubcategory != null)
      {
        throw new InvalidOperationException($"A subcategory with the name '{viewModel.Name}' already exists in the {viewModel.Category} category");
      }

      // Update subcategory properties
      subcategory.Category = viewModel.Category;
      subcategory.Name = viewModel.Name;
      subcategory.Description = viewModel.Description;
      subcategory.IsActive = viewModel.IsActive;

      await _context.SaveChangesAsync();

      return new UsageSubcategoryViewModel
      {
        Id = subcategory.Id,
        Category = subcategory.Category,
        Name = subcategory.Name,
        Description = subcategory.Description,
        IsActive = subcategory.IsActive
      };
    }

    public async Task DeleteUsageSubcategoryAsync(int id)
    {
      var subcategory = await _context.UsageSubcategories.FindAsync(id);
      if (subcategory == null)
      {
        throw new KeyNotFoundException($"Usage subcategory with ID {id} not found");
      }

      // Check if the subcategory is being used in any crane usage entries
      var isInUse = await _context.CraneUsageEntries
          .AnyAsync(e => e.UsageSubcategoryId == id);

      if (isInUse)
      {
        throw new InvalidOperationException($"Cannot delete this subcategory because it is being used in crane usage records");
      }

      _context.UsageSubcategories.Remove(subcategory);
      await _context.SaveChangesAsync();
    }
  }
}
