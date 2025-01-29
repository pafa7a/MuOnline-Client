using System;
using GameServerProto;

[MessageType("PlayerDisconnected")]
public class PlayerDisconnectedHandler : IMessageHandler
{
    public void HandleMessage(byte[] message, Action<byte[]> sendMessage)
    {
        if (PlayerManager.Instance == null)
        {
            return;
        }
        PlayerDisconnected playerDisconnected = PlayerDisconnected.Parser.ParseFrom(message);
        PlayerManager.Instance.RemovePlayer(playerDisconnected.Id);

    }
}
