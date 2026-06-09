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
}