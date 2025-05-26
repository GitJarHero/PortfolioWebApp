using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using PortfolioWebApp.Components.Pages.Chats;
using PortfolioWebApp.Models;
using PortfolioWebApp.Models.Entities;
using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services.Chat;

public interface IDirectChatService {
    
    public enum ChatPreviewFilter {
        All,
        Unread,
        Favorites,
        Groups
    }
    
    public Task LoadChatsAsync(string user);

    public bool ChatsLoaded();
    
    public event Action<int,string>? OnProgressChanged;

    public List<ChatPreview> GetChatPreviews(ChatPreviewFilter filter);

    public KeyValuePair<UserDto, List<DirectMessageDto>> GetFullChatForChatPreview(ChatPreview chatPreview);
    
}

public class DirectChatService : IDirectChatService {

    private readonly ILogger<DirectChatService> _logger;
    private readonly AppDbContext _appDbContext;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    private int progress = 0;
    public event Action<int, string>? OnProgressChanged;
    
    private Dictionary<UserDto, List<DirectMessageDto>> _chats = new();
    
    
    public DirectChatService(
        ILogger<DirectChatService> logger, 
        AppDbContext appDbContext,
        AuthenticationStateProvider authenticationStateProvider
    ) {
        _logger = logger;
        _logger.LogInformation("ChatLoadingService initialized");
        _appDbContext = appDbContext;
        _authenticationStateProvider = authenticationStateProvider;
    }

    
    public List<ChatPreview> GetChatPreviews(IDirectChatService.ChatPreviewFilter filter) {
        
        var previews = new List<ChatPreview>();

        foreach (var entry in _chats) {
            
            var chatPartner = entry.Key;
            var messages = entry.Value;
            var latestMessage = messages.FirstOrDefault();
            var authState = _authenticationStateProvider.GetAuthenticationStateAsync();
            var me = authState.Result.User.Identity?.Name;
            var unreadMessagesCount = messages.Count(m => m.To == me && m.Read == null);
            
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
            
            if (chat.Key.Equals(chatPreview.ChatPartner)) {
                
                // return copies to avoid external modification
                var userCopy = chat.Key with { };
                var messagesCopy = chat.Value
                    .Select(message => message with { }).ToList();
                
                return new KeyValuePair<UserDto, List<DirectMessageDto>>(userCopy, messagesCopy);
            }
            
        }
        throw new KeyNotFoundException("Chat not found for the provided ChatPreview.");
    }




    
    
    public async Task LoadChatsAsync(string myUsername) {
        
        OnProgressChanged?.Invoke(progress, "Loading chats...");
        
        var me = await _appDbContext.Users.FirstOrDefaultAsync(u => u.UserName == myUsername) ?? throw new Exception("User not found");
        
        var messages = await _appDbContext.DirectMessages
            .Where(m => m.FromUser.Id == me.Id || m.ToUser.Id == me.Id)
            .Include(m => m.FromUser)
            .Include(m => m.ToUser)
            .ToListAsync();
        
        var chatDict = new Dictionary<UserDto, List<DirectMessageDto>>();  
        
        var groupedMessages = messages
            .GroupBy(m => m.FromUser.Id == me.Id ? m.ToUser : m.FromUser)
            .ToList();

        foreach (var group in groupedMessages) {
            var chatPartner = group.Key;
            var chatPartnerDto = new UserDto(chatPartner.UserName, chatPartner.Id);

            var messageList = new List<DirectMessageDto>();

            foreach (var message in group) {
                var messageDto = new DirectMessageDto(
                    From: message.FromUser.UserName,
                    To: message.ToUser.UserName,
                    Content: message.Content,
                    Created: message.Created,
                    Read: message.Read,
                    Delivered: message.Delivered
                );
                messageList.Add(messageDto);
            }
            
            var sortedMessages = messageList
                .OrderBy(m => m.Created)
                .ToList();

            chatDict[chatPartnerDto] = sortedMessages;
        }
        
        _chats = chatDict;
        progress = 100;
        OnProgressChanged?.Invoke(100, "Done");
    }

    


    public bool ChatsLoaded() {
        return progress == 100;
    }
    
}