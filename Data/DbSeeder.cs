using BatoClinic.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BatoClinic.Api.Data;

// DbSeeder adds default data into the database.
// This prevents us from manually adding categories, services, branches, and admin user every time.
public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
        await SeedDoctorUserAsync(context, userManager);
        await SeedBranchesAsync(context);
        await SeedServiceCategoriesAndServicesAsync(context);
        await SeedDoctorAssignmentsAsync(context, userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Patient", "Doctor", "Admin" };

        foreach (var role in roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(role);

            if (!roleExists)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@batoclinic.com";
        const string adminPassword = "Password123!";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            FullName = "BATO Admin",
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            PhoneNumber = "+96550000001",
            RoleType = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }

    private static async Task SeedDoctorUserAsync(
    AppDbContext context,
    UserManager<ApplicationUser> userManager)
    {
        const string doctorEmail = "doctor@batoclinic.com";
        const string doctorPassword = "Password123!";

        var existingDoctor = await userManager.FindByEmailAsync(doctorEmail);

        if (existingDoctor is not null)
        {
            var existingProfile = await context.DoctorProfiles
                .AnyAsync(profile => profile.UserId == existingDoctor.Id);

            if (!existingProfile)
            {
                context.DoctorProfiles.Add(new DoctorProfile
                {
                    UserId = existingDoctor.Id,
                    Specialization = "Dermatology & Aesthetic Medicine",
                    LicenseNumber = "BATO-DR-001",
                    ExperienceYears = 8,
                    Bio = "Specialist in premium skin rejuvenation, facial treatments, and personalized medical beauty plans.",
                    ConsultationFee = 25,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }

            return;
        }

        var doctor = new ApplicationUser
        {
            FullName = "Dr. Sarah Khan",
            UserName = doctorEmail,
            Email = doctorEmail,
            EmailConfirmed = true,
            PhoneNumber = "+96550000002",
            RoleType = "Doctor",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(doctor, doctorPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(doctor, "Doctor");

            context.DoctorProfiles.Add(new DoctorProfile
            {
                UserId = doctor.Id,
                Specialization = "Dermatology & Aesthetic Medicine",
                LicenseNumber = "BATO-DR-001",
                ExperienceYears = 8,
                Bio = "Specialist in premium skin rejuvenation, facial treatments, and personalized medical beauty plans.",
                ConsultationFee = 25,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedBranchesAsync(AppDbContext context)
    {
        var hasBranches = await context.Branches.AnyAsync();

        if (hasBranches)
        {
            return;
        }

        var mainBranch = new Branch
        {
            Name = "BATO Clinic - Main Branch",
            Phone = "+96550000000",
            Address = "Mishrif, Mubarak Al-Kabeer, Kuwait",
            City = "Mishrif",
            Country = "Kuwait",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Branches.Add(mainBranch);
        await context.SaveChangesAsync();
    }

    private static async Task SeedServiceCategoriesAndServicesAsync(AppDbContext context)
    {
        var hasCategories = await context.ServiceCategories.AnyAsync();

        if (hasCategories)
        {
            return;
        }

        var skinCategory = new ServiceCategory
        {
            Name = "Skin Treatments",
            Slug = "skin-treatments",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var hairCategory = new ServiceCategory
        {
            Name = "Hair Treatments",
            Slug = "hair-treatments",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var faceCategory = new ServiceCategory
        {
            Name = "Face Treatments",
            Slug = "face-treatments",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.ServiceCategories.AddRange(skinCategory, hairCategory, faceCategory);
        await context.SaveChangesAsync();

        var services = new List<ClinicService>
        {
            new()
            {
                ServiceCategoryId = skinCategory.Id,
                Name = "HydraFacial Treatment",
                Description = "Premium deep cleansing and hydration treatment for glowing skin.",
                DurationMinutes = 60,
                Price = 45,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                ServiceCategoryId = hairCategory.Id,
                Name = "Hair Nourishment",
                Description = "Medical-grade nourishment session to improve scalp and hair health.",
                DurationMinutes = 45,
                Price = 35,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                ServiceCategoryId = faceCategory.Id,
                Name = "Botox Consultation",
                Description = "Specialist consultation for facial enhancement and anti-aging care.",
                DurationMinutes = 30,
                Price = 25,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.ClinicServices.AddRange(services);
        await context.SaveChangesAsync();
    }
    private static async Task SeedDoctorAssignmentsAsync(
    AppDbContext context,
    UserManager<ApplicationUser> userManager)
    {
        var doctorUser = await userManager.FindByEmailAsync("doctor@batoclinic.com");

        if (doctorUser is null)
        {
            return;
        }

        var doctorProfile = await context.DoctorProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == doctorUser.Id);

        if (doctorProfile is null)
        {
            return;
        }

        var branch = await context.Branches.FirstOrDefaultAsync();

        if (branch is not null)
        {
            var hasDoctorBranch = await context.DoctorBranches
                .AnyAsync(item =>
                    item.DoctorProfileId == doctorProfile.Id &&
                    item.BranchId == branch.Id);

            if (!hasDoctorBranch)
            {
                context.DoctorBranches.Add(new DoctorBranch
                {
                    DoctorProfileId = doctorProfile.Id,
                    BranchId = branch.Id
                });
            }
        }

        var services = await context.ClinicServices.ToListAsync();

        foreach (var service in services)
        {
            var hasDoctorService = await context.DoctorServices
                .AnyAsync(item =>
                    item.DoctorProfileId == doctorProfile.Id &&
                    item.ClinicServiceId == service.Id);

            if (!hasDoctorService)
            {
                context.DoctorServices.Add(new DoctorService
                {
                    DoctorProfileId = doctorProfile.Id,
                    ClinicServiceId = service.Id
                });
            }
        }

        await context.SaveChangesAsync();
    }
}