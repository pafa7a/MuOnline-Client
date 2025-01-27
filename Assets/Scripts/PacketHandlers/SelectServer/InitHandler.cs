using System;
using ConnectProto;
using Google.Protobuf;

[MessageType("Init")]
public class InitHandler : IMessageHandler
{
    public void HandleMessage(byte[] message, Action<byte[]> Send)
    {
        Wrapper wrapper = new()
        {
            Type = "RequestServerGroupList",
            Payload = ByteString.Empty
        };
        Send(wrapper.ToByteArray());
    }
}

