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
        PlayerData playerData = playerJoined.NewPlayer;
        PlayerManager.Instance.SpawnPlayer(playerData, new Vector3(playerData.X, playerData.Y, playerData.Z), Quaternion.Euler(playerData.RotationX, playerData.RotationY, playerData.RotationZ), false);
    }
}
