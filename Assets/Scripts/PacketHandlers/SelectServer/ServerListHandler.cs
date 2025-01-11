using System;
using ConnectProto;

[MessageType("ServerList")]
public class ServerListHandler : IMessageHandler
{
    private SelectServerManager _selectServerManager;

    public void HandleMessage(byte[] message, Action<byte[]> sendMessage)
    {
        if (SelectServerManager.Instance == null)
        {
            return;
        }
        _selectServerManager = SelectServerManager.Instance;
        // Parse the ServerListResponse message
        ServerList serverListResponse = ServerList.Parser.ParseFrom(message);
        _selectServerManager.DisplayServersList(serverListResponse.Servers);

    }
}
