namespace BatoClinic.Api.DTOs.Admin;

// Summary data for the admin dashboard.
// This gives the mobile admin home screen quick business stats.
public class AdminDashboardSummaryDto
{
    public int TotalPatients { get; set; }

    public int TotalDoctors { get; set; }

    public int TotalBranches { get; set; }

    public int TotalServices { get; set; }

    public int TotalAppointments { get; set; }

    public int PendingApprovals { get; set; }

    public int TodayAppointments { get; set; }

    public int CompletedAppointments { get; set; }

    public int CancelledAppointments { get; set; }
}