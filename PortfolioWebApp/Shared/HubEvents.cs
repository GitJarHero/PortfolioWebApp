namespace PortfolioWebApp.Shared;

public static class HubEvents
{
    public static class GlobalChat
    {
        public static class Server
        {
            public const string BroadCast = "BroadcastMessage";
        }

        public static class Client
        {
            public const string Receive = "ReceiveMessage";
        }
    }

    public static class DirectChat
    {
        public static class Server
        {
            public const string Send = "SendMessage";
            public const string SendDelivered = "SendMessageDelivered";
            public const string SendRead = "SendMessageRead";
        }

        public static class Client
        {
            public const string MessageReceived = "ReceiveDirectMessage";
            public const string MessageAcknowledged = "MessageSentAcknowledgement";
            public const string MessageDelivered = "MessageDelivered";
            public const string MessageRead = "MessageRead";
            
        }
    }
}