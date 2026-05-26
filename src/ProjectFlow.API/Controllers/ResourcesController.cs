using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Application.DTOs;
using ProjectFlow.Application.Interfaces;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _resourceService;

    public ResourcesController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ResourceType? type = null,
        [FromQuery] ResourceStatus? status = null,
        [FromQuery] string? search = null)
    {
        var result = await _resourceService.GetAllAsync(page, pageSize, type, status, search);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var resource = await _resourceService.GetByIdAsync(id);
        return Ok(resource);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateResourceDto dto)
    {
        var resource = await _resourceService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = resource.Id }, resource);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateResourceDto dto)
    {
        var resource = await _resourceService.UpdateAsync(id, dto);
        return Ok(resource);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _resourceService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("movements")]
    public async Task<IActionResult> CreateMovement([FromBody] CreateResourceMovementDto dto)
    {
        var userId = Guid.Parse(User.Identity.Name);
        var movement = await _resourceService.CreateMovementAsync(dto, userId);
        return CreatedAtAction(nameof(GetMovements), new { resourceId = dto.ResourceId }, movement);
    }

    [HttpGet("{resourceId:guid}/movements")]
    public async Task<IActionResult> GetMovements(Guid resourceId)
    {
        var movements = await _resourceService.GetMovementsAsync(resourceId);
        return Ok(movements);
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> AssignToUser(Guid id, [FromQuery] Guid userId)
    {
        await _resourceService.AssignToUserAsync(id, userId);
        return NoContent();
    }

    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> ReturnFromUser(Guid id)
    {
        await _resourceService.ReturnFromUserAsync(id);
        return NoContent();
    }
}
