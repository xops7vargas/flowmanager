using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Application.DTOs;
using ProjectFlow.Application.Interfaces;

namespace ProjectFlow.API.Controllers;

[ApiController]
[Route("api/user-roles")]
[Authorize]
public class UserRolesController : ControllerBase
{
    private readonly IUserService _userService;

    public UserRolesController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("Test works");
    }

    [HttpPut("{userId:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<UserDto>> UpdateUserRole(Guid userId, [FromBody] UpdateUserRoleDto dto)
    {
        try
        {
            var user = await _userService.UpdateUserRoleAsync(userId, dto.RoleId);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
