using ConnectProto;
using Unity.VisualScripting;
using UnityEngine;

[MessageType("HelloResponse")]
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
        HelloResponse helloResponse = HelloResponse.Parser.ParseFrom(message);
        Debug.Log($"Received HelloResponse: {helloResponse.Message}");
        _selectServerManager.DisplayServersList();

    }
}

