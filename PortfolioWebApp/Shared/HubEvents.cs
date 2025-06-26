namespace PortfolioWebApp.Shared;

public static class HubEvents
{
    public static class GlobalChat
    {
        public static class Server
        {
            [HubEvent(EventName = "BroadcastMessage")]
            public record BroadCastEvent(GlobalChatMessageDto Payload);
        }

        public static class Client
        {
            [HubEvent(EventName = "ReceiveMessage")]
            public record MessageReceivedEvent(GlobalChatMessageDto Payload);
        }
    }

    public static class DirectChat
    {
        public static class Server
        {
            [HubEvent(EventName = "SendMessage")]
            public record SendMessageEvent(DirectMessageDto Payload);

            [HubEvent(EventName = "SendMessageDelivered")]
            public record MessageDeliveredEvent(MessageDeliveredDto Payload);

            [HubEvent(EventName = "SendMessageRead")]
            public record MessageReadEvent(MessageReadDto Payload);
        }

        public static class Client
        {
            [HubEvent(EventName = "ReceiveDirectMessage")]
            public record MessageReceivedEvent(DirectMessageDto Payload);

            [HubEvent(EventName = "MessageSentAcknowledgement")]
            public record MessageAcknowledgedEvent(DirectMessageDto Payload);

            [HubEvent(EventName = "MessageDelivered")]
            public record MessageDeliveredEvent(MessageDeliveredDto Payload);

            [HubEvent(EventName = "MessageRead")]
            public record MessageReadEvent(MessageReadDto Payload);
        }
    }

    public static class FriendRequest 
    {
        public static class Server 
        {
            [HubEvent(EventName = "SendFriendRequest")]
            public record SendFriendRequestEvent(FriendShipRequestDto Payload);
            
            [HubEvent(EventName = "SendFriendRequestAnswer")]
            public record SendFriendRequestAnswerEvent(FriendShipRequestAnswerDto Payload);

            [HubEvent(EventName = "SendFriendRequestCancellation")]
            public record SendFriendRequestCancellationEvent(FriendShipRequestDto Payload);

        }  
        public static class Client 
        {
            [HubEvent(EventName = "ReceiveFriendRequestCancellation")]
            public record ReceiveFriendRequestCancellationEvent(FriendShipRequestDto Payload);
            
            [HubEvent(EventName = "ReceiveFriendRequestAnswer")]
            public record ReceiveFriendRequestAnswerEvent(FriendShipRequestAnswerDto Payload);
            
            [HubEvent(EventName = "SendFriendRequestAnswerAck")]
            public record FriendRequestAnswerAcknowledgedEvent(FriendShipRequestAnswerDto Payload);
            
            [HubEvent(EventName = "ReceiveFriendRequest")]
            public record ReceiveFriendRequestEvent(FriendShipRequestDto Payload);
            
            [HubEvent(EventName = "SendFriendRequestAck")]
            public record FriendRequestSentAcknowledgedEvent(FriendShipRequestDto Payload);
        }   
    }
}