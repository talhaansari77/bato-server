namespace BatoClinic.Api.DTOs.Appointments;

// Clean appointment response for mobile app and admin panels.
public class AppointmentResponseDto
{
    public Guid Id { get; set; }

    public string PatientUserId { get; set; } = string.Empty;

    public string PatientName { get; set; } = string.Empty;

    public Guid DoctorProfileId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public Guid ClinicServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public Guid BranchId { get; set; }

    public string BranchName { get; set; } = string.Empty;

    public DateTime AppointmentDate { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public string PaymentMethod { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}