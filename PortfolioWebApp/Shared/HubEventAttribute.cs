namespace PortfolioWebApp.Shared;

[AttributeUsage(AttributeTargets.Class)]
public class HubEventAttribute : Attribute {
    public required string EventName { get; set; }
}