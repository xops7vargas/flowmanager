using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Application.DTOs;
using ProjectFlow.Application.Interfaces;

namespace ProjectFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAnalytics(
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var analytics = await _analyticsService.GetAnalyticsAsync(userId, startDate, endDate);
        return Ok(analytics);
    }

    [HttpGet("compliance")]
    public async Task<IActionResult> GetComplianceMetrics(
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var metrics = await _analyticsService.GetComplianceMetricsAsync(userId, startDate, endDate);
        return Ok(metrics);
    }

    [HttpGet("user-performance")]
    public async Task<IActionResult> GetUserPerformance(
        [FromQuery] Guid? projectId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var performance = await _analyticsService.GetUserPerformanceAsync(projectId, startDate, endDate);
        return Ok(performance);
    }

    [HttpGet("project-metrics")]
    public async Task<IActionResult> GetProjectMetrics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var metrics = await _analyticsService.GetProjectMetricsAsync(startDate, endDate);
        return Ok(metrics);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyData([FromQuery] int months = 12)
    {
        var data = await _analyticsService.GetMonthlyDataAsync(months);
        return Ok(data);
    }
}
