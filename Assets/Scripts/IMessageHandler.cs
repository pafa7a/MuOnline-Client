using System;

public interface IMessageHandler
{
    void HandleMessage(byte[] message, Action<byte[]> sendMessage);
}
