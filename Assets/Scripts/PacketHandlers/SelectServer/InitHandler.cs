using ConnectProto;
using Google.Protobuf;
using NativeWebSocket;

[MessageType("Init")]
public class InitHandler : IMessageHandler
{
    public void HandleMessage(byte[] message, WebSocket websocket)
    {
        Wrapper wrapper = new()
        {
            Type = "RequestServerList",
            Payload = ByteString.Empty
        };
        websocket.Send(wrapper.ToByteArray());
    }
}

