using System;
using System.Collections.Generic;
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
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetAll([FromQuery] bool unreadOnly = false)
    {
        var userId = GetUserId();
        var notifications = await _notificationService.GetByUserAsync(userId, unreadOnly);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(count);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _notificationService.MarkAsReadAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetUserId();
        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _notificationService.DeleteAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return Guid.Parse(userIdClaim?.Value ?? Guid.Empty.ToString());
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetAll()
    {
        var tags = await _tagService.GetAllAsync();
        return Ok(tags);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<ActionResult<TagDto>> Create([FromBody] CreateTagDto dto)
    {
        try
        {
            var tag = await _tagService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetAll), null, tag);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<ActionResult<TagDto>> Update(Guid id, [FromBody] CreateTagDto dto)
    {
        try
        {
            var tag = await _tagService.UpdateAsync(id, dto);
            return Ok(tag);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _tagService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _workflowService;

    public WorkflowsController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<WorkflowDto>>> GetByProject(Guid projectId)
    {
        var workflows = await _workflowService.GetByProjectAsync(projectId);
        return Ok(workflows);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkflowDto>> GetById(Guid id)
    {
        try
        {
            var workflow = await _workflowService.GetByIdAsync(id);
            return Ok(workflow);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<ActionResult<WorkflowDto>> Create([FromBody] CreateWorkflowDto dto)
    {
        var workflow = await _workflowService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = workflow.Id }, workflow);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<ActionResult<WorkflowDto>> Update(Guid id, [FromBody] CreateWorkflowDto dto)
    {
        try
        {
            var workflow = await _workflowService.UpdateAsync(id, dto);
            return Ok(workflow);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _workflowService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/transitions")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> AddTransition(Guid id, [FromBody] CreateWorkflowTransitionDto dto)
    {
        try
        {
            await _workflowService.AddTransitionAsync(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("transitions/{id}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> RemoveTransition(Guid id)
    {
        try
        {
            await _workflowService.RemoveTransitionAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DelaysController : ControllerBase
{
    private readonly IDelayService _delayService;

    public DelaysController(IDelayService delayService)
    {
        _delayService = delayService;
    }

    [HttpGet("task/{taskId}")]
    public async Task<ActionResult<List<DelayDto>>> GetByTask(Guid taskId)
    {
        var delays = await _delayService.GetByTaskAsync(taskId);
        return Ok(delays);
    }

    [HttpGet]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<ActionResult<List<DelayDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DelayCategory? category = null)
    {
        var delays = await _delayService.GetAllAsync(page, pageSize, category);
        return Ok(delays);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager,Developer,Programmer")]
    public async Task<ActionResult<DelayDto>> Create([FromBody] CreateDelayDto dto)
    {
        var userId = GetUserId();
        var delay = await _delayService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetByTask), new { taskId = dto.TaskId }, delay);
    }

    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return Guid.Parse(userIdClaim?.Value ?? Guid.Empty.ToString());
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get()
    {
        var userId = GetUserId();
        var dashboard = await _dashboardService.GetDashboardAsync(userId);
        return Ok(dashboard);
    }

    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return Guid.Parse(userIdClaim?.Value ?? Guid.Empty.ToString());
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;

    public CalendarController(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    [HttpGet("events")]
    public async Task<ActionResult<List<CalendarEventDto>>> GetEvents(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end)
    {
        var userId = GetUserId();
        var events = await _calendarService.GetEventsAsync(userId, start, end);
        return Ok(events);
    }

    [HttpPut("tasks/{id}/move")]
    public async Task<IActionResult> MoveTask(Guid id, [FromBody] MoveTaskDto dto)
    {
        try
        {
            var userId = GetUserId();
            await _calendarService.MoveTaskAsync(id, dto.NewStartDate, dto.NewEndDate, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return Guid.Parse(userIdClaim?.Value ?? Guid.Empty.ToString());
    }
}

public class MoveTaskDto
{
    public DateTime NewStartDate { get; set; }
    public DateTime NewEndDate { get; set; }
}