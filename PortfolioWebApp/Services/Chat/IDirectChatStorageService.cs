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

    public bool ChatsLoaded();
    
    public event Action<int,string>? OnProgressChanged;

    public List<ChatPreview> GetChatPreviews(ChatPreviewFilter filter);

    public KeyValuePair<UserDto, List<DirectMessageDto>> GetFullChatForChatPreview(ChatPreview chatPreview);
    
}