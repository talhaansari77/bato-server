namespace BatoClinic.Api.DTOs.Appointments;

// Used when patient/admin changes appointment date/time.
public class RescheduleAppointmentDto
{
    public DateTime NewStartTime { get; set; }

    public string? Reason { get; set; }
}