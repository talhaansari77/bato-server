namespace BatoClinic.Api.DTOs.Appointments;

// Used when admin/patient sends a reason or note for an appointment action.
// Example: reject reason, cancel reason, approval note.
public class AppointmentActionDto
{
    public string? Reason { get; set; }
}