using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.ViewModels.BookingManagement;
using AspnetCoreMvcFull.ViewModels.HazardManagement;
using AspnetCoreMvcFull.Events;

namespace AspnetCoreMvcFull.Services
{
  public class BookingService : IBookingService
  {
    private readonly AppDbContext _context;
    private readonly ICraneService _craneService;
    private readonly IHazardService _hazardService;
    private readonly IShiftDefinitionService _shiftDefinitionService;
    private readonly IScheduleConflictService _scheduleConflictService;
    private readonly ILogger<BookingService> _logger;
    private readonly IEmailService _emailService;
    private readonly IEmployeeService _employeeService;

    public BookingService(
        AppDbContext context,
        ICraneService craneService,
        IHazardService hazardService,
        IShiftDefinitionService shiftDefinitionService,
        IScheduleConflictService scheduleConflictService,
        IEmailService emailService,
        IEmployeeService employeeService,
        ILogger<BookingService> logger)
    {
      _context = context;
      _craneService = craneService;
      _hazardService = hazardService;
      _shiftDefinitionService = shiftDefinitionService;
      _scheduleConflictService = scheduleConflictService;
      _emailService = emailService;
      _employeeService = employeeService;
      _logger = logger;
    }

    public async Task<IEnumerable<BookingViewModel>> GetAllBookingsAsync()
    {
      var bookings = await _context.Bookings
          .Include(r => r.Crane)
          .OrderByDescending(r => r.SubmitTime)
          .ToListAsync();

      return bookings.Select(r => new BookingViewModel
      {
        Id = r.Id,
        BookingNumber = r.BookingNumber,
        DocumentNumber = r.DocumentNumber,
        Name = r.Name,
        Department = r.Department,
        CraneId = r.CraneId,
        CraneCode = r.Crane?.Code,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        SubmitTime = r.SubmitTime,
        Location = r.Location,
        ProjectSupervisor = r.ProjectSupervisor,
        CostCode = r.CostCode,
        PhoneNumber = r.PhoneNumber,
        Description = r.Description
      }).ToList();
    }

    public async Task<BookingDetailViewModel> GetBookingByIdAsync(int id)
    {
      var booking = await _context.Bookings
          .Include(r => r.Crane)
          .Include(r => r.BookingShifts)
            .ThenInclude(bs => bs.ShiftDefinition)
          .Include(r => r.BookingItems)
          .Include(r => r.BookingHazards)
            .ThenInclude(bh => bh.Hazard)
          .FirstOrDefaultAsync(r => r.Id == id);

      if (booking == null)
      {
        throw new KeyNotFoundException($"Booking with ID {id} not found");
      }

      return MapToBookingDetailViewModel(booking);
    }

    public async Task<BookingDetailViewModel> GetBookingByDocumentNumberAsync(string documentNumber)
    {
      var booking = await _context.Bookings
          .Include(r => r.Crane)
          .Include(r => r.BookingShifts)
            .ThenInclude(bs => bs.ShiftDefinition)
          .Include(r => r.BookingItems)
          .Include(r => r.BookingHazards)
            .ThenInclude(bh => bh.Hazard)
          .FirstOrDefaultAsync(r => r.DocumentNumber == documentNumber);

      if (booking == null)
      {
        throw new KeyNotFoundException($"Booking with document number {documentNumber} not found");
      }

      return MapToBookingDetailViewModel(booking);
    }

    private BookingDetailViewModel MapToBookingDetailViewModel(Booking booking)
    {
      return new BookingDetailViewModel
      {
        Id = booking.Id,
        BookingNumber = booking.BookingNumber,
        DocumentNumber = booking.DocumentNumber,
        Name = booking.Name,
        Department = booking.Department,
        CraneId = booking.CraneId,
        CraneCode = booking.Crane?.Code,
        StartDate = booking.StartDate,
        EndDate = booking.EndDate,
        SubmitTime = booking.SubmitTime,
        Location = booking.Location,
        ProjectSupervisor = booking.ProjectSupervisor,
        CostCode = booking.CostCode,
        PhoneNumber = booking.PhoneNumber,
        Description = booking.Description,
        CustomHazard = booking.CustomHazard,

        // Status approval
        Status = booking.Status,

        // Manager approval info
        ManagerName = booking.ManagerName,
        ManagerApprovalTime = booking.ManagerApprovalTime,
        ManagerRejectReason = booking.ManagerRejectReason,

        // PIC approval info
        ApprovedByPIC = booking.ApprovedByPIC,
        ApprovedAtByPIC = booking.ApprovedAtByPIC,
        PICRejectReason = booking.PICRejectReason,

        // PIC completion info
        DoneByPIC = booking.DoneByPIC,
        DoneAt = booking.DoneAt,

        // Cancellation properties
        CancelledBy = booking.CancelledBy,
        CancelledByName = booking.CancelledByName,
        CancelledAt = booking.CancelledAt,
        CancelledReason = booking.CancelledReason,

        // Revision tracking
        RevisionCount = booking.RevisionCount,
        LastModifiedAt = booking.LastModifiedAt,
        LastModifiedBy = booking.LastModifiedBy,

        Shifts = booking.BookingShifts.Select(s => new BookingShiftViewModel
        {
          Id = s.Id,
          Date = s.Date,
          ShiftDefinitionId = s.ShiftDefinitionId,
          ShiftName = s.ShiftName ?? s.ShiftDefinition?.Name,
          StartTime = s.ShiftStartTime != default ? s.ShiftStartTime : s.ShiftDefinition?.StartTime,
          EndTime = s.ShiftEndTime != default ? s.ShiftEndTime : s.ShiftDefinition?.EndTime
        }).ToList(),
        Items = booking.BookingItems.Select(i => new BookingItemViewModel
        {
          Id = i.Id,
          ItemName = i.ItemName,
          Weight = i.Weight,
          Height = i.Height,
          Quantity = i.Quantity
        }).ToList(),
        SelectedHazards = booking.BookingHazards
          .Where(bh => bh.Hazard != null)
          .Select(bh => new HazardViewModel
          {
            Id = bh.Hazard!.Id,
            Name = bh.Hazard.Name
          }).ToList()
      };
    }

