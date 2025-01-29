using System;
using GameServerProto;
using UnityEngine;

[MessageType("MovePlayer")]
public class MovePlayerdHandler : IMessageHandler
{
    public void HandleMessage(byte[] message, Action<byte[]> sendMessage)
    {
        if (PlayerManager.Instance == null)
        {
            return;
        }
        MovePlayer movePlayer = MovePlayer.Parser.ParseFrom(message);
        PlayerManager.Instance.MovePlayer(movePlayer.Id, new Vector3(movePlayer.X, movePlayer.Y, movePlayer.Z));
    }
}
