using System.Security.Claims;
using BatoClinic.Api.Data;
using BatoClinic.Api.DTOs.MedicalRecords;
using BatoClinic.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BatoClinic.Api.Controllers;

[ApiController]
[Route("api/medical-records")]
[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MedicalRecordsController(AppDbContext context)
    {
        _context = context;
    }

    // POST /api/medical-records
    // Doctor/Admin creates a medical record for a patient.
    [Authorize(Roles = "Doctor,Admin")]
    [HttpPost]
    public async Task<ActionResult<MedicalRecordResponseDto>> CreateMedicalRecord(CreateMedicalRecordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Diagnosis))
        {
            return BadRequest(new { message = "Diagnosis is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.Notes))
        {
            return BadRequest(new { message = "Medical notes are required" });
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
            return BadRequest(new { message = "Doctor profile is required to create medical record" });
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

        var record = new MedicalRecord
        {
            Id = Guid.NewGuid(),
            PatientProfileId = dto.PatientProfileId,
            DoctorProfileId = doctorProfile.Id,
            AppointmentId = dto.AppointmentId,
            Diagnosis = dto.Diagnosis.Trim(),
            Notes = dto.Notes.Trim(),
            Prescription = dto.Prescription?.Trim(),
            FollowUpInstructions = dto.FollowUpInstructions?.Trim(),
            RecordDate = dto.RecordDate ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();

        record.PatientProfile = patient;
        record.DoctorProfile = doctorProfile;

        return CreatedAtAction(
            nameof(GetMedicalRecordById),
            new { id = record.Id },
            ToMedicalRecordResponse(record)
        );
    }

    // GET /api/medical-records/my
    // Patient views their own medical records.
    [Authorize(Roles = "Patient")]
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<MedicalRecordResponseDto>>> GetMyMedicalRecords()
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

        var records = await _context.MedicalRecords
            .Where(record => record.PatientProfileId == patientProfile.Id)
            .Include(record => record.PatientProfile!)
                .ThenInclude(patient => patient.User)
            .Include(record => record.DoctorProfile!)
                .ThenInclude(doctor => doctor.User)
            .OrderByDescending(record => record.RecordDate)
            .ToListAsync();

        return Ok(records.Select(ToMedicalRecordResponse));
    }

    // GET /api/medical-records/patient/{patientProfileId}
    // Doctor/Admin views records for a specific patient.
    [Authorize(Roles = "Doctor,Admin")]
    [HttpGet("patient/{patientProfileId:guid}")]
    public async Task<ActionResult<IEnumerable<MedicalRecordResponseDto>>> GetMedicalRecordsByPatient(Guid patientProfileId)
    {
        var patientExists = await _context.PatientProfiles.AnyAsync(profile => profile.Id == patientProfileId);

        if (!patientExists)
        {
            return NotFound(new { message = "Patient profile not found" });
        }

        var records = await _context.MedicalRecords
            .Where(record => record.PatientProfileId == patientProfileId)
            .Include(record => record.PatientProfile!)
                .ThenInclude(patient => patient.User)
            .Include(record => record.DoctorProfile!)
                .ThenInclude(doctor => doctor.User)
            .OrderByDescending(record => record.RecordDate)
            .ToListAsync();

        return Ok(records.Select(ToMedicalRecordResponse));
    }

    // GET /api/medical-records/{id}
    // Patient can view own record. Doctor/Admin can view medical record.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MedicalRecordResponseDto>> GetMedicalRecordById(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        var record = await _context.MedicalRecords
            .Include(item => item.PatientProfile!)
                .ThenInclude(patient => patient.User)
            .Include(item => item.DoctorProfile!)
                .ThenInclude(doctor => doctor.User)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (record is null)
        {
            return NotFound(new { message = "Medical record not found" });
        }

        if (role == "Patient")
        {
            var patientProfile = await _context.PatientProfiles
                .FirstOrDefaultAsync(profile => profile.UserId == userId);

            if (patientProfile is null || record.PatientProfileId != patientProfile.Id)
            {
                return Forbid();
            }
        }

        return Ok(ToMedicalRecordResponse(record));
    }

    // PATCH /api/medical-records/{id}
    // Doctor/Admin updates a medical record.
    [Authorize(Roles = "Doctor,Admin")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<MedicalRecordResponseDto>> UpdateMedicalRecord(
        Guid id,
        UpdateMedicalRecordDto dto)
    {
        var record = await _context.MedicalRecords
            .Include(item => item.PatientProfile!)
                .ThenInclude(patient => patient.User)
            .Include(item => item.DoctorProfile!)
                .ThenInclude(doctor => doctor.User)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (record is null)
        {
            return NotFound(new { message = "Medical record not found" });
        }

        if (!string.IsNullOrWhiteSpace(dto.Diagnosis))
        {
            record.Diagnosis = dto.Diagnosis.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            record.Notes = dto.Notes.Trim();
        }

        if (dto.Prescription is not null)
        {
            record.Prescription = dto.Prescription.Trim();
        }

        if (dto.FollowUpInstructions is not null)
        {
            record.FollowUpInstructions = dto.FollowUpInstructions.Trim();
        }

        if (dto.RecordDate.HasValue)
        {
            record.RecordDate = dto.RecordDate.Value;
        }

        await _context.SaveChangesAsync();

        return Ok(ToMedicalRecordResponse(record));
    }

    private static MedicalRecordResponseDto ToMedicalRecordResponse(MedicalRecord record)
    {
        return new MedicalRecordResponseDto
        {
            Id = record.Id,
            PatientProfileId = record.PatientProfileId,
            PatientName = record.PatientProfile?.User?.FullName ?? string.Empty,
            DoctorProfileId = record.DoctorProfileId,
            DoctorName = record.DoctorProfile?.User?.FullName ?? string.Empty,
            AppointmentId = record.AppointmentId,
            Diagnosis = record.Diagnosis,
            Notes = record.Notes,
            Prescription = record.Prescription,
            FollowUpInstructions = record.FollowUpInstructions,
            RecordDate = record.RecordDate,
            CreatedAt = record.CreatedAt
        };
    }
}