    public async Task<IEnumerable<BookingViewModel>> GetBookingsByCraneIdAsync(int craneId)
    {
      if (!await _craneService.CraneExistsAsync(craneId))
      {
        throw new KeyNotFoundException($"Crane with ID {craneId} not found");
      }

      var bookings = await _context.Bookings
          .Include(r => r.Crane)
          .Where(r => r.CraneId == craneId)
          .OrderByDescending(r => r.SubmitTime)
          .ToListAsync();

      return bookings.Select(r => new BookingViewModel
      {
        Id = r.Id,
        BookingNumber = r.BookingNumber,
        DocumentNumber = r.DocumentNumber,
        Name = r.Name,
        Department = r.Department,
        CraneId = r.CraneId,
        CraneCode = r.Crane?.Code,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        SubmitTime = r.SubmitTime,
        Location = r.Location,
        ProjectSupervisor = r.ProjectSupervisor,
        CostCode = r.CostCode,
        PhoneNumber = r.PhoneNumber,
        Description = r.Description
      }).ToList();
    }

    // Perubahan pada BookingService.cs
    public async Task<CalendarResponseViewModel> GetCalendarViewAsync(DateTime startDate, DateTime endDate)
    {
      // Gunakan langsung date tanpa konversi ke UTC
      var startDateLocal = startDate.Date;
      var endDateLocal = endDate.Date;

      // Ambil semua crane (urut berdasarkan Code)
      var cranes = await _context.Cranes
          .OrderBy(c => c.Code)
          .ToListAsync();

      // Siapkan response
      var response = new CalendarResponseViewModel
      {
        WeekRange = new WeekRangeViewModel
        {
          StartDate = startDateLocal.ToString("yyyy-MM-dd"),
          EndDate = endDateLocal.ToString("yyyy-MM-dd")
        },
        Cranes = new List<CraneBookingsViewModel>()
      };

      // Dapatkan semua booking dalam rentang tanggal dengan status PICApproved atau Done
      var bookingShifts = await _context.BookingShifts
          .Include(bs => bs.Booking)
          .ThenInclude(b => b!.Crane)
          .Include(bs => bs.ShiftDefinition)
          .Where(bs => bs.Date >= startDateLocal && bs.Date <= endDateLocal)
          // Filter hanya booking dengan status PICApproved atau Done
          .Where(bs => bs.Booking != null &&
                      (bs.Booking.Status == BookingStatus.PICApproved || bs.Booking.Status == BookingStatus.Done))
          .ToListAsync();

      // Kelompokkan berdasarkan crane
      foreach (var crane in cranes)
      {
        var craneDto = new CraneBookingsViewModel
        {
          CraneId = crane.Code,
          Capacity = crane.Capacity,
          Bookings = new List<BookingCalendarViewModel>(),
          MaintenanceSchedules = new List<MaintenanceCalendarViewModel>()
        };

        // Group shifts by date and booking
        var craneShifts = bookingShifts
            .Where(bs => bs.Booking!.CraneId == crane.Id)
            .GroupBy(bs => new { bs.Date, bs.BookingId })
            .ToList();

        foreach (var group in craneShifts)
        {
          // Get first shift to access booking info
          var firstShift = group.First();

          var calendarBooking = new BookingCalendarViewModel
          {
            Id = firstShift.BookingId,
            BookingNumber = firstShift.Booking!.BookingNumber,
            DocumentNumber = firstShift.Booking!.DocumentNumber,
            Department = firstShift.Booking.Department,
            Date = group.Key.Date,
            // Tambahkan status booking untuk styling yang berbeda
            Status = firstShift.Booking.Status,
            Shifts = group.Select(s => new ShiftBookingViewModel
            {
              ShiftDefinitionId = s.ShiftDefinitionId,
              ShiftName = s.ShiftName ?? s.ShiftDefinition?.Name,
              StartTime = s.ShiftStartTime != default ? s.ShiftStartTime : s.ShiftDefinition?.StartTime ?? TimeSpan.Zero,
              EndTime = s.ShiftEndTime != default ? s.ShiftEndTime : s.ShiftDefinition?.EndTime ?? TimeSpan.Zero
            }).ToList()
          };

          craneDto.Bookings.Add(calendarBooking);
        }

        response.Cranes.Add(craneDto);
      }

      // Dapatkan maintenance shifts dalam rentang tanggal
      var maintenanceShifts = await _context.MaintenanceScheduleShifts
          .Include(ms => ms.MaintenanceSchedule)
          .ThenInclude(m => m!.Crane)
          .Include(ms => ms.ShiftDefinition)
          .Where(ms => ms.Date >= startDateLocal && ms.Date <= endDateLocal)
          .ToListAsync();

      // Tambahkan maintenance shifts ke response
      foreach (var crane in cranes)
      {
        var craneDto = response.Cranes.FirstOrDefault(c => c.CraneId == crane.Code);
        if (craneDto == null) continue;

        // Group maintenance shifts by date and schedule
        var craneMaintenance = maintenanceShifts
            .Where(ms => ms.MaintenanceSchedule!.CraneId == crane.Id)
            .GroupBy(ms => new { ms.Date, ms.MaintenanceScheduleId })
            .ToList();

        foreach (var group in craneMaintenance)
        {
          // Get first shift to access maintenance info
          var firstShift = group.First();

          var calendarMaintenance = new MaintenanceCalendarViewModel
          {
            Id = firstShift.MaintenanceScheduleId,
            Title = firstShift.MaintenanceSchedule!.Title,
            Date = group.Key.Date,
            Shifts = group.Select(s => new ShiftBookingViewModel
            {
              ShiftDefinitionId = s.ShiftDefinitionId,
              ShiftName = s.ShiftName ?? s.ShiftDefinition?.Name,
              StartTime = s.ShiftStartTime != default ? s.ShiftStartTime : s.ShiftDefinition?.StartTime ?? TimeSpan.Zero,
              EndTime = s.ShiftEndTime != default ? s.ShiftEndTime : s.ShiftDefinition?.EndTime ?? TimeSpan.Zero
            }).ToList()
          };

          craneDto.MaintenanceSchedules.Add(calendarMaintenance);
        }
      }

      return response;
    }

