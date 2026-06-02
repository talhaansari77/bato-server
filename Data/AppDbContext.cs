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
    }
}