using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ProjectFlow.API.Hubs;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, Guid> _connections = new();
    private static readonly ConcurrentDictionary<Guid, List<string>> _userConnections = new();

    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }

    public Task RegisterUser(Guid userId)
    {
        _connections[Context.ConnectionId] = userId;
        
        if (!_userConnections.ContainsKey(userId))
            _userConnections[userId] = new List<string>();
        
        _userConnections[userId].Add(Context.ConnectionId);
        
        return Task.CompletedTask;
    }

    public async Task SendMessage(Guid conversationId, Guid senderId, string senderName, string content, Guid? replyToId = null)
    {
        await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", new
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = senderId,
            SenderName = senderName,
            Content = content,
            ReplyToId = replyToId,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task SendTyping(Guid conversationId, Guid userId, string userName)
    {
        await Clients.OthersInGroup(conversationId.ToString()).SendAsync("UserTyping", new
        {
            UserId = userId,
            UserName = userName
        });
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connections.TryRemove(Context.ConnectionId, out var userId))
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(Context.ConnectionId);
                if (connections.Count == 0)
                    _userConnections.TryRemove(userId, out _);
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
