namespace PortfolioWebApp.Shared;

    
public record UserDto(string username, int id, string profile_color);

public record FriendShipRequestDto(UserDto from, UserDto to, DateTime ceated);

public record FriendShipRequestAnswerDto(FriendShipRequestDto request, bool accepted);

public record DirectMessageDto(string From, string To, string Content, DateTime? Created, DateTime? Read, DateTime? Delivered);

public record ChatPreview(UserDto ChatPartner, DirectMessageDto? LatestMessage, int NewMessagesCount);