    public async Task<BookingDetailViewModel> CreateBookingAsync(BookingCreateViewModel bookingViewModel)
    {
      try
      {
        _logger.LogInformation("Creating booking for crane {CraneId}", bookingViewModel.CraneId);

        // Validate crane exists
        if (!await _craneService.CraneExistsAsync(bookingViewModel.CraneId))
        {
          throw new KeyNotFoundException($"Crane with ID {bookingViewModel.CraneId} not found");
        }

        // Validate crane is available (not in maintenance)
        var crane = await _context.Cranes.FindAsync(bookingViewModel.CraneId);
        if (crane?.Status == CraneStatus.Maintenance)
        {
          throw new InvalidOperationException($"Cannot reserve crane with ID {bookingViewModel.CraneId} because it is currently under maintenance");
        }

        // Gunakan tanggal lokal tanpa konversi UTC
        var startDate = bookingViewModel.StartDate.Date;
        var endDate = bookingViewModel.EndDate.Date;

        // Validate date range
        if (startDate > endDate)
        {
          throw new ArgumentException("Start date must be before or equal to end date");
        }

        // Validate shift selections
        if (bookingViewModel.ShiftSelections == null || !bookingViewModel.ShiftSelections.Any())
        {
          throw new ArgumentException("At least one shift selection is required");
        }

        // Check if all dates in the range have shift selections
        var dateRange = Enumerable.Range(0, (endDate - startDate).Days + 1)
            .Select(d => startDate.AddDays(d))
            .ToList();

        var selectedDates = bookingViewModel.ShiftSelections
            .Select(s => s.Date.Date)
            .ToList();

        if (!dateRange.All(d => selectedDates.Contains(d)))
        {
          throw new ArgumentException("All dates in the range must have shift selections");
        }

        // Validate each shift selection has at least one shift selected
        foreach (var selection in bookingViewModel.ShiftSelections)
        {
          if (selection.SelectedShiftIds == null || !selection.SelectedShiftIds.Any())
          {
            throw new ArgumentException($"At least one shift must be selected for date {selection.Date.ToShortDateString()}");
          }

          // Gunakan tanggal lokal untuk pengecekan konflik
          var dateLocal = selection.Date.Date;

          // Check for scheduling conflicts for each selected shift
          foreach (var shiftId in selection.SelectedShiftIds)
          {
            // Verify the shift definition exists
            if (!await _shiftDefinitionService.ShiftDefinitionExistsAsync(shiftId))
            {
              throw new KeyNotFoundException($"Shift definition with ID {shiftId} not found");
            }

            bool hasConflict = await _scheduleConflictService.IsBookingConflictAsync(
                bookingViewModel.CraneId,
                dateLocal,
                shiftId);

            if (hasConflict)
            {
              // Get shift name for better error message
              var shift = await _context.ShiftDefinitions.FindAsync(shiftId);
              throw new InvalidOperationException($"Scheduling conflict detected for date {dateLocal.ToShortDateString()} and shift {shift?.Name ?? shiftId.ToString()}");
            }

            // Periksa konflik dengan jadwal maintenance
            bool hasMaintenanceConflict = await _scheduleConflictService.IsMaintenanceConflictAsync(
                bookingViewModel.CraneId,
                dateLocal,
                shiftId);

            if (hasMaintenanceConflict)
            {
              // Get shift name for better error message
              var shift = await _context.ShiftDefinitions.FindAsync(shiftId);
              throw new InvalidOperationException($"Scheduling conflict with maintenance schedule detected for date {dateLocal.ToShortDateString()} and shift {shift?.Name ?? shiftId.ToString()}");
            }
          }
        }

        // Create booking dengan status Pending
        var booking = new Booking
        {
          BookingNumber = "TEMP", // Temporary value
          DocumentNumber = Guid.NewGuid().ToString(),
          Name = bookingViewModel.Name,
          Department = bookingViewModel.Department,
          CraneId = bookingViewModel.CraneId,
          StartDate = startDate,
          EndDate = endDate,
          SubmitTime = DateTime.Now,
          Location = bookingViewModel.Location,
          ProjectSupervisor = bookingViewModel.ProjectSupervisor,
          CostCode = bookingViewModel.CostCode,
          PhoneNumber = bookingViewModel.PhoneNumber,
          Description = bookingViewModel.Description,
          CustomHazard = bookingViewModel.CustomHazard,
          Status = BookingStatus.PendingApproval
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Update booking number based on ID
        booking.BookingNumber = $"C{booking.Id:D4}";
        await _context.SaveChangesAsync();

        // Create shift selections with historical data
        foreach (var selection in bookingViewModel.ShiftSelections)
        {
          var dateLocal = selection.Date.Date;

          foreach (var shiftId in selection.SelectedShiftIds)
          {
            // Dapatkan informasi shift saat ini
            var shiftDefinition = await _context.ShiftDefinitions.FindAsync(shiftId);
            if (shiftDefinition == null)
            {
              throw new KeyNotFoundException($"Shift definition with ID {shiftId} not found");
            }

            var bookingShift = new BookingShift
            {
              BookingId = booking.Id,
              Date = dateLocal,
              ShiftDefinitionId = shiftId,
              // Simpan juga data historis shift
              ShiftName = shiftDefinition.Name,
              ShiftStartTime = shiftDefinition.StartTime,
              ShiftEndTime = shiftDefinition.EndTime
            };

            _context.BookingShifts.Add(bookingShift);
          }
        }

        // Add booking items
        if (bookingViewModel.Items != null && bookingViewModel.Items.Any())
        {
          foreach (var itemViewModel in bookingViewModel.Items)
          {
            var item = new BookingItem
            {
              BookingId = booking.Id,
              ItemName = itemViewModel.ItemName,
              Weight = itemViewModel.Weight,
              Height = itemViewModel.Height,
              Quantity = itemViewModel.Quantity
            };

            _context.BookingItems.Add(item);
          }
        }

        // Handle predefined hazards
        if (bookingViewModel.HazardIds != null && bookingViewModel.HazardIds.Any())
        {
          foreach (var hazardId in bookingViewModel.HazardIds)
          {
            // Validasi hazard exists
            if (await _hazardService.HazardExistsAsync(hazardId))
            {
              var bookingHazard = new BookingHazard
              {
                BookingId = booking.Id,
                HazardId = hazardId
              };
              _context.BookingHazards.Add(bookingHazard);
            }
          }
        }

        await _context.SaveChangesAsync();

        // Cari manager departemen user
        var manager = await _employeeService.GetManagerByDepartmentAsync(booking.Department);

        // Dapatkan data user yang melakukan booking
        var user = await _employeeService.GetEmployeeByLdapUserAsync(booking.Name);

        // Kirim email notifikasi ke user
        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
          await _emailService.SendBookingSubmittedEmailAsync(booking, user.Email);
        }

        // Kirim email permintaan approval ke manager
        if (manager != null && !string.IsNullOrEmpty(manager.Email) && !string.IsNullOrEmpty(manager.LdapUser))
        {
          await _emailService.SendManagerApprovalRequestEmailAsync(
              booking,
              manager.Email,
              manager.Name,
              manager.LdapUser);
        }
        else
        {
          _logger.LogWarning("Manager tidak ditemukan untuk departemen {Department}", booking.Department);
        }

        // Return the created booking with details
        return await GetBookingByIdAsync(booking.Id);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error creating booking: {Message}", ex.Message);
        throw;
      }
    }

