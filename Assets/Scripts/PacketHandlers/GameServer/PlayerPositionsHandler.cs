using System;
using GameServerProto;
using UnityEngine;

[MessageType("PlayerPositions")]
public class PlayerPositionsHandler : IMessageHandler
{
    public void HandleMessage(byte[] message, Action<byte[]> sendMessage)
    {
        if (PlayerManager.Instance == null)
        {
            return;
        }
        PlayerPositions playerPositions = PlayerPositions.Parser.ParseFrom(message);
        PlayerPositionData localPlayer = playerPositions.LocalPlayer;
        PlayerManager.Instance.SpawnPlayer(localPlayer.Id, new Vector3(localPlayer.X, localPlayer.Y, localPlayer.Z), Quaternion.Euler(localPlayer.RotationX, localPlayer.RotationY, localPlayer.RotationZ), true);
        foreach (PlayerPositionData otherPlayer in playerPositions.OtherPlayers)
        {
            PlayerManager.Instance.SpawnPlayer(otherPlayer.Id, new Vector3(otherPlayer.X, otherPlayer.Y, otherPlayer.Z), Quaternion.Euler(otherPlayer.RotationX, otherPlayer.RotationY, otherPlayer.RotationZ), false);
        }
    }
}
