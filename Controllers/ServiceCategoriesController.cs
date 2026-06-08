using BatoClinic.Api.Data;
using BatoClinic.Api.DTOs.ServiceCategories;
using BatoClinic.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BatoClinic.Api.Controllers;

[ApiController]
[Route("api/service-categories")]
public class ServiceCategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ServiceCategoriesController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/service-categories
    // Returns all active service categories.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceCategoryResponseDto>>> GetCategories()
    {
        var categories = await _context.ServiceCategories
            .Where(category => category.IsActive)
            .OrderBy(category => category.Name)
            .Select(category => new ServiceCategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                ImageUrl = category.ImageUrl,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            })
            .ToListAsync();

        return Ok(categories);
    }

    // POST /api/service-categories
    // Creates a new category like Hair, Skin, or Face treatments.
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ServiceCategoryResponseDto>> CreateCategory(CreateServiceCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { message = "Category name is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.Slug))
        {
            return BadRequest(new { message = "Category slug is required" });
        }

        var slugExists = await _context.ServiceCategories
            .AnyAsync(category => category.Slug == dto.Slug.Trim().ToLower());

        if (slugExists)
        {
            return BadRequest(new { message = "Category slug already exists" });
        }

        var category = new ServiceCategory
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Slug = dto.Slug.Trim().ToLower(),
            ImageUrl = dto.ImageUrl?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ServiceCategories.Add(category);
        await _context.SaveChangesAsync();

        var response = new ServiceCategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            ImageUrl = category.ImageUrl,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt
        };

        return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, response);
    }
}