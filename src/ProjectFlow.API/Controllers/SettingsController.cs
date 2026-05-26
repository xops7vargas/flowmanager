using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Application.DTOs;
using ProjectFlow.Application.Interfaces;

namespace ProjectFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var settings = await _settingsService.GetAllAsync();
        return Ok(settings);
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        var setting = await _settingsService.GetByKeyAsync(key);
        return Ok(setting);
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateSystemSettingDto dto)
    {
        var setting = await _settingsService.UpdateAsync(key, dto);
        return Ok(setting);
    }

    [HttpPost("initialize")]
    public async Task<IActionResult> Initialize()
    {
        await _settingsService.InitializeDefaultsAsync();
        return NoContent();
    }
}
