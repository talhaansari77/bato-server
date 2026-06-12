using System.Security.Claims;
using BatoClinic.Api.Data;
using BatoClinic.Api.DTOs.Patients;
using BatoClinic.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BatoClinic.Api.Controllers;

[ApiController]
[Route("api/patients")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PatientsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/patients/me
    // Patient can view their own patient profile.
    [Authorize(Roles = "Patient")]
    [HttpGet("me")]
    public async Task<ActionResult<PatientResponseDto>> GetMyPatientProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var patient = await _context.PatientProfiles
            .Include(profile => profile.User)
            .Where(profile => profile.UserId == userId)
            .Select(profile => new PatientResponseDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.User != null ? profile.User.FullName : string.Empty,
                Email = profile.User != null ? profile.User.Email : null,
                PhoneNumber = profile.User != null ? profile.User.PhoneNumber : null,
                AvatarUrl = profile.User != null ? profile.User.AvatarUrl : null,
                DateOfBirth = profile.DateOfBirth,
                Gender = profile.Gender,
                EmergencyContactName = profile.EmergencyContactName,
                EmergencyContactPhone = profile.EmergencyContactPhone,
                MedicalNotes = profile.MedicalNotes,
                VipStatus = profile.VipStatus,
                IsActive = profile.User != null && profile.User.IsActive,
                CreatedAt = profile.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (patient is null)
        {
            return NotFound(new { message = "Patient profile not found" });
        }

        return Ok(patient);
    }

    // PATCH /api/patients/me
    // Patient updates their own profile.
    [Authorize(Roles = "Patient")]
    [HttpPatch("me")]
    public async Task<ActionResult<PatientResponseDto>> UpdateMyPatientProfile(UpdatePatientProfileDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var patientProfile = await _context.PatientProfiles
            .Include(profile => profile.User)
            .FirstOrDefaultAsync(profile => profile.UserId == userId);

        if (patientProfile is null || patientProfile.User is null)
        {
            return NotFound(new { message = "Patient profile not found" });
        }

        if (!string.IsNullOrWhiteSpace(dto.FullName))
        {
            patientProfile.User.FullName = dto.FullName.Trim();
        }

        if (dto.PhoneNumber is not null)
        {
            patientProfile.User.PhoneNumber = dto.PhoneNumber.Trim();
        }

        if (dto.AvatarUrl is not null)
        {
            patientProfile.User.AvatarUrl = dto.AvatarUrl.Trim();
        }

        if (dto.DateOfBirth.HasValue)
        {
            patientProfile.DateOfBirth = dto.DateOfBirth.Value;
        }

        if (dto.Gender is not null)
        {
            patientProfile.Gender = dto.Gender.Trim();
        }

        if (dto.EmergencyContactName is not null)
        {
            patientProfile.EmergencyContactName = dto.EmergencyContactName.Trim();
        }

        if (dto.EmergencyContactPhone is not null)
        {
            patientProfile.EmergencyContactPhone = dto.EmergencyContactPhone.Trim();
        }

        if (dto.MedicalNotes is not null)
        {
            patientProfile.MedicalNotes = dto.MedicalNotes.Trim();
        }

        await _context.SaveChangesAsync();

        return Ok(ToPatientResponse(patientProfile));
    }

    // GET /api/patients
    // Admin can view all patients.
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatientResponseDto>>> GetPatients()
    {
        var patients = await _context.PatientProfiles
            .Include(profile => profile.User)
            .OrderBy(profile => profile.User!.FullName)
            .Select(profile => new PatientResponseDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.User != null ? profile.User.FullName : string.Empty,
                Email = profile.User != null ? profile.User.Email : null,
                PhoneNumber = profile.User != null ? profile.User.PhoneNumber : null,
                AvatarUrl = profile.User != null ? profile.User.AvatarUrl : null,
                DateOfBirth = profile.DateOfBirth,
                Gender = profile.Gender,
                EmergencyContactName = profile.EmergencyContactName,
                EmergencyContactPhone = profile.EmergencyContactPhone,
                MedicalNotes = profile.MedicalNotes,
                VipStatus = profile.VipStatus,
                IsActive = profile.User != null && profile.User.IsActive,
                CreatedAt = profile.CreatedAt
            })
            .ToListAsync();

        return Ok(patients);
    }

    // GET /api/patients/{id}
    // Admin can view one patient by patient profile id.
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PatientResponseDto>> GetPatientById(Guid id)
    {
        var patient = await _context.PatientProfiles
            .Include(profile => profile.User)
            .FirstOrDefaultAsync(profile => profile.Id == id);

        if (patient is null)
        {
            return NotFound(new { message = "Patient not found" });
        }

        return Ok(ToPatientResponse(patient));
    }

    private static PatientResponseDto ToPatientResponse(PatientProfile profile)
    {
        return new PatientResponseDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FullName = profile.User?.FullName ?? string.Empty,
            Email = profile.User?.Email,
            PhoneNumber = profile.User?.PhoneNumber,
            AvatarUrl = profile.User?.AvatarUrl,
            DateOfBirth = profile.DateOfBirth,
            Gender = profile.Gender,
            EmergencyContactName = profile.EmergencyContactName,
            EmergencyContactPhone = profile.EmergencyContactPhone,
            MedicalNotes = profile.MedicalNotes,
            VipStatus = profile.VipStatus,
            IsActive = profile.User?.IsActive ?? false,
            CreatedAt = profile.CreatedAt
        };
    }
}