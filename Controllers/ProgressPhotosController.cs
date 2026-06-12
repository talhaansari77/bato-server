using System.Security.Claims;
using BatoClinic.Api.Data;
using BatoClinic.Api.DTOs.ProgressPhotos;
using BatoClinic.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BatoClinic.Api.Controllers;

[ApiController]
[Route("api/progress-photos")]
[Authorize]
public class ProgressPhotosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProgressPhotosController(AppDbContext context)
    {
        _context = context;
    }

    // POST /api/progress-photos
    // Patient adds a progress photo URL to their own profile.
    [Authorize(Roles = "Patient")]
    [HttpPost]
    public async Task<ActionResult<ProgressPhotoResponseDto>> CreateProgressPhoto(CreateProgressPhotoDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        if (string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            return BadRequest(new { message = "Image URL is required" });
        }

        var patientProfile = await _context.PatientProfiles
            .Include(profile => profile.User)
            .FirstOrDefaultAsync(profile => profile.UserId == userId);

        if (patientProfile is null)
        {
            return NotFound(new { message = "Patient profile not found" });
        }

        if (dto.TreatmentPlanId.HasValue)
        {
            var ownsTreatmentPlan = await _context.TreatmentPlans.AnyAsync(plan =>
                plan.Id == dto.TreatmentPlanId.Value &&
                plan.PatientProfileId == patientProfile.Id);

            if (!ownsTreatmentPlan)
            {
                return BadRequest(new { message = "Treatment plan not found for this patient" });
            }
        }

        var photo = new ProgressPhoto
        {
            Id = Guid.NewGuid(),
            PatientProfileId = patientProfile.Id,
            TreatmentPlanId = dto.TreatmentPlanId,
            ImageUrl = dto.ImageUrl.Trim(),
            PhotoType = string.IsNullOrWhiteSpace(dto.PhotoType) ? "Progress" : dto.PhotoType.Trim(),
            Notes = dto.Notes?.Trim(),
            TakenAt = dto.TakenAt ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProgressPhotos.Add(photo);
        await _context.SaveChangesAsync();

        photo.PatientProfile = patientProfile;

        return Ok(ToProgressPhotoResponse(photo));
    }

    // GET /api/progress-photos/my
    // Patient views their own progress photos.
    [Authorize(Roles = "Patient")]
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<ProgressPhotoResponseDto>>> GetMyProgressPhotos()
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

        var photos = await _context.ProgressPhotos
            .Where(photo => photo.PatientProfileId == patientProfile.Id)
            .Include(photo => photo.PatientProfile!)
                .ThenInclude(patient => patient.User)
            .OrderByDescending(photo => photo.TakenAt)
            .ToListAsync();

        return Ok(photos.Select(ToProgressPhotoResponse));
    }

    // GET /api/progress-photos/patient/{patientProfileId}
    // Doctor/Admin views progress photos for a patient.
    [Authorize(Roles = "Doctor,Admin")]
    [HttpGet("patient/{patientProfileId:guid}")]
    public async Task<ActionResult<IEnumerable<ProgressPhotoResponseDto>>> GetProgressPhotosByPatient(Guid patientProfileId)
    {
        var patientExists = await _context.PatientProfiles.AnyAsync(profile => profile.Id == patientProfileId);

        if (!patientExists)
        {
            return NotFound(new { message = "Patient profile not found" });
        }

        var photos = await _context.ProgressPhotos
            .Where(photo => photo.PatientProfileId == patientProfileId)
            .Include(photo => photo.PatientProfile!)
                .ThenInclude(patient => patient.User)
            .OrderByDescending(photo => photo.TakenAt)
            .ToListAsync();

        return Ok(photos.Select(ToProgressPhotoResponse));
    }

    // DELETE /api/progress-photos/{id}
    // Patient can delete own photo. Admin can delete any photo.
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteProgressPhoto(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var photo = await _context.ProgressPhotos
            .Include(item => item.PatientProfile)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (photo is null)
        {
            return NotFound(new { message = "Progress photo not found" });
        }

        var isAdmin = role == "Admin";

        if (!isAdmin)
        {
            var patientProfile = await _context.PatientProfiles
                .FirstOrDefaultAsync(profile => profile.UserId == userId);

            if (patientProfile is null || photo.PatientProfileId != patientProfile.Id)
            {
                return Forbid();
            }
        }

        _context.ProgressPhotos.Remove(photo);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Progress photo deleted successfully" });
    }

    private static ProgressPhotoResponseDto ToProgressPhotoResponse(ProgressPhoto photo)
    {
        return new ProgressPhotoResponseDto
        {
            Id = photo.Id,
            PatientProfileId = photo.PatientProfileId,
            PatientName = photo.PatientProfile?.User?.FullName ?? string.Empty,
            TreatmentPlanId = photo.TreatmentPlanId,
            ImageUrl = photo.ImageUrl,
            PhotoType = photo.PhotoType,
            Notes = photo.Notes,
            TakenAt = photo.TakenAt,
            CreatedAt = photo.CreatedAt
        };
    }
}