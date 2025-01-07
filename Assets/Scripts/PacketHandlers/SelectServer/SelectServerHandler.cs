using ConnectProto;
using UnityEngine;

[MessageType("ServerListResponse")]
public class SelectServerHandler : IMessageHandler
{
    private SelectServerManager _selectServerManager;

    public void HandleMessage(byte[] message)
    {
        if (SelectServerManager.Instance == null)
        {
            return;
        }
        _selectServerManager = SelectServerManager.Instance;
        // Parse the HelloResponse message
        ServerListResponse serverListResponse = ServerListResponse.Parser.ParseFrom(message);
        _selectServerManager.DisplayServersList(serverListResponse.Servers);

    }
}

