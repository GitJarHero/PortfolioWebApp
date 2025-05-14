namespace PortfolioWebApp.Shared;

    
public record UserDto(string username, int id);

public record FriendShipRequestDto(string from, string to, DateTime ceated);
