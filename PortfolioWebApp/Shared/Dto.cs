namespace PortfolioWebApp.Shared;

    
public record UserDto(string username, int id);

public record FriendShipRequestDto(string from, string to, DateTime ceated);

// "from" refers to the user who wanted to be friends with "to"
public record FriendShipRequestAnswerDto(FriendShipRequestDto request, bool accepted);

public record DirectMessageDto(string From, string To, string Content, DateTime? Created, DateTime? Read, DateTime? Delivered);