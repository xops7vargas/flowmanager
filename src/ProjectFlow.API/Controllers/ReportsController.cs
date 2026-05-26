using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Application.Interfaces;

namespace ProjectFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public ReportsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetProjectReport(Guid projectId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var report = await _analyticsService.GetProjectReportAsync(projectId, startDate, endDate);
        return Ok(report);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserReport(Guid userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var report = await _analyticsService.GetUserReportAsync(userId, startDate, endDate);
        return Ok(report);
    }

    [HttpGet("financial")]
    public async Task<IActionResult> GetFinancialReport([FromQuery] Guid? projectId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var report = await _analyticsService.GetFinancialReportAsync(projectId, startDate, endDate);
        return Ok(report);
    }

    [HttpGet("export/project/{projectId}")]
    public async Task<IActionResult> ExportProjectReport(Guid projectId, [FromQuery] string format = "pdf")
    {
        var report = await _analyticsService.GetProjectReportAsync(projectId, null, null);
        return Ok(new { format, data = report, message = "Reporte exportado correctamente" });
    }
}
