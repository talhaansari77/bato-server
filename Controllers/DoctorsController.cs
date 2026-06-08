using BatoClinic.Api.Data;
using BatoClinic.Api.DTOs.Doctors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BatoClinic.Api.Entities;

namespace BatoClinic.Api.Controllers;

[ApiController]
[Route("api/doctors")]
public class DoctorsController : ControllerBase
{
    private readonly AppDbContext _context;

    public DoctorsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/doctors
    // Public endpoint used by patient booking flow.
    // Returns all active and available doctors.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetDoctors()
    {
        var doctors = await _context.DoctorProfiles
            .Include(profile => profile.User)
            .Where(profile => profile.IsAvailable && profile.User != null && profile.User.IsActive)
            .OrderBy(profile => profile.User!.FullName)
            .Select(profile => new DoctorResponseDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.User != null ? profile.User.FullName : string.Empty,
                Email = profile.User != null ? profile.User.Email : null,
                PhoneNumber = profile.User != null ? profile.User.PhoneNumber : null,
                AvatarUrl = profile.User != null ? profile.User.AvatarUrl : null,
                Specialization = profile.Specialization,
                LicenseNumber = profile.LicenseNumber,
                ExperienceYears = profile.ExperienceYears,
                Bio = profile.Bio,
                ConsultationFee = profile.ConsultationFee,
                IsAvailable = profile.IsAvailable,
                CreatedAt = profile.CreatedAt
            })
            .ToListAsync();

        return Ok(doctors);
    }

    // GET /api/doctors/{id}
    // Returns one doctor profile by doctor profile id.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DoctorResponseDto>> GetDoctorById(Guid id)
    {
        var doctor = await _context.DoctorProfiles
            .Include(profile => profile.User)
            .Where(profile => profile.Id == id)
            .Select(profile => new DoctorResponseDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.User != null ? profile.User.FullName : string.Empty,
                Email = profile.User != null ? profile.User.Email : null,
                PhoneNumber = profile.User != null ? profile.User.PhoneNumber : null,
                AvatarUrl = profile.User != null ? profile.User.AvatarUrl : null,
                Specialization = profile.Specialization,
                LicenseNumber = profile.LicenseNumber,
                ExperienceYears = profile.ExperienceYears,
                Bio = profile.Bio,
                ConsultationFee = profile.ConsultationFee,
                IsAvailable = profile.IsAvailable,
                CreatedAt = profile.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (doctor is null)
        {
            return NotFound(new
            {
                message = "Doctor not found"
            });
        }

        return Ok(doctor);
    }

    // GET /api/doctors/available?serviceId={serviceId}&branchId={branchId}
    // Used by booking flow after patient selects service and branch.
    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetAvailableDoctors(
        [FromQuery] Guid serviceId,
        [FromQuery] Guid branchId)
    {
        if (serviceId == Guid.Empty || branchId == Guid.Empty)
        {
            return BadRequest(new
            {
                message = "serviceId and branchId are required"
            });
        }

        var doctors = await _context.DoctorProfiles
            .Include(profile => profile.User)
            .Where(profile =>
                profile.IsAvailable &&
                profile.User != null &&
                profile.User.IsActive &&
                profile.DoctorServices.Any(item => item.ClinicServiceId == serviceId) &&
                profile.DoctorBranches.Any(item => item.BranchId == branchId))
            .OrderBy(profile => profile.User!.FullName)
            .Select(profile => new DoctorResponseDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.User != null ? profile.User.FullName : string.Empty,
                Email = profile.User != null ? profile.User.Email : null,
                PhoneNumber = profile.User != null ? profile.User.PhoneNumber : null,
                AvatarUrl = profile.User != null ? profile.User.AvatarUrl : null,
                Specialization = profile.Specialization,
                LicenseNumber = profile.LicenseNumber,
                ExperienceYears = profile.ExperienceYears,
                Bio = profile.Bio,
                ConsultationFee = profile.ConsultationFee,
                IsAvailable = profile.IsAvailable,
                CreatedAt = profile.CreatedAt
            })
            .ToListAsync();

        return Ok(doctors);
    }

    // POST /api/doctors/{doctorId}/branches
    // Admin-only endpoint to replace branch assignments for a doctor.
    [Authorize(Roles = "Admin")]
    [HttpPost("{doctorId:guid}/branches")]
    public async Task<ActionResult> AssignBranches(Guid doctorId, AssignDoctorBranchesDto dto)
    {
        var doctor = await _context.DoctorProfiles
            .FirstOrDefaultAsync(profile => profile.Id == doctorId);

        if (doctor is null)
        {
            return NotFound(new { message = "Doctor not found" });
        }

        var validBranchesCount = await _context.Branches
            .CountAsync(branch => dto.BranchIds.Contains(branch.Id));

        if (validBranchesCount != dto.BranchIds.Count)
        {
            return BadRequest(new { message = "One or more branches are invalid" });
        }

        var existingAssignments = await _context.DoctorBranches
            .Where(item => item.DoctorProfileId == doctorId)
            .ToListAsync();

        _context.DoctorBranches.RemoveRange(existingAssignments);

        var newAssignments = dto.BranchIds.Select(branchId => new DoctorBranch
        {
            DoctorProfileId = doctorId,
            BranchId = branchId
        });

        _context.DoctorBranches.AddRange(newAssignments);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Doctor branches updated successfully" });
    }

    // POST /api/doctors/{doctorId}/services
    // Admin-only endpoint to replace service assignments for a doctor.
    [Authorize(Roles = "Admin")]
    [HttpPost("{doctorId:guid}/services")]
    public async Task<ActionResult> AssignServices(Guid doctorId, AssignDoctorServicesDto dto)
    {
        var doctor = await _context.DoctorProfiles
            .FirstOrDefaultAsync(profile => profile.Id == doctorId);

        if (doctor is null)
        {
            return NotFound(new { message = "Doctor not found" });
        }

        var validServicesCount = await _context.ClinicServices
            .CountAsync(service => dto.ServiceIds.Contains(service.Id));

        if (validServicesCount != dto.ServiceIds.Count)
        {
            return BadRequest(new { message = "One or more services are invalid" });
        }

        var existingAssignments = await _context.DoctorServices
            .Where(item => item.DoctorProfileId == doctorId)
            .ToListAsync();

        _context.DoctorServices.RemoveRange(existingAssignments);

        var newAssignments = dto.ServiceIds.Select(serviceId => new DoctorService
        {
            DoctorProfileId = doctorId,
            ClinicServiceId = serviceId
        });

        _context.DoctorServices.AddRange(newAssignments);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Doctor services updated successfully" });
    }
}