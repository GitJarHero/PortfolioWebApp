using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using PortfolioWebApp.Repositories;
using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services.Chat;

public class DirectChatStorageService : IDirectChatStorageService {

    private readonly ILogger<DirectChatStorageService> _logger;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly DirectMessageRepository _directMessageRepository;
    private readonly UserService _userService;

    private int _progress;
  
    public event Action<int, string>? OnProgressChanged;
    
    private Dictionary<UserDto, List<DirectMessageDto>> _chats = new();
    
    // set when clicking on the chat icon in the FriendsList in Friends.razor
    // unset when the chat is being shown
    public static string? ShowChatForUser = null;
    
    public DirectChatStorageService(
        ILogger<DirectChatStorageService> logger, 
        AuthenticationStateProvider authenticationStateProvider,
        DirectMessageRepository directMessageRepository,
        UserService userService
    ) {
        _logger = logger;
        _logger.LogInformation("ChatLoadingService initialized");
        _authenticationStateProvider = authenticationStateProvider;
        _directMessageRepository = directMessageRepository;
        _userService = userService;
    }

    
    public List<ChatPreviewDto> GetChatPreviews(IDirectChatStorageService.ChatPreviewFilter filter) {
        
        var authState = _authenticationStateProvider.GetAuthenticationStateAsync();
        var me = authState.Result.User.Identity?.Name;
        var previews = new List<ChatPreviewDto>();

        foreach (var entry in _chats) {
            
            var chatPartner = entry.Key;
            var messages = entry.Value;
            var latestMessage = messages.LastOrDefault();
            var unreadMessagesCount = messages.Count(m => m.To.username == me && m.Read == null);
            
            switch (filter) {
                
                case IDirectChatStorageService.ChatPreviewFilter.All:
                    previews.Add(new ChatPreviewDto(chatPartner, latestMessage, unreadMessagesCount));
                    break;

                case IDirectChatStorageService.ChatPreviewFilter.Unread:
                    if (unreadMessagesCount > 0) {
                        previews.Add(new ChatPreviewDto(chatPartner, latestMessage, unreadMessagesCount));
                    }
                    break;
                
                default:
                    _logger.LogError($"Unsupported Value for" + nameof(IDirectChatStorageService.ChatPreviewFilter) + ": {filter}", filter);
                    break;
                
            }
        }
        return previews
            .OrderBy(pv =>
            {
                var msg = pv.LatestMessage;
                if (msg == null) return DateTime.MinValue;

                return msg.From.username == me
                    ? msg.Created
                    : msg.Delivered ?? msg.Created; // fallback if Delivered is null
            })
            .ToList();

    }

    public KeyValuePair<UserDto, List<DirectMessageDto>> GetFullChatForChatPreview(ChatPreviewDto chatPreviewDto) 
    {
        foreach (var chat in _chats) {
            if (chat.Key != chatPreviewDto.ChatPartner) continue;
            
            return new KeyValuePair<UserDto, List<DirectMessageDto>>(chat.Key, chat.Value);
        }

        throw new KeyNotFoundException("Chat not found for the provided ChatPreview.");
    }

    
    public async Task LoadChatsAsync(string myUsername) {
        
        OnProgressChanged?.Invoke(_progress, "Loading chats...");
        
        var me = _userService.FindUserByName(myUsername);
        var messages = await _directMessageRepository.GetAllByUserAsync(me.Id);
        
        _progress = 50;
        OnProgressChanged?.Invoke(_progress, "Loading chats...");
        
        var chatDict = new Dictionary<UserDto, List<DirectMessageDto>>();  
        
        var groupedMessages = messages
            .GroupBy(m => m.FromUser.Id == me.Id ? m.ToUser : m.FromUser)
            .ToList();

        foreach (var group in groupedMessages) {
            var chatPartner = group.Key;
            var chatPartnerDto = new UserDto(chatPartner.UserName, chatPartner.Id, chatPartner.ProfileColor);

            var messageList = new List<DirectMessageDto>();

            foreach (var message in group) {
                var messageDto = new DirectMessageDto(
                    message.Id,
                    new UserDto(message.FromUser.UserName, message.FromUser.Id, message.FromUser.ProfileColor),
                    new UserDto(message.ToUser.UserName, message.ToUser.Id, message.ToUser.ProfileColor),
                    message.Content,
                    message.Created,
                    message.Read,
                    message.Delivered
                );
                messageList.Add(messageDto);
            }
            
            var sortedMessages = messageList
                .OrderBy(m => m.Created)
                .ToList();

            chatDict[chatPartnerDto] = sortedMessages;
        }
        
        _chats = chatDict;
        
        
        
        _progress = 100;
        OnProgressChanged?.Invoke(100, "Done");
    }

