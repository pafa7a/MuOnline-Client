using NativeWebSocket;

public interface IMessageHandler
{
    void HandleMessage(byte[] message, WebSocket websocket);
}
