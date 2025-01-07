using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class MessageTypeAttribute : Attribute
{
    public string MessageType { get; }

    public MessageTypeAttribute(string messageType)
    {
        MessageType = messageType;
    }
}
