using BatoClinic.Api.Enums;

namespace BatoClinic.Api.Entities;

// Appointment represents a patient booking.
// It connects patient user, doctor, service, branch, date/time, status, and payment status.
public class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string PatientUserId { get; set; } = string.Empty;

    public Guid DoctorProfileId { get; set; }

    public Guid ClinicServiceId { get; set; }

    public Guid BranchId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.PendingPayment;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Online;

    public string? Notes { get; set; }

    public string? CancelReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? PatientUser { get; set; }

    public DoctorProfile? DoctorProfile { get; set; }

    public ClinicService? ClinicService { get; set; }

    public Branch? Branch { get; set; }
}