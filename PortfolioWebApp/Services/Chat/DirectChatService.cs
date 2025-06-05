using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using PortfolioWebApp.Components.Pages.Chats;
using PortfolioWebApp.Models;
using PortfolioWebApp.Models.Entities;
using PortfolioWebApp.Repositories;
using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services.Chat;

public class DirectChatService : IDirectChatService {

    private readonly ILogger<DirectChatService> _logger;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly DirectMessageRepository _directMessageRepository;
    private readonly UserService _userService;

    private int _progress;
    public event Action<int, string>? OnProgressChanged;
    
    private Dictionary<UserDto, List<DirectMessageDto>> _chats = new();
    
    
    public DirectChatService(
        ILogger<DirectChatService> logger, 
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

    
    public List<ChatPreview> GetChatPreviews(IDirectChatService.ChatPreviewFilter filter) {
        
        var previews = new List<ChatPreview>();

        foreach (var entry in _chats) {
            
            var chatPartner = entry.Key;
            var messages = entry.Value;
            var latestMessage = messages.FirstOrDefault();
            var authState = _authenticationStateProvider.GetAuthenticationStateAsync();
            var me = authState.Result.User.Identity?.Name;
            var unreadMessagesCount = messages.Count(m => m.To.username == me && m.Read == null);
            
            switch (filter) {
                
                case IDirectChatService.ChatPreviewFilter.All:
                    previews.Add(new ChatPreview(chatPartner, latestMessage, unreadMessagesCount));
                    break;

                case IDirectChatService.ChatPreviewFilter.Unread:
                    if (unreadMessagesCount > 0) {
                        previews.Add(new ChatPreview(chatPartner, latestMessage, unreadMessagesCount));
                    }
                    break;
                
                default:
                    _logger.LogError($"Unsupported Value for" + nameof(IDirectChatService.ChatPreviewFilter) + ": {filter}", filter);
                    break;
                
            }
        }
        return previews;
    }

    public KeyValuePair<UserDto, List<DirectMessageDto>> GetFullChatForChatPreview(ChatPreview chatPreview) {
        
        foreach (var chat in _chats) {
            
            if (chat.Key != chatPreview.ChatPartner) continue;
            
            // return copies to avoid external modification
            var userCopy = chat.Key with { };
            var messagesCopy = chat.Value
                .Select(message => message with { }).ToList();
                
            return new KeyValuePair<UserDto, List<DirectMessageDto>>(userCopy, messagesCopy);

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
    
}