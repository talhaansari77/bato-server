using BatoClinic.Api.Data;
using BatoClinic.Api.DTOs.Branches;
using BatoClinic.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BatoClinic.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly AppDbContext _context;

    public BranchesController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/branches
    // Returns all active and inactive branches, newest first.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BranchResponseDto>>> GetBranches()
    {
        var branches = await _context.Branches
            .OrderByDescending(branch => branch.CreatedAt)
            .Select(branch => new BranchResponseDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Phone = branch.Phone,
                Address = branch.Address,
                City = branch.City,
                Country = branch.Country,
                IsActive = branch.IsActive,
                CreatedAt = branch.CreatedAt
            })
            .ToListAsync();

        return Ok(branches);
    }

    // GET /api/branches/{id}
    // Returns one branch by id.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BranchResponseDto>> GetBranchById(Guid id)
    {
        var branch = await _context.Branches
            .Where(branch => branch.Id == id)
            .Select(branch => new BranchResponseDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Phone = branch.Phone,
                Address = branch.Address,
                City = branch.City,
                Country = branch.Country,
                IsActive = branch.IsActive,
                CreatedAt = branch.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (branch is null)
        {
            return NotFound(new
            {
                message = "Branch not found"
            });
        }

        return Ok(branch);
    }

    // POST /api/branches
    // Creates a new clinic branch.
    [HttpPost]
    public async Task<ActionResult<BranchResponseDto>> CreateBranch(CreateBranchDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Branch name is required"
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Address))
        {
            return BadRequest(new
            {
                message = "Branch address is required"
            });
        }

        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Phone = dto.Phone?.Trim(),
            Address = dto.Address.Trim(),
            City = dto.City?.Trim(),
            Country = dto.Country?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();

        var response = new BranchResponseDto
        {
            Id = branch.Id,
            Name = branch.Name,
            Phone = branch.Phone,
            Address = branch.Address,
            City = branch.City,
            Country = branch.Country,
            IsActive = branch.IsActive,
            CreatedAt = branch.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetBranchById),
            new { id = branch.Id },
            response
        );
    }

    // PATCH /api/branches/{id}
// Updates an existing clinic branch.
// PATCH is used because the admin may update only some fields.
[HttpPatch("{id:guid}")]
public async Task<ActionResult<BranchResponseDto>> UpdateBranch(Guid id, UpdateBranchDto dto)
{
    var branch = await _context.Branches.FirstOrDefaultAsync(branch => branch.Id == id);

    if (branch is null)
    {
        return NotFound(new
        {
            message = "Branch not found"
        });
    }

    if (!string.IsNullOrWhiteSpace(dto.Name))
    {
        branch.Name = dto.Name.Trim();
    }

    if (dto.Phone is not null)
    {
        branch.Phone = dto.Phone.Trim();
    }

    if (!string.IsNullOrWhiteSpace(dto.Address))
    {
        branch.Address = dto.Address.Trim();
    }

    if (dto.City is not null)
    {
        branch.City = dto.City.Trim();
    }

    if (dto.Country is not null)
    {
        branch.Country = dto.Country.Trim();
    }

    if (dto.IsActive.HasValue)
    {
        branch.IsActive = dto.IsActive.Value;
    }

    await _context.SaveChangesAsync();

    var response = new BranchResponseDto
    {
        Id = branch.Id,
        Name = branch.Name,
        Phone = branch.Phone,
        Address = branch.Address,
        City = branch.City,
        Country = branch.Country,
        IsActive = branch.IsActive,
        CreatedAt = branch.CreatedAt
    };

    return Ok(response);
}

// DELETE /api/branches/{id}
// Soft deletes a branch by setting IsActive to false.
// We do not remove it from the database because old appointments may still need branch history.
[HttpDelete("{id:guid}")]
public async Task<ActionResult> DeactivateBranch(Guid id)
{
    var branch = await _context.Branches.FirstOrDefaultAsync(branch => branch.Id == id);

    if (branch is null)
    {
        return NotFound(new
        {
            message = "Branch not found"
        });
    }

    branch.IsActive = false;
    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Branch deactivated successfully"
    });
}
}

