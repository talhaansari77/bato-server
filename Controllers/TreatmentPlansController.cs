using System.Security.Claims;
using BatoClinic.Api.Data;
using BatoClinic.Api.DTOs.TreatmentPlans;
using BatoClinic.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BatoClinic.Api.Controllers;

[ApiController]
[Route("api/treatment-plans")]
[Authorize]
public class TreatmentPlansController : ControllerBase
{
    private readonly AppDbContext _context;

    public TreatmentPlansController(AppDbContext context)
    {
        _context = context;
    }

    // POST /api/treatment-plans
    // Doctor/Admin creates a treatment plan for a patient.
    [Authorize(Roles = "Doctor,Admin")]
    [HttpPost]
    public async Task<ActionResult<TreatmentPlanResponseDto>> CreateTreatmentPlan(CreateTreatmentPlanDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(new { message = "Treatment plan title is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            return BadRequest(new { message = "Treatment plan description is required" });
        }

        var patient = await _context.PatientProfiles
            .Include(profile => profile.User)
            .FirstOrDefaultAsync(profile => profile.Id == dto.PatientProfileId);

        if (patient is null)
        {
            return BadRequest(new { message = "Valid patient profile is required" });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        DoctorProfile? doctorProfile;

        if (role == "Admin")
        {
            doctorProfile = await _context.DoctorProfiles
                .Include(profile => profile.User)
                .FirstOrDefaultAsync();
        }
        else
        {
            doctorProfile = await _context.DoctorProfiles
                .Include(profile => profile.User)
                .FirstOrDefaultAsync(profile => profile.UserId == userId);
        }

        if (doctorProfile is null)
        {
            return BadRequest(new { message = "Doctor profile is required to create treatment plan" });
        }

        if (dto.AppointmentId.HasValue)
        {
            var appointmentExists = await _context.Appointments
                .AnyAsync(appointment => appointment.Id == dto.AppointmentId.Value);

            if (!appointmentExists)
            {
                return BadRequest(new { message = "Appointment not found" });
            }
        }

        var treatmentPlan = new TreatmentPlan
        {
            Id = Guid.NewGuid(),
            PatientProfileId = dto.PatientProfileId,
            DoctorProfileId = doctorProfile.Id,
            AppointmentId = dto.AppointmentId,
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        _context.TreatmentPlans.Add(treatmentPlan);
        await _context.SaveChangesAsync();

        treatmentPlan.PatientProfile = patient;
        treatmentPlan.DoctorProfile = doctorProfile;

        return CreatedAtAction(
            nameof(GetTreatmentPlanById),
            new { id = treatmentPlan.Id },
            ToTreatmentPlanResponse(treatmentPlan)
        );
    }

    // GET /api/treatment-plans/my
    // Patient views their own treatment plans.
    [Authorize(Roles = "Patient")]
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<TreatmentPlanResponseDto>>> GetMyTreatmentPlans()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var patientProfile = await _context.PatientProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId);

        if (patientProfile is null)
        {
            return NotFound(new { message = "Patient profile not found" });
        }

        var plans = await _context.TreatmentPlans
            .Where(plan => plan.PatientProfileId == patientProfile.Id)
            .Include(plan => plan.PatientProfile!)
                .ThenInclude(patient => patient.User)
            .Include(plan => plan.DoctorProfile!)
                .ThenInclude(doctor => doctor.User)
            .OrderByDescending(plan => plan.CreatedAt)
            .Select(plan => ToTreatmentPlanResponse(plan))
            .ToListAsync();

        return Ok(plans);
    }

    // GET /api/treatment-plans/patient/{patientProfileId}
    // Doctor/Admin views treatment plans for a specific patient.
    [Authorize(Roles = "Doctor,Admin")]
    [HttpGet("patient/{patientProfileId:guid}")]
    public async Task<ActionResult<IEnumerable<TreatmentPlanResponseDto>>> GetTreatmentPlansByPatient(Guid patientProfileId)
    {
        var patientExists = await _context.PatientProfiles.AnyAsync(profile => profile.Id == patientProfileId);

        if (!patientExists)
        {
            return NotFound(new { message = "Patient profile not found" });
        }

        var plans = await _context.TreatmentPlans
            .Where(plan => plan.PatientProfileId == patientProfileId)
            .Include(plan => plan.PatientProfile!)
                .ThenInclude(patient => patient.User)
            .Include(plan => plan.DoctorProfile!)
                .ThenInclude(doctor => doctor.User)
            .OrderByDescending(plan => plan.CreatedAt)
            .Select(plan => ToTreatmentPlanResponse(plan))
            .ToListAsync();

        return Ok(plans);
    }

    // GET /api/treatment-plans/{id}
    // Patient can view own plan. Doctor/Admin can view treatment plan.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TreatmentPlanResponseDto>> GetTreatmentPlanById(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var plan = await _context.TreatmentPlans
            .Include(item => item.PatientProfile!)
                .ThenInclude(patient => patient.User)
            .Include(item => item.DoctorProfile!)
                .ThenInclude(doctor => doctor.User)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (plan is null)
        {
            return NotFound(new { message = "Treatment plan not found" });
        }

        if (role == "Patient")
        {
            var patientProfile = await _context.PatientProfiles
                .FirstOrDefaultAsync(profile => profile.UserId == userId);

            if (patientProfile is null || plan.PatientProfileId != patientProfile.Id)
            {
                return Forbid();
            }
        }

        return Ok(ToTreatmentPlanResponse(plan));
    }

    // Converts TreatmentPlan entity into TreatmentPlanResponseDto.
    // This keeps API responses clean and avoids exposing full database objects.
    private static TreatmentPlanResponseDto ToTreatmentPlanResponse(TreatmentPlan plan)
    {
        return new TreatmentPlanResponseDto
        {
            Id = plan.Id,
            PatientProfileId = plan.PatientProfileId,
            PatientName = plan.PatientProfile?.User?.FullName ?? string.Empty,
            DoctorProfileId = plan.DoctorProfileId,
            DoctorName = plan.DoctorProfile?.User?.FullName ?? string.Empty,
            AppointmentId = plan.AppointmentId,
            Title = plan.Title,
            Description = plan.Description,
            StartDate = plan.StartDate,
            EndDate = plan.EndDate,
            Status = plan.Status,
            CreatedAt = plan.CreatedAt
        };
    }
}