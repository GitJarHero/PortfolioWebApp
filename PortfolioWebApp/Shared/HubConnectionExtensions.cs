using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace PortfolioWebApp.Shared;

/// <summary>
/// Provides extension methods for SignalR <see cref="HubConnection"/> and <see cref="IClientProxy"/>
/// that enable strongly-typed event binding and messaging using annotated event classes.
/// </summary>
/// <remarks>
/// These extensions work in combination with classes annotated with <see cref="HubEventAttribute"/>.
/// This allows cleaner syntax such as:
/// <code>
/// hubConnection.OnHubEvent&lt;MyEvent&gt;(e => { ... });
/// hubConnection.SendHubEventAsync(new MyEvent { ... });
/// Clients.User("userId").SendEventAsync(new MyEvent { ... });
/// </code>
/// instead of using magic strings like:
/// <code>
/// hubConnection.On&lt;MyEvent&gt;("EventName", e => { ... });
/// hubConnection.SendAsync("EventName", eventData);
/// </code>
/// </remarks>
public static class HubConnectionExtensions
{
    /// <summary>
    /// Subscribes to a SignalR hub event using a strongly-typed event class annotated with <see cref="HubEventAttribute"/>.
    /// </summary>
    /// <typeparam name="T">The event type.</typeparam>
    /// <param name="hubConnection">The SignalR hub connection.</param>
    /// <param name="handler">The delegate to handle the received event.</param>
    /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe from the event.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the event type is not annotated with <see cref="HubEventAttribute"/>.</exception>
    public static IDisposable OnHubEvent<T>(this HubConnection hubConnection, Action<T> handler)
    {
        var attr = typeof(T).GetCustomAttribute<HubEventAttribute>();
        if (attr == null)
        {
            throw new InvalidOperationException($"Type {typeof(T).Name} is missing {nameof(HubEventAttribute)}.");
        }

        return hubConnection.On<T>(attr.EventName, handler);
    }

    /// <summary>
    /// Sends a message to the SignalR hub using the event name defined in the <see cref="HubEventAttribute"/> on the payload type.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="hubConnection">The SignalR hub connection.</param>
    /// <param name="payload">The event payload to send.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the payload type is not annotated with <see cref="HubEventAttribute"/>.</exception>
    public static Task SendHubEventAsync<T>(this HubConnection hubConnection, T payload)
    {
        var attr = typeof(T).GetCustomAttribute<HubEventAttribute>();
        if (attr == null)
        {
            throw new InvalidOperationException($"Type {typeof(T).Name} is missing {nameof(HubEventAttribute)}.");
        }

        return hubConnection.SendAsync(attr.EventName, payload);
    }

    /// <summary>
    /// Sends a message from the server to a specific client using the event name defined in the <see cref="HubEventAttribute"/>.
    /// </summary>
    /// <typeparam name="TEvent">The event object type.</typeparam>
    /// <param name="clientProxy">The client proxy (e.g., Clients.User(...)).</param>
    /// <param name="eventObject">The event object to send.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the event object type is not annotated with <see cref="HubEventAttribute"/>.</exception>
    public static Task SendHubEventAsync<TEvent>(this IClientProxy clientProxy, TEvent eventObject)
    {
        var eventType = typeof(TEvent);
        var attr = eventType.GetCustomAttribute<HubEventAttribute>();

        if (attr == null)
            throw new InvalidOperationException($"Missing [{nameof(HubEventAttribute)}] on event type {eventType.Name}");

        return clientProxy.SendAsync(attr.EventName, eventObject);
    }
}
