using BatoClinic.Api.Data;
using BatoClinic.Api.DTOs.Services;
using BatoClinic.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BatoClinic.Api.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ServicesController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/services
    // Returns all active clinic services with their category name.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClinicServiceResponseDto>>> GetServices()
    {
        var services = await _context.ClinicServices
            .Where(service => service.IsActive)
            .Include(service => service.ServiceCategory)
            .OrderBy(service => service.Name)
            .Select(service => new ClinicServiceResponseDto
            {
                Id = service.Id,
                ServiceCategoryId = service.ServiceCategoryId,
                CategoryName = service.ServiceCategory != null ? service.ServiceCategory.Name : string.Empty,
                Name = service.Name,
                Description = service.Description,
                DurationMinutes = service.DurationMinutes,
                Price = service.Price,
                ImageUrl = service.ImageUrl,
                IsActive = service.IsActive,
                CreatedAt = service.CreatedAt
            })
            .ToListAsync();

        return Ok(services);
    }

    // GET /api/services/{id}
    // Returns one service by id.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClinicServiceResponseDto>> GetServiceById(Guid id)
    {
        var service = await _context.ClinicServices
            .Where(service => service.Id == id)
            .Include(service => service.ServiceCategory)
            .Select(service => new ClinicServiceResponseDto
            {
                Id = service.Id,
                ServiceCategoryId = service.ServiceCategoryId,
                CategoryName = service.ServiceCategory != null ? service.ServiceCategory.Name : string.Empty,
                Name = service.Name,
                Description = service.Description,
                DurationMinutes = service.DurationMinutes,
                Price = service.Price,
                ImageUrl = service.ImageUrl,
                IsActive = service.IsActive,
                CreatedAt = service.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (service is null)
        {
            return NotFound(new { message = "Service not found" });
        }

        return Ok(service);
    }

    // POST /api/services
    // Creates a new clinic service/treatment.
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ClinicServiceResponseDto>> CreateService(CreateClinicServiceDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { message = "Service name is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            return BadRequest(new { message = "Service description is required" });
        }

        if (dto.DurationMinutes <= 0)
        {
            return BadRequest(new { message = "Duration must be greater than 0" });
        }

        if (dto.Price < 0)
        {
            return BadRequest(new { message = "Price cannot be negative" });
        }

        var category = await _context.ServiceCategories
            .FirstOrDefaultAsync(category => category.Id == dto.ServiceCategoryId && category.IsActive);

        if (category is null)
        {
            return BadRequest(new { message = "Valid service category is required" });
        }

        var service = new ClinicService
        {
            Id = Guid.NewGuid(),
            ServiceCategoryId = dto.ServiceCategoryId,
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            DurationMinutes = dto.DurationMinutes,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ClinicServices.Add(service);
        await _context.SaveChangesAsync();

        var response = new ClinicServiceResponseDto
        {
            Id = service.Id,
            ServiceCategoryId = service.ServiceCategoryId,
            CategoryName = category.Name,
            Name = service.Name,
            Description = service.Description,
            DurationMinutes = service.DurationMinutes,
            Price = service.Price,
            ImageUrl = service.ImageUrl,
            IsActive = service.IsActive,
            CreatedAt = service.CreatedAt
        };

        return CreatedAtAction(nameof(GetServiceById), new { id = service.Id }, response);
    }

    // PATCH /api/services/{id}
    // Updates only the service fields sent by the client.
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ClinicServiceResponseDto>> UpdateService(Guid id, UpdateClinicServiceDto dto)
    {
        var service = await _context.ClinicServices
            .Include(service => service.ServiceCategory)
            .FirstOrDefaultAsync(service => service.Id == id);

        if (service is null)
        {
            return NotFound(new { message = "Service not found" });
        }

        if (dto.ServiceCategoryId.HasValue)
        {
            var category = await _context.ServiceCategories
                .FirstOrDefaultAsync(category => category.Id == dto.ServiceCategoryId.Value && category.IsActive);

            if (category is null)
            {
                return BadRequest(new { message = "Valid service category is required" });
            }

            service.ServiceCategoryId = dto.ServiceCategoryId.Value;
            service.ServiceCategory = category;
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            service.Name = dto.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            service.Description = dto.Description.Trim();
        }

        if (dto.DurationMinutes.HasValue)
        {
            if (dto.DurationMinutes.Value <= 0)
            {
                return BadRequest(new { message = "Duration must be greater than 0" });
            }

            service.DurationMinutes = dto.DurationMinutes.Value;
        }

        if (dto.Price.HasValue)
        {
            if (dto.Price.Value < 0)
            {
                return BadRequest(new { message = "Price cannot be negative" });
            }

            service.Price = dto.Price.Value;
        }

        if (dto.ImageUrl is not null)
        {
            service.ImageUrl = dto.ImageUrl.Trim();
        }

        if (dto.IsActive.HasValue)
        {
            service.IsActive = dto.IsActive.Value;
        }

        await _context.SaveChangesAsync();

        var response = new ClinicServiceResponseDto
        {
            Id = service.Id,
            ServiceCategoryId = service.ServiceCategoryId,
            CategoryName = service.ServiceCategory?.Name ?? string.Empty,
            Name = service.Name,
            Description = service.Description,
            DurationMinutes = service.DurationMinutes,
            Price = service.Price,
            ImageUrl = service.ImageUrl,
            IsActive = service.IsActive,
            CreatedAt = service.CreatedAt
        };

        return Ok(response);
    }

    // DELETE /api/services/{id}
    // Soft delete: marks service inactive instead of removing it from database.
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeactivateService(Guid id)
    {
        var service = await _context.ClinicServices.FirstOrDefaultAsync(service => service.Id == id);

        if (service is null)
        {
            return NotFound(new { message = "Service not found" });
        }

        service.IsActive = false;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Service deactivated successfully" });
    }
}