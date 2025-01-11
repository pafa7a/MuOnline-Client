using ConnectProto;
using NativeWebSocket;

[MessageType("ServerList")]
public class ServerListHandler : IMessageHandler
{
    private SelectServerManager _selectServerManager;

    public void HandleMessage(byte[] message, WebSocket websocket)
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