    public async Task<BookingDetailViewModel> UpdateBookingAsync(int id, BookingUpdateViewModel bookingViewModel)
    {
      try
      {
        _logger.LogInformation("Updating booking ID: {Id}", id);

        var booking = await _context.Bookings
            .Include(r => r.BookingShifts)
            .Include(r => r.BookingItems)
            .Include(r => r.BookingHazards)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (booking == null)
        {
          throw new KeyNotFoundException($"Booking with ID {id} not found");
        }

        // Validate crane exists if changing crane
        if (booking.CraneId != bookingViewModel.CraneId &&
            !await _craneService.CraneExistsAsync(bookingViewModel.CraneId))
        {
          throw new KeyNotFoundException($"Crane with ID {bookingViewModel.CraneId} not found");
        }

        // Validate crane is available if changing crane
        if (booking.CraneId != bookingViewModel.CraneId)
        {
          var crane = await _context.Cranes.FindAsync(bookingViewModel.CraneId);
          if (crane?.Status == CraneStatus.Maintenance)
          {
            throw new InvalidOperationException($"Cannot reserve crane with ID {bookingViewModel.CraneId} because it is currently under maintenance");
          }
        }

        // Gunakan tanggal lokal tanpa konversi UTC
        var startDate = bookingViewModel.StartDate.Date;
        var endDate = bookingViewModel.EndDate.Date;

        // Validate date range
        if (startDate > endDate)
        {
          throw new ArgumentException("Start date must be before or equal to end date");
        }

        // Validate shift selections
        if (bookingViewModel.ShiftSelections == null || !bookingViewModel.ShiftSelections.Any())
        {
          throw new ArgumentException("At least one shift selection is required");
        }

        // Check if all dates in the range have shift selections
        var dateRange = Enumerable.Range(0, (endDate - startDate).Days + 1)
            .Select(d => startDate.AddDays(d))
            .ToList();

        var selectedDates = bookingViewModel.ShiftSelections
            .Select(s => s.Date.Date)
            .ToList();

        if (!dateRange.All(d => selectedDates.Contains(d)))
        {
          throw new ArgumentException("All dates in the range must have shift selections");
        }

        // Validate each shift selection has at least one shift selected
        foreach (var selection in bookingViewModel.ShiftSelections)
        {
          if (selection.SelectedShiftIds == null || !selection.SelectedShiftIds.Any())
          {
            throw new ArgumentException($"At least one shift must be selected for date {selection.Date.ToShortDateString()}");
          }

          // Gunakan tanggal lokal untuk pengecekan konflik
          var dateLocal = selection.Date.Date;

          // Check for scheduling conflicts for each selected shift
          foreach (var shiftId in selection.SelectedShiftIds)
          {
            // Verify the shift definition exists
            if (!await _shiftDefinitionService.ShiftDefinitionExistsAsync(shiftId))
            {
              throw new KeyNotFoundException($"Shift definition with ID {shiftId} not found");
            }

            bool hasConflict = await _scheduleConflictService.IsBookingConflictAsync(
                bookingViewModel.CraneId,
                dateLocal,
                shiftId,
                id);

            if (hasConflict)
            {
              // Get shift name for better error message
              var shift = await _context.ShiftDefinitions.FindAsync(shiftId);
              throw new InvalidOperationException($"Scheduling conflict detected for date {dateLocal.ToShortDateString()} and shift {shift?.Name ?? shiftId.ToString()}");
            }

            // Periksa konflik dengan jadwal maintenance
            bool hasMaintenanceConflict = await _scheduleConflictService.IsMaintenanceConflictAsync(
                bookingViewModel.CraneId,
                dateLocal,
                shiftId);

            if (hasMaintenanceConflict)
            {
              // Get shift name for better error message
              var shift = await _context.ShiftDefinitions.FindAsync(shiftId);
              throw new InvalidOperationException($"Scheduling conflict with maintenance schedule detected for date {dateLocal.ToShortDateString()} and shift {shift?.Name ?? shiftId.ToString()}");
            }
          }
        }

        // Update booking
        booking.Name = bookingViewModel.Name;
        booking.Department = bookingViewModel.Department;
        booking.CraneId = bookingViewModel.CraneId;
        booking.StartDate = startDate;
        booking.EndDate = endDate;
        booking.CustomHazard = bookingViewModel.CustomHazard;
        booking.Location = bookingViewModel.Location;
        booking.ProjectSupervisor = bookingViewModel.ProjectSupervisor;
        booking.CostCode = bookingViewModel.CostCode;
        booking.PhoneNumber = bookingViewModel.PhoneNumber;
        booking.Description = bookingViewModel.Description;
        // SubmitTime is not updated

        // Remove existing shift selections
        _context.BookingShifts.RemoveRange(booking.BookingShifts);

        // Remove existing hazards
        _context.BookingHazards.RemoveRange(booking.BookingHazards);

        // Create new shift selections with historical data
        foreach (var selection in bookingViewModel.ShiftSelections)
        {
          var dateLocal = selection.Date.Date;

          foreach (var shiftId in selection.SelectedShiftIds)
          {
            // Dapatkan informasi shift saat ini
            var shiftDefinition = await _context.ShiftDefinitions.FindAsync(shiftId);
            if (shiftDefinition == null)
            {
              throw new KeyNotFoundException($"Shift definition with ID {shiftId} not found");
            }

            var bookingShift = new BookingShift
            {
              BookingId = booking.Id,
              Date = dateLocal,
              ShiftDefinitionId = shiftId,
              // Simpan juga data historis shift
              ShiftName = shiftDefinition.Name,
              ShiftStartTime = shiftDefinition.StartTime,
              ShiftEndTime = shiftDefinition.EndTime
            };

            _context.BookingShifts.Add(bookingShift);
          }
        }

        // Remove existing items
        _context.BookingItems.RemoveRange(booking.BookingItems);

        // Add new items
        if (bookingViewModel.Items != null && bookingViewModel.Items.Any())
        {
          foreach (var itemViewModel in bookingViewModel.Items)
          {
            var item = new BookingItem
            {
              BookingId = booking.Id,
              ItemName = itemViewModel.ItemName,
              Weight = itemViewModel.Weight,
              Height = itemViewModel.Height,
              Quantity = itemViewModel.Quantity
            };

            _context.BookingItems.Add(item);
          }
        }

        // Handle predefined hazards
        if (bookingViewModel.HazardIds != null && bookingViewModel.HazardIds.Any())
        {
          foreach (var hazardId in bookingViewModel.HazardIds)
          {
            // Validasi hazard exists
            if (await _hazardService.HazardExistsAsync(hazardId))
            {
              var bookingHazard = new BookingHazard
              {
                BookingId = booking.Id,
                HazardId = hazardId
              };
              _context.BookingHazards.Add(bookingHazard);
            }
          }
        }

        await _context.SaveChangesAsync();

        // Return the updated booking with details
        return await GetBookingByIdAsync(booking.Id);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error updating booking: {Message}", ex.Message);
        throw;
      }
    }

