using System;
using GameServerProto;
using UnityEngine;

[MessageType("PlayerJoined")]
public class PlayerJoinedHandler : IMessageHandler
{
    public void HandleMessage(byte[] message, Action<byte[]> sendMessage)
    {
        if (PlayerManager.Instance == null)
        {
            return;
        }
        PlayerJoined playerJoined = PlayerJoined.Parser.ParseFrom(message);
        PlayerPositionData newPlayerPosition = playerJoined.NewPlayer;
        PlayerManager.Instance.SpawnPlayer(newPlayerPosition.Id, new Vector3(newPlayerPosition.X, newPlayerPosition.Y, newPlayerPosition.Z), Quaternion.Euler(newPlayerPosition.RotationX, newPlayerPosition.RotationY, newPlayerPosition.RotationZ), false);

    }
}