    public bool ChatsLoaded() {
        return _progress == 100;
    }

    public async Task<List<MessageDeliveredDto>> MarkNewMessagesAsDeliveredAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var currentUserId))
        {
            return new List<MessageDeliveredDto>();
        }

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        // Dictionary<fromUserId, List<messageId>>
        var groupedMessageIds = new Dictionary<int, List<int>>();

        foreach (var (_, messages) in _chats)
        {
            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];

                if (message.To.id == currentUserId && message.Delivered == null)
                {
                    var deliveredMessage = message with { Delivered = now };
                    messages[i] = deliveredMessage;

                    if (!groupedMessageIds.ContainsKey(message.From.id))
                    {
                        groupedMessageIds[message.From.id] = new List<int>();
                    }

                    groupedMessageIds[message.From.id].Add(message.MessageId);
                }
            }
        }

        // Jetzt pro Absender ein MessageDeliveredDto erzeugen
        var result = groupedMessageIds
            .Select(group => new MessageDeliveredDto(group.Value, now))
            .ToList();

        return result;
    }

    public List<int> MarkUnreadMessagesAsRead(int fromUserId, DateTime readDate) {
        List<int> messagesMarked = [];

        var messages = _chats.First(userChat => userChat.Key.id == fromUserId).Value;

        for (int i = 0; i < messages.Count; i++) {
            var message = messages[i];
            
            if (message.From.id == fromUserId && message.Read == null) {
                var updatedMessage = message with { Read = readDate };
                messages[i] = updatedMessage;
                messagesMarked.Add(updatedMessage.MessageId);
            }
        }

        return messagesMarked;
    }


    
    public void InvalidateStorage() {
        _chats.Clear();
        _progress = 0;
        _logger.LogInformation("Chat Storage invalidated");
    }
    
    public void HandleReceiveMessage(DirectMessageDto message)
    {
        var chatPartner = message.From;

        if (!_chats.TryGetValue(chatPartner, out var messages))
        {
            // if no chat with that chat partner exists, someone started a new chat with us
            // --> add a new chat with that message
            _chats[chatPartner] = new List<DirectMessageDto> { message }; 
        }
        else
        {
            messages.Add(message);
            messages.Sort((a, b) => a.Created.CompareTo(b.Created));
        }
    }
    
    public void HandleMessageSentAcknowledgement(DirectMessageDto message) 
    {
        var chatPartner = message.To;
        if (!_chats.TryGetValue(chatPartner, out var messages)) 
        {
            // if no chat with that chat partner exists, we started a new chat with someone
            // --> add a new chat with that message
            _chats[chatPartner] = new List<DirectMessageDto> { message };
        }
        else {
            // else when a chat with that chat partner exists:
            messages.Add(message);
            messages.Sort((a, b) => a.Created.CompareTo(b.Created));
        }
    }

    public void HandleMessageDelivered(MessageDeliveredDto message) 
    {
        foreach (var chat in _chats.Values) 
        {
            for (int i = 0; i < chat.Count; i++) 
            {
                var msg = chat[i];
                if (message.DeliveredMessages.Contains(msg.MessageId)) 
                {
                    var updated = msg with { Delivered = message.TimeStamp };
                    chat[i] = updated;
                }
            }
        }
    }

    public void HandleMessageRead(MessageReadDto message)
    {
        foreach (var chat in _chats.Values)
        {
            for (int i = 0; i < chat.Count; i++)
            {
                var msg = chat[i];
                if (message.ReadMessages.Contains(msg.MessageId))
                {
                    var updated = msg with { Read = message.TimeStamp };
                    chat[i] = updated;
                    _logger.LogInformation($"Message {msg.MessageId} marked as read at {message.TimeStamp}");
                }
            }
        }
    }

    
}