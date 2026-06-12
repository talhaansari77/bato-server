using System.Security.Claims;
using BatoClinic.Api.Data;
using BatoClinic.Api.DTOs.Appointments;
using BatoClinic.Api.Entities;
using BatoClinic.Api.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BatoClinic.Api.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AppointmentsController(AppDbContext context)
    {
        _context = context;
    }

    // POST /api/appointments
    // Patient creates a new appointment.
    [HttpPost]
    public async Task<ActionResult<AppointmentResponseDto>> CreateAppointment(CreateAppointmentDto dto)
    {
        var patientUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(patientUserId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var patient = await _context.Users.FirstOrDefaultAsync(user =>
            user.Id == patientUserId &&
            user.IsActive &&
            user.RoleType == "Patient");

        if (patient is null)
        {
            return BadRequest(new { message = "Only active patients can book appointments" });
        }

        var service = await _context.ClinicServices
            .FirstOrDefaultAsync(service => service.Id == dto.ClinicServiceId && service.IsActive);

        if (service is null)
        {
            return BadRequest(new { message = "Valid service is required" });
        }

        var branch = await _context.Branches
            .FirstOrDefaultAsync(branch => branch.Id == dto.BranchId && branch.IsActive);

        if (branch is null)
        {
            return BadRequest(new { message = "Valid branch is required" });
        }

        var doctor = await _context.DoctorProfiles
            .Include(profile => profile.User)
            .FirstOrDefaultAsync(profile =>
                profile.Id == dto.DoctorProfileId &&
                profile.IsAvailable &&
                profile.User != null &&
                profile.User.IsActive);

        if (doctor is null)
        {
            return BadRequest(new { message = "Valid doctor is required" });
        }

        var doctorCanDoService = await _context.DoctorServices.AnyAsync(item =>
            item.DoctorProfileId == dto.DoctorProfileId &&
            item.ClinicServiceId == dto.ClinicServiceId);

        if (!doctorCanDoService)
        {
            return BadRequest(new { message = "Selected doctor does not perform this service" });
        }

        var doctorWorksAtBranch = await _context.DoctorBranches.AnyAsync(item =>
            item.DoctorProfileId == dto.DoctorProfileId &&
            item.BranchId == dto.BranchId);

        if (!doctorWorksAtBranch)
        {
            return BadRequest(new { message = "Selected doctor does not work at this branch" });
        }

        var startTime = dto.StartTime;
        var endTime = startTime.AddMinutes(service.DurationMinutes);

        var hasConflict = await _context.Appointments.AnyAsync(appointment =>
            appointment.DoctorProfileId == dto.DoctorProfileId &&
            appointment.Status != AppointmentStatus.Cancelled &&
            appointment.Status != AppointmentStatus.Rejected &&
            appointment.Status != AppointmentStatus.Refunded &&
            startTime < appointment.EndTime &&
            endTime > appointment.StartTime);

        if (hasConflict)
        {
            return BadRequest(new { message = "Selected time slot is not available" });
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientUserId = patientUserId,
            DoctorProfileId = dto.DoctorProfileId,
            ClinicServiceId = dto.ClinicServiceId,
            BranchId = dto.BranchId,
            AppointmentDate = startTime.Date,
            StartTime = startTime,
            EndTime = endTime,
            PaymentMethod = dto.PaymentMethod,
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        if (dto.PaymentMethod == PaymentMethod.PayAtClinic)
        {
            appointment.Status = AppointmentStatus.PendingAdminApproval;
            appointment.PaymentStatus = PaymentStatus.PayAtClinic;
        }
        else
        {
            appointment.Status = AppointmentStatus.PendingPayment;
            appointment.PaymentStatus = PaymentStatus.Unpaid;
        }

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        var response = new AppointmentResponseDto
        {
            Id = appointment.Id,
            PatientUserId = patient.Id,
            PatientName = patient.FullName,
            DoctorProfileId = doctor.Id,
            DoctorName = doctor.User?.FullName ?? string.Empty,
            ClinicServiceId = service.Id,
            ServiceName = service.Name,
            BranchId = branch.Id,
            BranchName = branch.Name,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status.ToString(),
            PaymentStatus = appointment.PaymentStatus.ToString(),
            PaymentMethod = appointment.PaymentMethod.ToString(),
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt
        };

        return CreatedAtAction(nameof(GetAppointmentById), new { id = appointment.Id }, response);
    }


    // GET /api/appointments/available-slots?doctorProfileId={id}&clinicServiceId={id}&branchId={id}&date=2026-06-20
    // Returns available appointment slots for selected doctor, service, branch, and date.
    // For now, working hours are fixed: 9 AM to 6 PM.
    // Later, we will create doctor availability tables.
    [AllowAnonymous]
    [HttpGet("available-slots")]
    public async Task<ActionResult<IEnumerable<AvailableSlotDto>>> GetAvailableSlots(
        [FromQuery] Guid doctorProfileId,
        [FromQuery] Guid clinicServiceId,
        [FromQuery] Guid branchId,
        [FromQuery] DateTime date)
    {
        if (doctorProfileId == Guid.Empty || clinicServiceId == Guid.Empty || branchId == Guid.Empty)
        {
            return BadRequest(new
            {
                message = "doctorProfileId, clinicServiceId, and branchId are required"
            });
        }

        var service = await _context.ClinicServices
            .FirstOrDefaultAsync(service => service.Id == clinicServiceId && service.IsActive);

        if (service is null)
        {
            return BadRequest(new { message = "Valid service is required" });
        }

        var branch = await _context.Branches
            .FirstOrDefaultAsync(branch => branch.Id == branchId && branch.IsActive);

        if (branch is null)
        {
            return BadRequest(new { message = "Valid branch is required" });
        }

        var doctor = await _context.DoctorProfiles
            .Include(profile => profile.User)
            .FirstOrDefaultAsync(profile =>
                profile.Id == doctorProfileId &&
                profile.IsAvailable &&
                profile.User != null &&
                profile.User.IsActive);

        if (doctor is null)
        {
            return BadRequest(new { message = "Valid doctor is required" });
        }

        var doctorCanDoService = await _context.DoctorServices.AnyAsync(item =>
            item.DoctorProfileId == doctorProfileId &&
            item.ClinicServiceId == clinicServiceId);

        if (!doctorCanDoService)
        {
            return BadRequest(new { message = "Selected doctor does not perform this service" });
        }

        var doctorWorksAtBranch = await _context.DoctorBranches.AnyAsync(item =>
            item.DoctorProfileId == doctorProfileId &&
            item.BranchId == branchId);

        if (!doctorWorksAtBranch)
        {
            return BadRequest(new { message = "Selected doctor does not work at this branch" });
        }

        var selectedDate = date.Date;

        var dayStart = selectedDate.AddHours(9);
        var dayEnd = selectedDate.AddHours(18);

        var existingAppointments = await _context.Appointments
            .Where(appointment =>
                appointment.DoctorProfileId == doctorProfileId &&
                appointment.AppointmentDate == selectedDate &&
                appointment.Status != AppointmentStatus.Cancelled &&
                appointment.Status != AppointmentStatus.Rejected &&
                appointment.Status != AppointmentStatus.Refunded)
            .ToListAsync();

        var slots = new List<AvailableSlotDto>();

        var currentStart = dayStart;

        while (currentStart.AddMinutes(service.DurationMinutes) <= dayEnd)
        {
            var currentEnd = currentStart.AddMinutes(service.DurationMinutes);

            var hasConflict = existingAppointments.Any(appointment =>
                currentStart < appointment.EndTime &&
                currentEnd > appointment.StartTime);

            slots.Add(new AvailableSlotDto
            {
                StartTime = currentStart,
                EndTime = currentEnd,
                IsAvailable = !hasConflict
            });

            // Move to next slot.
            // For now, we move by service duration.
            // Later we can support custom slot intervals like every 15 or 30 minutes.
            currentStart = currentStart.AddMinutes(service.DurationMinutes);
        }

        return Ok(slots);
    }

    // GET /api/appointments/my
    // Returns logged-in patient's own appointments.
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetMyAppointments()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var appointments = await _context.Appointments
            .Where(appointment => appointment.PatientUserId == userId)
            .Include(appointment => appointment.PatientUser)
            .Include(appointment => appointment.DoctorProfile)
                .ThenInclude(doctor => doctor.User)
            .Include(appointment => appointment.ClinicService)
            .Include(appointment => appointment.Branch)
            .OrderByDescending(appointment => appointment.StartTime)
            .Select(appointment => new AppointmentResponseDto
            {
                Id = appointment.Id,
                PatientUserId = appointment.PatientUserId,
                PatientName = appointment.PatientUser != null ? appointment.PatientUser.FullName : string.Empty,
                DoctorProfileId = appointment.DoctorProfileId,
                DoctorName = appointment.DoctorProfile != null && appointment.DoctorProfile.User != null
                    ? appointment.DoctorProfile.User.FullName
                    : string.Empty,
                ClinicServiceId = appointment.ClinicServiceId,
                ServiceName = appointment.ClinicService != null ? appointment.ClinicService.Name : string.Empty,
                BranchId = appointment.BranchId,
                BranchName = appointment.Branch != null ? appointment.Branch.Name : string.Empty,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status.ToString(),
                PaymentStatus = appointment.PaymentStatus.ToString(),
                PaymentMethod = appointment.PaymentMethod.ToString(),
                Notes = appointment.Notes,
                CreatedAt = appointment.CreatedAt
            })
            .ToListAsync();

        return Ok(appointments);
    }

    // GET /api/appointments
    // Admin endpoint: returns all appointments.
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAppointments()
    {
        var appointments = await _context.Appointments
            .Include(appointment => appointment.PatientUser)
            .Include(appointment => appointment.DoctorProfile)
                .ThenInclude(doctor => doctor.User)
            .Include(appointment => appointment.ClinicService)
            .Include(appointment => appointment.Branch)
            .OrderByDescending(appointment => appointment.StartTime)
            .Select(appointment => new AppointmentResponseDto
            {
                Id = appointment.Id,
                PatientUserId = appointment.PatientUserId,
                PatientName = appointment.PatientUser != null ? appointment.PatientUser.FullName : string.Empty,
                DoctorProfileId = appointment.DoctorProfileId,
                DoctorName = appointment.DoctorProfile != null && appointment.DoctorProfile.User != null
                    ? appointment.DoctorProfile.User.FullName
                    : string.Empty,
                ClinicServiceId = appointment.ClinicServiceId,
                ServiceName = appointment.ClinicService != null ? appointment.ClinicService.Name : string.Empty,
                BranchId = appointment.BranchId,
                BranchName = appointment.Branch != null ? appointment.Branch.Name : string.Empty,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status.ToString(),
                PaymentStatus = appointment.PaymentStatus.ToString(),
                PaymentMethod = appointment.PaymentMethod.ToString(),
                Notes = appointment.Notes,
                CreatedAt = appointment.CreatedAt
            })
            .ToListAsync();

        return Ok(appointments);
    }

    // GET /api/appointments/{id}
    // Admin can view any appointment. Patient can view only own appointment.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentResponseDto>> GetAppointmentById(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var appointment = await _context.Appointments
            .Include(item => item.PatientUser)
            .Include(item => item.DoctorProfile)
                .ThenInclude(doctor => doctor.User)
            .Include(item => item.ClinicService)
            .Include(item => item.Branch)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (appointment is null)
        {
            return NotFound(new { message = "Appointment not found" });
        }

        if (role != "Admin" && appointment.PatientUserId != userId)
        {
            return Forbid();
        }

        var response = new AppointmentResponseDto
        {
            Id = appointment.Id,
            PatientUserId = appointment.PatientUserId,
            PatientName = appointment.PatientUser?.FullName ?? string.Empty,
            DoctorProfileId = appointment.DoctorProfileId,
            DoctorName = appointment.DoctorProfile?.User?.FullName ?? string.Empty,
            ClinicServiceId = appointment.ClinicServiceId,
            ServiceName = appointment.ClinicService?.Name ?? string.Empty,
            BranchId = appointment.BranchId,
            BranchName = appointment.Branch?.Name ?? string.Empty,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status.ToString(),
            PaymentStatus = appointment.PaymentStatus.ToString(),
            PaymentMethod = appointment.PaymentMethod.ToString(),
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt
        };

        return Ok(response);
    }


    // PATCH /api/appointments/{id}/approve
    // Admin-only endpoint.
    // Used to approve pay-at-clinic appointments.
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/approve")]
    public async Task<ActionResult<AppointmentResponseDto>> ApproveAppointment(Guid id)
    {
        var appointment = await _context.Appointments
            .Include(item => item.PatientUser)
            .Include(item => item.DoctorProfile)
                .ThenInclude(doctor => doctor.User)
            .Include(item => item.ClinicService)
            .Include(item => item.Branch)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (appointment is null)
        {
            return NotFound(new { message = "Appointment not found" });
        }

        if (appointment.Status != AppointmentStatus.PendingAdminApproval)
        {
            return BadRequest(new
            {
                message = "Only appointments pending admin approval can be approved"
            });
        }

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.PaymentStatus = PaymentStatus.PayAtClinic;

        await _context.SaveChangesAsync();

        return Ok(ToAppointmentResponse(appointment));
    }

    // PATCH /api/appointments/{id}/reject
    // Admin-only endpoint.
    // Used when admin rejects a pay-at-clinic appointment request.
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/reject")]
    public async Task<ActionResult<AppointmentResponseDto>> RejectAppointment(
        Guid id,
        AppointmentActionDto dto)
    {
        var appointment = await _context.Appointments
            .Include(item => item.PatientUser)
            .Include(item => item.DoctorProfile)
                .ThenInclude(doctor => doctor.User)
            .Include(item => item.ClinicService)
            .Include(item => item.Branch)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (appointment is null)
        {
            return NotFound(new { message = "Appointment not found" });
        }

        if (appointment.Status != AppointmentStatus.PendingAdminApproval)
        {
            return BadRequest(new
            {
                message = "Only appointments pending admin approval can be rejected"
            });
        }

        appointment.Status = AppointmentStatus.Rejected;
        appointment.CancelReason = dto.Reason?.Trim();

        await _context.SaveChangesAsync();

        return Ok(ToAppointmentResponse(appointment));
    }

    // PATCH /api/appointments/{id}/cancel
    // Patient can cancel own appointment.
    // Admin can cancel any appointment.
    [HttpPatch("{id:guid}/cancel")]
    public async Task<ActionResult<AppointmentResponseDto>> CancelAppointment(
        Guid id,
        AppointmentActionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var appointment = await _context.Appointments
            .Include(item => item.PatientUser)
            .Include(item => item.DoctorProfile)
                .ThenInclude(doctor => doctor.User)
            .Include(item => item.ClinicService)
            .Include(item => item.Branch)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (appointment is null)
        {
            return NotFound(new { message = "Appointment not found" });
        }

        var isAdmin = role == "Admin";
        var isOwner = appointment.PatientUserId == userId;

        if (!isAdmin && !isOwner)
        {
            return Forbid();
        }

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
        {
            return BadRequest(new
            {
                message = "This appointment cannot be cancelled"
            });
        }

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelReason = dto.Reason?.Trim();

        await _context.SaveChangesAsync();

        return Ok(ToAppointmentResponse(appointment));
    }

    // PATCH /api/appointments/{id}/reschedule
    // Patient can reschedule own appointment.
    // Admin can reschedule any appointment.
    [HttpPatch("{id:guid}/reschedule")]
    public async Task<ActionResult<AppointmentResponseDto>> RescheduleAppointment(
        Guid id,
        RescheduleAppointmentDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var appointment = await _context.Appointments
            .Include(item => item.PatientUser)
            .Include(item => item.DoctorProfile)
                .ThenInclude(doctor => doctor.User)
            .Include(item => item.ClinicService)
            .Include(item => item.Branch)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (appointment is null)
        {
            return NotFound(new { message = "Appointment not found" });
        }

        var isAdmin = role == "Admin";
        var isOwner = appointment.PatientUserId == userId;

        if (!isAdmin && !isOwner)
        {
            return Forbid();
        }

        if (appointment.Status is
            AppointmentStatus.Completed or
            AppointmentStatus.Cancelled or
            AppointmentStatus.Rejected or
            AppointmentStatus.Refunded)
        {
            return BadRequest(new
            {
                message = "This appointment cannot be rescheduled"
            });
        }

        if (dto.NewStartTime <= DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message = "New appointment time must be in the future"
            });
        }

        if (appointment.ClinicService is null)
        {
            return BadRequest(new
            {
                message = "Appointment service data is missing"
            });
        }

        var newStartTime = dto.NewStartTime;
        var newEndTime = newStartTime.AddMinutes(appointment.ClinicService.DurationMinutes);

        var hasConflict = await _context.Appointments.AnyAsync(existingAppointment =>
            existingAppointment.Id != appointment.Id &&
            existingAppointment.DoctorProfileId == appointment.DoctorProfileId &&
            existingAppointment.Status != AppointmentStatus.Cancelled &&
            existingAppointment.Status != AppointmentStatus.Rejected &&
            existingAppointment.Status != AppointmentStatus.Refunded &&
            newStartTime < existingAppointment.EndTime &&
            newEndTime > existingAppointment.StartTime);

        if (hasConflict)
        {
            return BadRequest(new
            {
                message = "Selected time slot is not available"
            });
        }

        appointment.AppointmentDate = newStartTime.Date;
        appointment.StartTime = newStartTime;
        appointment.EndTime = newEndTime;
        appointment.Status = AppointmentStatus.Rescheduled;

        if (!string.IsNullOrWhiteSpace(dto.Reason))
        {
            appointment.Notes = string.IsNullOrWhiteSpace(appointment.Notes)
                ? $"Reschedule reason: {dto.Reason.Trim()}"
                : $"{appointment.Notes}\nReschedule reason: {dto.Reason.Trim()}";
        }

        await _context.SaveChangesAsync();

        return Ok(ToAppointmentResponse(appointment));
    }



    // Converts Appointment entity into AppointmentResponseDto.
    // This keeps API responses clean and avoids exposing full database entities.
    private static AppointmentResponseDto ToAppointmentResponse(Appointment appointment)
    {
        return new AppointmentResponseDto
        {
            Id = appointment.Id,
            PatientUserId = appointment.PatientUserId,
            PatientName = appointment.PatientUser?.FullName ?? string.Empty,
            DoctorProfileId = appointment.DoctorProfileId,
            DoctorName = appointment.DoctorProfile?.User?.FullName ?? string.Empty,
            ClinicServiceId = appointment.ClinicServiceId,
            ServiceName = appointment.ClinicService?.Name ?? string.Empty,
            BranchId = appointment.BranchId,
            BranchName = appointment.Branch?.Name ?? string.Empty,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status.ToString(),
            PaymentStatus = appointment.PaymentStatus.ToString(),
            PaymentMethod = appointment.PaymentMethod.ToString(),
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt
        };
    }
}