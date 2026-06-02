namespace BatoClinic.Api.Configuration;

// This class represents the "Jwt" section from appsettings.json.
// We bind appsettings values into this class in Program.cs.
public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;
}