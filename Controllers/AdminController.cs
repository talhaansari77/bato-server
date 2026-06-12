using BatoClinic.Api.Data;
using BatoClinic.Api.DTOs.Admin;
using BatoClinic.Api.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BatoClinic.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/admin/dashboard-summary
    // Admin-only endpoint.
    // Returns quick stats for the admin dashboard screen.
    [HttpGet("dashboard-summary")]
    public async Task<ActionResult<AdminDashboardSummaryDto>> GetDashboardSummary()
    {
        var today = DateTime.UtcNow.Date;

        var summary = new AdminDashboardSummaryDto
        {
            TotalPatients = await _context.PatientProfiles.CountAsync(),
            TotalDoctors = await _context.DoctorProfiles.CountAsync(),
            TotalBranches = await _context.Branches.CountAsync(branch => branch.IsActive),
            TotalServices = await _context.ClinicServices.CountAsync(service => service.IsActive),
            TotalAppointments = await _context.Appointments.CountAsync(),

            PendingApprovals = await _context.Appointments.CountAsync(appointment =>
                appointment.Status == AppointmentStatus.PendingAdminApproval),

            TodayAppointments = await _context.Appointments.CountAsync(appointment =>
                appointment.AppointmentDate == today),

            CompletedAppointments = await _context.Appointments.CountAsync(appointment =>
                appointment.Status == AppointmentStatus.Completed),

            CancelledAppointments = await _context.Appointments.CountAsync(appointment =>
                appointment.Status == AppointmentStatus.Cancelled)
        };

        return Ok(summary);
    }
}