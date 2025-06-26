namespace PortfolioWebApp.Shared;

/// <summary>
/// Marks a class as a SignalR event type and provides the associated event name used for communication.
/// </summary>
/// <remarks>
/// This attribute allows event classes to be mapped to SignalR method names, enabling strongly-typed communication.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class HubEventAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the event name used for SignalR communication.
    /// </summary>
    public required string EventName { get; set; }
}