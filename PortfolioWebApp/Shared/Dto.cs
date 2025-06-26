namespace PortfolioWebApp.Shared {

    public record UserDto(string username, int id, string profile_color);

    public record FriendShipRequestDto(UserDto from, UserDto to, DateTime ceated);

    public record FriendShipRequestAnswerDto(FriendShipRequestDto request, bool accepted);
    
    public record GlobalChatMessageDto(UserDto User, string Content, DateTime Created);

    public record DirectMessageDto(
        int MessageId,
        UserDto From,
        UserDto To,
        string Content,
        DateTime Created,
        DateTime? Read,
        DateTime? Delivered);

    public record ChatPreviewDto(UserDto ChatPartner, DirectMessageDto? LatestMessage, int NewMessagesCount);
    
    public record MessageDeliveredDto(List<int> DeliveredMessages, DateTime TimeStamp);
    
    public record MessageReadDto(List<int> ReadMessages, DateTime TimeStamp);
    
}
