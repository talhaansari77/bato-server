using BatoClinic.Api.Enums;

namespace BatoClinic.Api.DTOs.Appointments;

// Request body for patient booking.
// PatientUserId is not sent from mobile; we read it from JWT token.
public class CreateAppointmentDto
{
    public Guid ClinicServiceId { get; set; }

    public Guid BranchId { get; set; }

    public Guid DoctorProfileId { get; set; }

    public DateTime StartTime { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string? Notes { get; set; }
}