    public async Task DeleteBookingAsync(int id)
    {
      try
      {
        _logger.LogInformation("Deleting booking ID: {Id}", id);

        var booking = await _context.Bookings
            .Include(r => r.BookingShifts)
            .Include(r => r.BookingItems)
            .Include(r => r.BookingHazards)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (booking == null)
        {
          throw new KeyNotFoundException($"Booking with ID {id} not found");
        }

        // Remove all associated shifts
        _context.BookingShifts.RemoveRange(booking.BookingShifts);

        // Remove all associated items
        _context.BookingItems.RemoveRange(booking.BookingItems);

        // Remove all associated hazards
        _context.BookingHazards.RemoveRange(booking.BookingHazards);

        // Remove the booking
        _context.Bookings.Remove(booking);

        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error deleting booking: {Message}", ex.Message);
        throw;
      }
    }

    public async Task<IEnumerable<BookingDetailViewModel>> GetBookingsByStatusAsync(BookingStatus status)
    {
      try
      {
        var bookings = await _context.Bookings
            .Include(b => b.Crane)
            .Where(b => b.Status == status)
            .OrderByDescending(b => b.Status == BookingStatus.PICApproved ? b.ApprovedAtByPIC : b.SubmitTime)
            .ToListAsync();

        var result = new List<BookingDetailViewModel>();

        foreach (var booking in bookings)
        {
          result.Add(new BookingDetailViewModel
          {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            DocumentNumber = booking.DocumentNumber,
            Name = booking.Name,
            Department = booking.Department,
            CraneId = booking.CraneId,
            CraneCode = booking.Crane?.Code,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            SubmitTime = booking.SubmitTime,
            Status = booking.Status,
            ManagerName = booking.ManagerName,
            ManagerApprovalTime = booking.ManagerApprovalTime,
            ApprovedByPIC = booking.ApprovedByPIC,
            ApprovedAtByPIC = booking.ApprovedAtByPIC
            // Other properties as needed
          });
        }

        return result;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error getting bookings by status: {Status}", status);
        throw;
      }
    }

