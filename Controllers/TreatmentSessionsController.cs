using System.Security.Claims;
using BatoClinic.Api.Data;
using BatoClinic.Api.DTOs.TreatmentSessions;
using BatoClinic.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BatoClinic.Api.Controllers;

[ApiController]
[Authorize]
public class TreatmentSessionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TreatmentSessionsController(AppDbContext context)
    {
        _context = context;
    }

    // POST /api/treatment-plans/{planId}/sessions
    // Doctor/Admin adds a session to a treatment plan.
    [Authorize(Roles = "Doctor,Admin")]
    [HttpPost("api/treatment-plans/{planId:guid}/sessions")]
    public async Task<ActionResult<TreatmentSessionResponseDto>> CreateSession(
        Guid planId,
        CreateTreatmentSessionDto dto)
    {
        var plan = await _context.TreatmentPlans
            .FirstOrDefaultAsync(plan => plan.Id == planId);

        if (plan is null)
        {
            return NotFound(new { message = "Treatment plan not found" });
        }

        if (dto.SessionNumber <= 0)
        {
            return BadRequest(new { message = "Session number must be greater than 0" });
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

        var session = new TreatmentSession
        {
            Id = Guid.NewGuid(),
            TreatmentPlanId = planId,
            AppointmentId = dto.AppointmentId,
            SessionNumber = dto.SessionNumber,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Scheduled" : dto.Status.Trim(),
            Notes = dto.Notes?.Trim(),
            ScheduledDate = dto.ScheduledDate,
            CreatedAt = DateTime.UtcNow
        };

        _context.TreatmentSessions.Add(session);
        await _context.SaveChangesAsync();

        return Ok(ToSessionResponse(session));
    }

    // GET /api/treatment-plans/{planId}/sessions
    // Patient can view sessions for their own plan.
    // Doctor/Admin can view sessions.
    [HttpGet("api/treatment-plans/{planId:guid}/sessions")]
    public async Task<ActionResult<IEnumerable<TreatmentSessionResponseDto>>> GetSessionsByPlan(Guid planId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var plan = await _context.TreatmentPlans
            .Include(plan => plan.PatientProfile)
            .FirstOrDefaultAsync(plan => plan.Id == planId);

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

        var sessions = await _context.TreatmentSessions
            .Where(session => session.TreatmentPlanId == planId)
            .OrderBy(session => session.SessionNumber)
            .Select(session => ToSessionResponse(session))
            .ToListAsync();

        return Ok(sessions);
    }

    // PATCH /api/treatment-sessions/{id}
    // Doctor/Admin updates a treatment session.
    [Authorize(Roles = "Doctor,Admin")]
    [HttpPatch("api/treatment-sessions/{id:guid}")]
    public async Task<ActionResult<TreatmentSessionResponseDto>> UpdateSession(
        Guid id,
        UpdateTreatmentSessionDto dto)
    {
        var session = await _context.TreatmentSessions
            .FirstOrDefaultAsync(session => session.Id == id);

        if (session is null)
        {
            return NotFound(new { message = "Treatment session not found" });
        }

        if (dto.AppointmentId.HasValue)
        {
            var appointmentExists = await _context.Appointments
                .AnyAsync(appointment => appointment.Id == dto.AppointmentId.Value);

            if (!appointmentExists)
            {
                return BadRequest(new { message = "Appointment not found" });
            }

            session.AppointmentId = dto.AppointmentId.Value;
        }

        if (dto.SessionNumber.HasValue)
        {
            if (dto.SessionNumber.Value <= 0)
            {
                return BadRequest(new { message = "Session number must be greater than 0" });
            }

            session.SessionNumber = dto.SessionNumber.Value;
        }

        if (dto.Status is not null)
        {
            session.Status = string.IsNullOrWhiteSpace(dto.Status)
                ? session.Status
                : dto.Status.Trim();
        }

        if (dto.Notes is not null)
        {
            session.Notes = dto.Notes.Trim();
        }

        if (dto.ScheduledDate.HasValue)
        {
            session.ScheduledDate = dto.ScheduledDate.Value;
        }

        await _context.SaveChangesAsync();

        return Ok(ToSessionResponse(session));
    }

    // DELETE /api/treatment-sessions/{id}
    // Doctor/Admin deletes a session from a treatment plan.
    [Authorize(Roles = "Doctor,Admin")]
    [HttpDelete("api/treatment-sessions/{id:guid}")]
    public async Task<ActionResult> DeleteSession(Guid id)
    {
        var session = await _context.TreatmentSessions
            .FirstOrDefaultAsync(session => session.Id == id);

        if (session is null)
        {
            return NotFound(new { message = "Treatment session not found" });
        }

        _context.TreatmentSessions.Remove(session);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Treatment session deleted successfully" });
    }

    private static TreatmentSessionResponseDto ToSessionResponse(TreatmentSession session)
    {
        return new TreatmentSessionResponseDto
        {
            Id = session.Id,
            TreatmentPlanId = session.TreatmentPlanId,
            AppointmentId = session.AppointmentId,
            SessionNumber = session.SessionNumber,
            Status = session.Status,
            Notes = session.Notes,
            ScheduledDate = session.ScheduledDate,
            CreatedAt = session.CreatedAt
        };
    }
}