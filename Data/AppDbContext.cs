using BatoClinic.Api.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BatoClinic.Api.Data;

// AppDbContext is the main bridge between C# entities and the MySQL database.
// IdentityDbContext adds ASP.NET Identity tables automatically, like Users, Roles, Claims, Logins.
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();

    public DbSet<ClinicService> ClinicServices => Set<ClinicService>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<DoctorBranch> DoctorBranches => Set<DoctorBranch>();

    public DbSet<DoctorService> DoctorServices => Set<DoctorService>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<PatientProfile> PatientProfiles => Set<PatientProfile>();
    public DbSet<TreatmentPlan> TreatmentPlans => Set<TreatmentPlan>();
    public DbSet<TreatmentSession> TreatmentSessions => Set<TreatmentSession>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<ProgressPhoto> ProgressPhotos => Set<ProgressPhoto>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Branch table setup
        builder.Entity<Branch>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.Country).HasMaxLength(100);
        });

        // ServiceCategory table setup
        builder.Entity<ServiceCategory>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        // ClinicService table setup
        builder.Entity<ClinicService>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();

            // decimal precision prevents money values from being stored incorrectly.
            entity.Property(x => x.Price).HasPrecision(10, 2);

            entity.HasOne(x => x.ServiceCategory)
                .WithMany(x => x.Services)
                .HasForeignKey(x => x.ServiceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DoctorProfile table setup
        builder.Entity<DoctorProfile>(entity =>
        {
            entity.Property(x => x.Specialization).HasMaxLength(150).IsRequired();
            entity.Property(x => x.LicenseNumber).HasMaxLength(100);
            entity.Property(x => x.Bio).HasMaxLength(1000);
            entity.Property(x => x.ConsultationFee).HasPrecision(10, 2);

            entity.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<DoctorProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        // DoctorBranch join table setup
        builder.Entity<DoctorBranch>(entity =>
        {
            entity.HasIndex(x => new { x.DoctorProfileId, x.BranchId }).IsUnique();

            entity.HasOne(x => x.DoctorProfile)
                .WithMany(x => x.DoctorBranches)
                .HasForeignKey(x => x.DoctorProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Branch)
                .WithMany(x => x.DoctorBranches)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DoctorService join table setup
        builder.Entity<DoctorService>(entity =>
        {
            entity.HasIndex(x => new { x.DoctorProfileId, x.ClinicServiceId }).IsUnique();

            entity.HasOne(x => x.DoctorProfile)
                .WithMany(x => x.DoctorServices)
                .HasForeignKey(x => x.DoctorProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ClinicService)
                .WithMany(x => x.DoctorServices)
                .HasForeignKey(x => x.ClinicServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Appointment table setup
        builder.Entity<Appointment>(entity =>
        {
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.CancelReason).HasMaxLength(500);

            // Store enums as strings for readability in database.
            entity.Property(x => x.Status).HasConversion<string>();
            entity.Property(x => x.PaymentStatus).HasConversion<string>();
            entity.Property(x => x.PaymentMethod).HasConversion<string>();

            entity.HasOne(x => x.PatientUser)
                .WithMany()
                .HasForeignKey(x => x.PatientUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DoctorProfile)
                .WithMany()
                .HasForeignKey(x => x.DoctorProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ClinicService)
                .WithMany()
                .HasForeignKey(x => x.ClinicServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PatientUserId);
            entity.HasIndex(x => x.DoctorProfileId);
            entity.HasIndex(x => x.BranchId);
            entity.HasIndex(x => x.ClinicServiceId);
            entity.HasIndex(x => x.AppointmentDate);
        });

        // PatientProfile table setup
        builder.Entity<PatientProfile>(entity =>
        {
            entity.Property(x => x.Gender).HasMaxLength(30);
            entity.Property(x => x.EmergencyContactName).HasMaxLength(150);
            entity.Property(x => x.EmergencyContactPhone).HasMaxLength(30);
            entity.Property(x => x.MedicalNotes).HasMaxLength(1000);

            entity.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<PatientProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        // TreatmentPlan table setup
        builder.Entity<TreatmentPlan>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();

            entity.HasOne(x => x.PatientProfile)
                .WithMany()
                .HasForeignKey(x => x.PatientProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DoctorProfile)
                .WithMany()
                .HasForeignKey(x => x.DoctorProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Appointment)
                .WithMany()
                .HasForeignKey(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => x.PatientProfileId);
            entity.HasIndex(x => x.DoctorProfileId);
            entity.HasIndex(x => x.AppointmentId);
        });

        // TreatmentSession table setup
        builder.Entity<TreatmentSession>(entity =>
        {
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);

            entity.HasOne(x => x.TreatmentPlan)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.TreatmentPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Appointment)
                .WithMany()
                .HasForeignKey(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => x.TreatmentPlanId);
            entity.HasIndex(x => x.AppointmentId);
        });

        // MedicalRecord table setup
        builder.Entity<MedicalRecord>(entity =>
        {
            entity.Property(x => x.Diagnosis).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Prescription).HasMaxLength(2000);
            entity.Property(x => x.FollowUpInstructions).HasMaxLength(1000);

            entity.HasOne(x => x.PatientProfile)
                .WithMany()
                .HasForeignKey(x => x.PatientProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DoctorProfile)
                .WithMany()
                .HasForeignKey(x => x.DoctorProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Appointment)
                .WithMany()
                .HasForeignKey(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => x.PatientProfileId);
            entity.HasIndex(x => x.DoctorProfileId);
            entity.HasIndex(x => x.AppointmentId);
            entity.HasIndex(x => x.RecordDate);
        });

        // ProgressPhoto table setup
builder.Entity<ProgressPhoto>(entity =>
{
    entity.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
    entity.Property(x => x.PhotoType).HasMaxLength(50).IsRequired();
    entity.Property(x => x.Notes).HasMaxLength(500);

    entity.HasOne(x => x.PatientProfile)
        .WithMany()
        .HasForeignKey(x => x.PatientProfileId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(x => x.TreatmentPlan)
        .WithMany()
        .HasForeignKey(x => x.TreatmentPlanId)
        .OnDelete(DeleteBehavior.SetNull);

    entity.HasIndex(x => x.PatientProfileId);
    entity.HasIndex(x => x.TreatmentPlanId);
    entity.HasIndex(x => x.TakenAt);
});

// Notification table setup
builder.Entity<Notification>(entity =>
{
    entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
    entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
    entity.Property(x => x.Type).HasMaxLength(50).IsRequired();

    entity.HasOne(x => x.User)
        .WithMany()
        .HasForeignKey(x => x.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasIndex(x => x.UserId);
    entity.HasIndex(x => x.IsRead);
    entity.HasIndex(x => x.CreatedAt);
});

// RefreshToken table setup
builder.Entity<RefreshToken>(entity =>
{
    entity.Property(x => x.TokenHash).HasMaxLength(500).IsRequired();

    entity.HasOne(x => x.User)
        .WithMany()
        .HasForeignKey(x => x.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasIndex(x => x.UserId);
    entity.HasIndex(x => x.TokenHash);
    entity.HasIndex(x => x.ExpiresAt);
});
    }
}