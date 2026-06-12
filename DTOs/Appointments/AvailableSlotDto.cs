namespace BatoClinic.Api.DTOs.Appointments;

// Returned to the mobile app when checking available appointment times.
public class AvailableSlotDto
{
    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsAvailable { get; set; }
}