using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Application.DTOs;
using ProjectFlow.Application.Interfaces;
using ProjectFlow.Domain.Enums;
using TaskStatus = ProjectFlow.Domain.Enums.TaskStatus;

namespace ProjectFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResultDto<ProjectDto>>> GetAll(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        [FromQuery] ProjectStatus? status = null)
    {
        var userId = GetCurrentUserId();
        var result = await _projectService.GetAllAsync(page, pageSize, status, userId);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id)
    {
        try
        {
            var project = await _projectService.GetByIdAsync(id);
            return Ok(project);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectDto dto)
    {
        var ownerId = GetUserId();
        var project = await _projectService.CreateAsync(dto, ownerId);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<ActionResult<ProjectDto>> Update(Guid id, [FromBody] UpdateProjectDto dto)
    {
        try
        {
            var project = await _projectService.UpdateAsync(id, dto);
            return Ok(project);
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
            await _projectService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id}/members")]
    public async Task<ActionResult<List<ProjectMemberDto>>> GetMembers(Guid id)
    {
        try
        {
            var members = await _projectService.GetMembersAsync(id);
            return Ok(members);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id}/members")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberDto dto)
    {
        try
        {
            await _projectService.AddMemberAsync(id, dto.UserId, dto.Role);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}/members/{userId}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        try
        {
            await _projectService.RemoveMemberAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{id}/progress")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> UpdateProgress(Guid id)
    {
        await _projectService.UpdateProgressAsync(id);
        return NoContent();
    }

    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return Guid.Parse(userIdClaim?.Value ?? Guid.Empty.ToString());
    }
}

public class AddMemberDto
{
    public Guid UserId { get; set; }
    public string? Role { get; set; }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResultDto<TaskDto>>> GetAll(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? projectId = null,
        [FromQuery] TaskStatus? status = null,
        [FromQuery] Guid? assignedToId = null)
    {
        var userId = GetCurrentUserId();
        var result = await _taskService.GetAllAsync(page, pageSize, projectId, status, assignedToId, userId);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetById(Guid id)
    {
        try
        {
            var task = await _taskService.GetByIdAsync(id);
            return Ok(task);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager,Developer,Programmer")]
    public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskDto dto)
    {
        var createdById = GetUserId();
        var task = await _taskService.CreateAsync(dto, createdById);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,ProjectManager,Developer,Programmer,QA")]
    public async Task<ActionResult<TaskDto>> Update(Guid id, [FromBody] UpdateTaskDto dto)
    {
        try
        {
            var task = await _taskService.UpdateAsync(id, dto);
            return Ok(task);
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
            await _taskService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Administrator,ProjectManager,Developer,Programmer,QA")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusDto dto)
    {
        try
        {
            var userId = GetUserId();
            await _taskService.UpdateStatusAsync(id, dto.Status, userId);
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

    [HttpGet("{id}/subtasks")]
    public async Task<ActionResult<List<TaskDto>>> GetSubtasks(Guid id)
    {
        try
        {
            var subtasks = await _taskService.GetSubtasksAsync(id);
            return Ok(subtasks);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("overdue/count")]
    public async Task<ActionResult<int>> GetOverdueCount()
    {
        var userId = GetUserId();
        var count = await _taskService.GetOverdueCountAsync(userId);
        return Ok(count);
    }

    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return Guid.Parse(userIdClaim?.Value ?? Guid.Empty.ToString());
    }
}

public class UpdateTaskStatusDto
{
    public TaskStatus Status { get; set; }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimeEntriesController : ControllerBase
{
    private readonly ITimeEntryService _timeEntryService;

    public TimeEntriesController(ITimeEntryService timeEntryService)
    {
        _timeEntryService = timeEntryService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResultDto<TimeEntryDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? taskId = null,
        [FromQuery] Guid? userId = null)
    {
        var result = await _timeEntryService.GetAllAsync(page, pageSize, taskId, userId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TimeEntryDto>> GetById(Guid id)
    {
        try
        {
            var entry = await _timeEntryService.GetByIdAsync(id);
            return Ok(entry);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<TimeEntryDto>> Create([FromBody] CreateTimeEntryDto dto)
    {
        var userId = GetUserId();
        var entry = await _timeEntryService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TimeEntryDto>> Update(Guid id, [FromBody] UpdateTimeEntryDto dto)
    {
        try
        {
            var userId = GetUserId();
            var entry = await _timeEntryService.UpdateAsync(id, dto, userId);
            return Ok(entry);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _timeEntryService.DeleteAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
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
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("task/{taskId}")]
    public async Task<ActionResult<List<CommentDto>>> GetByTask(Guid taskId)
    {
        var comments = await _commentService.GetByTaskAsync(taskId);
        return Ok(comments);
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> Create([FromBody] CreateCommentDto dto)
    {
        var userId = GetUserId();
        var comment = await _commentService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetByTask), new { taskId = dto.TaskId }, comment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _commentService.DeleteAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return Guid.Parse(userIdClaim?.Value ?? Guid.Empty.ToString());
    }
}