    public async Task<int> RelocateAffectedBookingsAsync(int craneId, DateTime maintenanceStartTime, DateTime maintenanceEndTime)
    {
      _logger.LogInformation("Mulai merelokasi booking yang terdampak oleh maintenance crane {CraneId} dari {StartTime} hingga {EndTime}",
          craneId, maintenanceStartTime, maintenanceEndTime);

      // Temukan semua booking yang terdampak (yang berada dalam rentang waktu maintenance)
      var affectedBookings = await _context.Bookings
          .Include(b => b.BookingShifts)
              .ThenInclude(bs => bs.ShiftDefinition)
          .Include(b => b.BookingItems)
          .Include(b => b.BookingHazards)
          .Where(b => b.CraneId == craneId &&
                 ((b.StartDate.Date <= maintenanceEndTime.Date && b.EndDate.Date >= maintenanceStartTime.Date)))
          .ToListAsync();

      // Sort affected bookings by submission time to maintain original order
      affectedBookings = affectedBookings.OrderBy(b => b.SubmitTime).ToList();
      _logger.LogInformation("Found {Count} affected bookings, sorted by submission time", affectedBookings.Count);

      int relocatedCount = 0;

      // Tanggal pertama yang tersedia setelah maintenance
      DateTime firstAvailableDate = maintenanceEndTime.Date.AddDays(1);

      // Track tanggal terakhir yang sudah dialokasikan untuk menghindari overlap
      DateTime lastAllocatedDate = firstAvailableDate.AddDays(-1);

      foreach (var booking in affectedBookings)
      {
        // Cek apakah ada shift dalam booking yang terdampak maintenance
        var affectedShifts = booking.BookingShifts
            .Where(bs => (bs.Date.Date >= maintenanceStartTime.Date && bs.Date.Date <= maintenanceEndTime.Date))
            .ToList();

        if (!affectedShifts.Any())
        {
          // Tidak ada shift yang terdampak, lanjut ke booking berikutnya
          continue;
        }

        _logger.LogInformation("Merelokasi booking {BookingId} ({BookingNumber}) dengan {ShiftCount} shift yang terdampak. Submit time: {SubmitTime}",
            booking.Id, booking.BookingNumber, affectedShifts.Count, booking.SubmitTime);

        // Kelompokkan shift berdasarkan tanggal dan urutkan berdasarkan tanggal
        var shiftsByDate = affectedShifts
            .GroupBy(s => s.Date.Date)
            .OrderBy(g => g.Key)
            .ToList();

        // Simpan mapping dari tanggal lama ke tanggal baru untuk menjaga urutan relatif hari
        Dictionary<DateTime, DateTime> dateMapping = new Dictionary<DateTime, DateTime>();

        // Tentukan tanggal awal pencarian berdasarkan tanggal terakhir yang dialokasikan
        // Gunakan tanggal terakhir yang dialokasikan + 1 hari untuk memastikan tidak ada overlap
        DateTime searchStartDate = lastAllocatedDate.AddDays(1);
        DateTime lastDateForCurrentBooking = searchStartDate;

        _logger.LogInformation("Starting search for available dates from {StartDate} for booking {BookingId}",
            searchStartDate, booking.Id);

        // Mencari slot untuk setiap tanggal yang terdampak
        foreach (var dateGroup in shiftsByDate)
        {
          DateTime affectedDate = dateGroup.Key;
          var shiftsForDate = dateGroup.ToList();

          // Dapatkan shift IDs untuk tanggal ini
          var shiftIds = shiftsForDate.Select(s => s.ShiftDefinitionId).ToList();

          // Mulai pencarian dari tanggal pencarian awal
          DateTime candidateDate = searchStartDate;
          bool foundAvailableDate = false;

          // Coba hingga 30 hari ke depan untuk mencari slot yang tersedia
          for (int i = 0; i < 30; i++)
          {
            bool dateHasConflict = false;

            // Periksa setiap shift ID
            foreach (var shiftId in shiftIds)
            {
              // Cek apakah ada konflik pada tanggal dan shift tersebut
              bool hasBookingConflict = await _scheduleConflictService.IsBookingConflictAsync(craneId, candidateDate, shiftId);
              bool hasMaintenanceConflict = await _scheduleConflictService.IsMaintenanceConflictAsync(craneId, candidateDate, shiftId);

              if (hasBookingConflict || hasMaintenanceConflict)
              {
                dateHasConflict = true;
                break;
              }
            }

            if (!dateHasConflict)
            {
              // Tanggal ini tersedia untuk semua shift
              foundAvailableDate = true;
              break;
            }

            // Coba tanggal berikutnya
            candidateDate = candidateDate.AddDays(1);
          }

          if (!foundAvailableDate)
          {
            _logger.LogWarning("Tidak dapat menemukan tanggal pengganti untuk booking {BookingId} tanggal {AffectedDate}",
                booking.Id, affectedDate);
            continue;
          }

          // Simpan mapping dari tanggal lama ke tanggal baru
          dateMapping[affectedDate] = candidateDate;

          _logger.LogInformation("Found available date {NewDate} for booking {BookingId} to replace date {OldDate}",
              candidateDate, booking.Id, affectedDate);

          // Update tanggal terakhir yang digunakan untuk mencari slot berikutnya
          lastDateForCurrentBooking = candidateDate;

          // Update tanggal awal pencarian untuk shift berikutnya dalam booking yang sama
          searchStartDate = candidateDate.AddDays(1);
        }

        // Update lastAllocatedDate dengan tanggal terakhir yang dialokasikan untuk booking ini
        // untuk memastikan booking berikutnya tidak overlap dengan booking ini
        lastAllocatedDate = lastDateForCurrentBooking;

        _logger.LogInformation("Last allocated date updated to {LastDate} after processing booking {BookingId}",
            lastAllocatedDate, booking.Id);

        // Pindahkan shift ke tanggal baru berdasarkan mapping
        foreach (var dateGroup in shiftsByDate)
        {
          DateTime oldDate = dateGroup.Key;
          if (!dateMapping.TryGetValue(oldDate, out DateTime newDate))
          {
            // Skip jika tidak ada mapping untuk tanggal ini
            continue;
          }

          var shiftsToRelocate = dateGroup.ToList();

          _logger.LogInformation("Memindahkan booking {BookingId} dari tanggal {OldDate} ke tanggal {NewDate}",
              booking.Id, oldDate, newDate);

          // Update tanggal booking shifts
          foreach (var shift in shiftsToRelocate)
          {
            // Lakukan deep clone dengan membuat shift baru
            _context.BookingShifts.Remove(shift);

            var newShift = new BookingShift
            {
              BookingId = shift.BookingId,
              Date = newDate, // Gunakan tanggal baru
              ShiftDefinitionId = shift.ShiftDefinitionId,
              ShiftName = shift.ShiftName,
              ShiftStartTime = shift.ShiftStartTime,
              ShiftEndTime = shift.ShiftEndTime
            };

            _context.BookingShifts.Add(newShift);
          }
        }

        // Update tanggal start/end booking
        DateTime? earliestNewDate = null;
        DateTime? latestNewDate = null;

        foreach (var pair in dateMapping)
        {
          if (earliestNewDate == null || pair.Value < earliestNewDate)
            earliestNewDate = pair.Value;

          if (latestNewDate == null || pair.Value > latestNewDate)
            latestNewDate = pair.Value;
        }

        if (earliestNewDate.HasValue)
          booking.StartDate = earliestNewDate.Value;

        if (latestNewDate.HasValue)
          booking.EndDate = latestNewDate.Value;

        relocatedCount++;
      }

      // Simpan perubahan ke database
      await _context.SaveChangesAsync();

      _logger.LogInformation("Berhasil merelokasi {Count} booking yang terdampak oleh maintenance crane {CraneId}", relocatedCount, craneId);
      return relocatedCount;
    }

