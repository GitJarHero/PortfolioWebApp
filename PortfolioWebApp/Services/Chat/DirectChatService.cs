using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using PortfolioWebApp.Models;
using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services.Chat;

interface IDirectChatService {
    
    public Task LoadChatsAsync(string user);

    public bool ChatsLoaded();
    
    public event Action<int,string>? OnProgressChanged;
}

public class DirectDirectChatService : IDirectChatService {

    private readonly ILogger<DirectDirectChatService> _logger;
    private readonly AppDbContext _appDbContext;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    private Dictionary<UserDto, List<DirectMessageDto>> _chats = new();
    
    private int progress = 0;
    
    public event Action<int, string>? OnProgressChanged;
    
    
    public DirectDirectChatService(
        ILogger<DirectDirectChatService> logger, 
        AppDbContext appDbContext,
        AuthenticationStateProvider authenticationStateProvider
    ) {
        _logger = logger;
        _logger.LogInformation("ChatLoadingService initialized");
        _appDbContext = appDbContext;
        _authenticationStateProvider = authenticationStateProvider;
    }
    
    public async Task LoadChatsAsync(string username) {
        
        var userEntity = await _appDbContext.Users
            .FirstOrDefaultAsync(u => u.UserName == username);

        if (userEntity == null)
            throw new Exception("User not found");

        int userId = userEntity.Id;

        var messages = await _appDbContext.DirectMessages
            .Where(m => m.FromUser.Id == userId || m.ToUser.Id == userId)
            .Include(m => m.FromUser)
            .Include(m => m.ToUser) 
            .ToListAsync();

        
        //TODO: sort my messages by created desc and message from the chatPartner by delivered desc and then sort the result of both combined DESC

        
        var chatDict = new Dictionary<UserDto, List<DirectMessageDto>>();

        foreach (var message in messages) {
            var chatPartner = message.FromUser.Id == userId ? message.ToUser : message.FromUser;

            var chatPartnerDto = new UserDto(chatPartner.UserName, chatPartner.Id);
            var messageDto = new DirectMessageDto(
                From: message.FromUser.UserName,
                To: message.ToUser.UserName,
                Content: message.Content,
                Created: message.Created,
                Read: message.Read,
                Delivered: message.Delivered
            );

            if (!chatDict.TryGetValue(chatPartnerDto, out var chatList)) {
                chatList = new List<DirectMessageDto>();
                chatDict[chatPartnerDto] = chatList;
            }

            chatList.Add(messageDto);
        }

        _chats = chatDict;
    }


    public bool ChatsLoaded() {
        return progress == 100;
    }
    
}