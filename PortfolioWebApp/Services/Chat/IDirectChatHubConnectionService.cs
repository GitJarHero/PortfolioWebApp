using PortfolioWebApp.Shared;

namespace PortfolioWebApp.Services.Chat;

public interface IDirectChatHubConnectionService {

    // invoked, when the user receives a direct message from the hub
    event Action<DirectMessageDto> ReceiveMessage;
    
    // invoked, when the hub received and handled the message
    event Action<DirectMessageDto> MessageSentAcknowledgement;
    
    // invoked, when a message(s) was received by the chat partner
    event Action<MessageDeliveredDto> MessageDelivered;
     
    // invoked, when a message(s) was read by the chat partner
    event Action<MessageReadDto> MessageRead;
    
    // Starts a connection to the DirectChatHub
    Task Connect();
    
    bool IsConnected();
    
    // Closes the connection to the DirectChatHub 
    Task Disconnect();
    
    // Sends a DirectMessage to the hub
    Task Send(DirectMessageDto message);

    // use this for new messages (no delivered date)
    // in the initially loaded messages
    // &
    // in the MessageReceived event handler
    Task SendMessageDelivered(MessageDeliveredDto dto);
    
    // use when opening a chatwindow that contains unread messages (no read date)
    // &
    // in the MessageReceived event handler when the chatwindow that belongs the the received message is open.
    Task SendMessageRead(MessageReadDto dto);

}