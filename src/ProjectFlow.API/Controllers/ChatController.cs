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
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return Guid.Parse(userIdClaim?.Value ?? Guid.Empty.ToString());
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = GetCurrentUserId();
        var conversations = await _chatService.GetConversationsAsync(userId);
        return Ok(conversations);
    }

    [HttpGet("conversations/{id:guid}")]
    public async Task<IActionResult> GetConversation(Guid id)
    {
        var userId = GetCurrentUserId();
        var conversation = await _chatService.GetConversationAsync(id, userId);
        return Ok(conversation);
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDto dto)
    {
        var userId = GetCurrentUserId();
        var conversation = await _chatService.CreateConversationAsync(dto, userId);
        return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, conversation);
    }

    [HttpGet("conversations/{id:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var messages = await _chatService.GetMessagesAsync(id, page, pageSize);
        return Ok(messages);
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] CreateMessageDto dto)
    {
        var userId = GetCurrentUserId();
        var message = await _chatService.SendMessageAsync(dto, userId);
        return CreatedAtAction(nameof(GetMessages), new { id = dto.ConversationId }, message);
    }

    [HttpPost("conversations/{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = Guid.Parse(User.Identity.Name);
        await _chatService.MarkAsReadAsync(id, userId);
        return NoContent();
    }

    [HttpGet("direct/{otherUserId:guid}")]
    public async Task<IActionResult> GetDirectMessages(Guid otherUserId)
    {
        var userId = Guid.Parse(User.Identity.Name);
        var conversations = await _chatService.GetDirectMessagesAsync(userId, otherUserId);
        return Ok(conversations);
    }
}
