namespace PortfolioWebApp.Services.Chat;

interface IChatLoadingService {
    
    public Task LoadChatsAsync();

    public bool ChatsLoaded();
    
    public event Action<int,string> OnProgressChanged;
}

public class ChatLoadingService : IChatLoadingService {

    private readonly ILogger<ChatLoadingService> _logger;
    
    private List<string> chats = new();
    
    private int progress = 0;
    
    public event Action<int, string> OnProgressChanged;
    
    public ChatLoadingService(ILogger<ChatLoadingService> logger) {
        _logger = logger;
        _logger.LogInformation("ChatLoadingService initialized");
    }
    
    public async Task LoadChatsAsync() {

        for (; progress < 100; progress++) {
            OnProgressChanged?.Invoke(progress, "Loading chats...");
            await Task.Delay(50);
        }
        chats.Add("Chat 1");
        chats.Add("Chat 2");
        chats.Add("Chat 3");
        OnProgressChanged?.Invoke(100, "Done");
    }

    public bool ChatsLoaded() {
        return progress == 100;
    }
    
}