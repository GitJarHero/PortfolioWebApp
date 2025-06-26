using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services.Chat;

public interface IDirectChatStorageService {
    
    public enum ChatPreviewFilter {
        All,
        Unread,
        Favorites,
        Groups
    }
    
    public Task LoadChatsAsync(string user);
    
    // To be used after LoadChatsAsync() is done. Marks Messages as delivered IF: the message has no delivery date && message.toUser = current user
    // returns: for each user that sent new messages to us: a MessageDeliveredDto with all the new messages from that user
    public Task<List<MessageDeliveredDto>> MarkNewMessagesAsDeliveredAsync();

    // To be used when opening a chat window with a chat partner. Marks all messages from that chatpartner as read
    public List<int> MarkUnreadMessagesAsRead(int fromUserId, DateTime readDate);
    
    public bool ChatsLoaded();

    public void InvalidateStorage();
    
    public event Action<int,string>? OnProgressChanged;

    public List<ChatPreviewDto> GetChatPreviews(ChatPreviewFilter filter);

    public KeyValuePair<UserDto, List<DirectMessageDto>> GetFullChatForChatPreview(ChatPreviewDto chatPreviewDto);
    
    public void HandleReceiveMessage(DirectMessageDto message);
    
    public void HandleMessageSentAcknowledgement(DirectMessageDto message);
    
    public void HandleMessageDelivered(MessageDeliveredDto message);
    
    public void HandleMessageRead(MessageReadDto message);
    
}