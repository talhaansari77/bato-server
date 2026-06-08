using BatoClinic.Api.Data;
using BatoClinic.Api.DTOs.Doctors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
}