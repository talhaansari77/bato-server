using BatoClinic.Api.DTOs.Auth;
using BatoClinic.Api.Entities;
using BatoClinic.Api.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;


namespace BatoClinic.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    // UserManager is from ASP.NET Core Identity.
    // It handles creating users, checking passwords, assigning roles, etc.
    public AuthController(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            return BadRequest(new { message = "Full name is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest(new { message = "Email is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "Password is required" });
        }

        var allowedRoles = new[] { "Patient", "Doctor", "Admin" };

        if (!allowedRoles.Contains(dto.Role))
        {
            return BadRequest(new { message = "Invalid role" });
        }

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);

        if (existingUser is not null)
        {
            return BadRequest(new { message = "User already exists with this email" });
        }

        var user = new ApplicationUser
        {
            FullName = dto.FullName.Trim(),
            UserName = dto.Email.Trim().ToLower(),
            Email = dto.Email.Trim().ToLower(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            RoleType = dto.Role,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // CreateAsync creates the user and securely hashes the password.
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Registration failed",
                errors = result.Errors.Select(error => error.Description)
            });
        }

        await _userManager.AddToRoleAsync(user, dto.Role);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);

        return Ok(new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = user.RoleType,
            Token = token
        });
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "Email and password are required" });
        }

        var user = await _userManager.FindByEmailAsync(dto.Email.Trim().ToLower());

        if (user is null)
        {
            return Unauthorized(new { message = "Invalid login credentials" });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { message = "Account is disabled" });
        }

        // CheckPasswordAsync compares plain password with secure Identity password hash.
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);

        if (!isPasswordValid)
        {
            return Unauthorized(new { message = "Invalid login credentials" });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);

        return Ok(new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = user.RoleType,
            Token = token
        });
    }
    // GET /api/auth/me
    // Protected endpoint.
    // The mobile app sends JWT token, and this returns the logged-in user profile.
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("/api/auth/me")]
    public async Task<ActionResult<MeResponseDto>> GetMe()
    {
        // Try to read user id from the standard .NET claim first.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Fallback: try to read user id from JWT "sub" claim.
        if (string.IsNullOrWhiteSpace(userId))
        {
            userId = User.FindFirstValue("sub");
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                message = "Invalid token: user id claim is missing",
                claims = User.Claims.Select(claim => new
                {
                    claim.Type,
                    claim.Value
                })
            });
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return NotFound(new
            {
                message = "User not found",
                userIdFromToken = userId,
                claims = User.Claims.Select(claim => new
                {
                    claim.Type,
                    claim.Value
                })
            });
        }

        return Ok(new MeResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Role = user.RoleType,
            AvatarUrl = user.AvatarUrl,
            IsActive = user.IsActive
        });
    }
    // GET /api/auth/ping
    // Simple test endpoint to confirm AuthController is reachable.
    [HttpGet("ping")]
    public ActionResult Ping()
    {
        return Ok(new
        {
            message = "Auth controller is working"
        });
    }
}

