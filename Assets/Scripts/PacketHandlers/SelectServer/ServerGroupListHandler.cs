using System;
using ConnectProto;

[MessageType("ServerGroupList")]
public class ServerGroupListHandler : IMessageHandler
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
        ServerGroupList serverGroupListResponse = ServerGroupList.Parser.ParseFrom(message);
        _selectServerManager.DisplayServersList(serverGroupListResponse.ServerGroups);

    }
}
