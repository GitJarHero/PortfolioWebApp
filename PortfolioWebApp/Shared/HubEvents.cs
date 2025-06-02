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
}