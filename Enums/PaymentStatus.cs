namespace BatoClinic.Api.Enums;

// PaymentStatus tracks whether the appointment is paid or unpaid.
public enum PaymentStatus
{
    Unpaid = 1,
    Paid = 2,
    PartiallyPaid = 3,
    Refunded = 4,
    Failed = 5,
    PayAtClinic = 6
}