    public async Task<IEnumerable<BookedShiftViewModel>> GetBookedShiftsByCraneAndDateRangeAsync(
    int craneId, DateTime startDate, DateTime endDate)
    {
      var bookingShifts = await _context.BookingShifts
          .Include(bs => bs.Booking)
          .Where(bs =>
              bs.Booking != null &&
              bs.Booking.Status != BookingStatus.Cancelled &&
              bs.Booking.CraneId == craneId &&
              bs.Date.Date >= startDate.Date &&
              bs.Date.Date <= endDate.Date)
          .Select(bs => new BookedShiftViewModel
          {
            CraneId = bs.Booking!.CraneId,
            Date = bs.Date,
            ShiftDefinitionId = bs.ShiftDefinitionId
          })
          .ToListAsync();

      return bookingShifts;
    }

    // Add this method to BookingService class (Services/Booking/BookingService.cs)
    public async Task<IEnumerable<BookingViewModel>> SearchBookingsAsync(string searchTerm, string currentUser, bool isPic, bool isAdmin)
    {
      try
      {
        _logger.LogInformation("Searching bookings with term: {SearchTerm}, user: {CurrentUser}", searchTerm, currentUser);

        // Normalize search term (lowercase and trim)
        searchTerm = searchTerm.ToLower().Trim();

        // Query bookings based on search term (BookingNumber or Name)
        var query = _context.Bookings
            .Include(b => b.Crane)
            .Where(b => b.BookingNumber.ToLower().Contains(searchTerm) ||
                        b.Name.ToLower().Contains(searchTerm));

        // Apply access control - if not PIC or admin, only show user's own bookings
        if (!isPic && !isAdmin)
        {
          query = query.Where(b => b.Name == currentUser);
        }

        // Execute query and order by submission time (newest first)
        var bookings = await query
            .OrderByDescending(b => b.SubmitTime)
            .ToListAsync();

        // Map to view models
        return bookings.Select(b => new BookingViewModel
        {
          Id = b.Id,
          BookingNumber = b.BookingNumber,
          DocumentNumber = b.DocumentNumber,
          Name = b.Name,
          Department = b.Department,
          CraneId = b.CraneId,
          CraneCode = b.Crane?.Code,
          StartDate = b.StartDate,
          EndDate = b.EndDate,
          SubmitTime = b.SubmitTime,
          Location = b.Location,
          Status = b.Status,
          ProjectSupervisor = b.ProjectSupervisor,
          CostCode = b.CostCode,
          PhoneNumber = b.PhoneNumber,
          Description = b.Description
        }).ToList();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error searching bookings with term: {SearchTerm}", searchTerm);
        throw;
      }
    }

    public async Task<bool> IsShiftBookingConflictAsync(int craneId, DateTime date, int shiftDefinitionId, int? excludeBookingId = null)
    {
      return await _scheduleConflictService.IsBookingConflictAsync(craneId, date, shiftDefinitionId, excludeBookingId);
    }

    public async Task<bool> BookingExistsAsync(int id)
    {
      return await _context.Bookings.AnyAsync(r => r.Id == id);
    }

    public async Task<bool> BookingExistsByDocumentNumberAsync(string documentNumber)
    {
      return await _context.Bookings.AnyAsync(r => r.DocumentNumber == documentNumber);
    }
  }
}
