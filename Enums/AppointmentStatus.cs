namespace BatoClinic.Api.Enums;

// AppointmentStatus tracks where the booking is in its lifecycle.
public enum AppointmentStatus
{
    PendingPayment = 1,
    PendingAdminApproval = 2,
    Confirmed = 3,
    Rejected = 4,
    Completed = 5,
    Cancelled = 6,
    Rescheduled = 7,
    NoShow = 8,
    Refunded = 9
}