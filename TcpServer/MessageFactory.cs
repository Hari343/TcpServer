using System.Text.Json;

namespace TcpServer;

// Message types for communication protocol. Server will only use RequestStatus.
public enum MessageType
{
    Ack,
    RequestStatus,
    StatusResponse,
    ClientBusy
}

// Message class for communication protocol
public class Message(MessageType type, bool[] content)
{
    public MessageType Type { get; init; } = type;
    public bool[] Content { get; init; } = content; // As of now only StatusResponse will have content (bool[10])
    public DateTime Timestamp { get; init; } = DateTime.Now;
}

// A simple factory for creating / parsing messages
public static class MessageFactory
{
    private static Message CreateMessage(MessageType type, bool[]? content = null)
    {
        content ??= [];
        return new Message(type, content);
    }

    public static Message CreateAckMessage()
    {
        return CreateMessage(MessageType.Ack);
    }

    public static Message CreateStatusMessage(bool[] buttonStatus)
    {
        return CreateMessage(MessageType.StatusResponse, buttonStatus);
    }

    public static Message CreateBusyMessage()
    {
        return CreateMessage(MessageType.ClientBusy);
    }

    public static Message CreateStatusRequestMessage()
    {
        return CreateMessage(MessageType.RequestStatus);
    }
    
    public static Message? ParseMessage(string message)
    {
        return JsonSerializer.Deserialize<Message>(message);
    }
}