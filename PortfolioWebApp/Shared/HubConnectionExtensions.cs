using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace PortfolioWebApp.Shared;

/*
 * These extensions allow to mark classes with [HubEvent()]
 */
public static class HubConnectionExtensions
{
    
    public static void OnHubEvent<T>(this HubConnection hubConnection, Func<T, Task> handler)
    {
        var attr = typeof(T).GetCustomAttribute<HubEventAttribute>();
        if (attr == null)
        {
            throw new InvalidOperationException($"Type {typeof(T).Name} is missing " + typeof(HubEventAttribute));
        }

        hubConnection.On<T>(attr.EventName, handler);
    }
    
     public static Task SendHubEventAsync<T>(this HubConnection hubConnection, T payload)
     {
        var attr = typeof(T).GetCustomAttribute<HubEventAttribute>();
        if (attr == null)
        {
           throw new InvalidOperationException($"Type {typeof(T).Name} is missing " + typeof(HubEventAttribute));
        }

        return hubConnection.SendAsync(attr.EventName, payload);
     }
     
     
     public static Task SendEventAsync<TEvent>(this IClientProxy clientProxy, TEvent eventObject)
     {
         var eventType = typeof(TEvent);
         var attr = eventType.GetCustomAttribute<HubEventAttribute>();

         if (attr == null)
             throw new InvalidOperationException($"Missing [HubEvent] attribute on event type {eventType.Name}");

         return clientProxy.SendAsync(attr.EventName, eventObject);
     }
    
}

    