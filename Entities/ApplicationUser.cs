using Microsoft.AspNetCore.Identity;

namespace BatoClinic.Api.Entities;

// ApplicationUser is our custom user model.
// IdentityUser already gives us Id, Email, PasswordHash, PhoneNumber, etc.
// We add app-specific fields like FullName and RoleType.
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public string RoleType { get; set; } = "Patient";

    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}