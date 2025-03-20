using System;
using GameServerProto;
using UnityEngine;
using UnityEngine.SceneManagement;

[MessageType("PlayersData")]
public class PlayersDataHandler : IMessageHandler
{
    public void HandleMessage(byte[] message, Action<byte[]> sendMessage)
    {
        if (PlayerManager.Instance == null)
        {
            return;
        }
        if (SceneManager.GetActiveScene().name != "World1")
        {
            return;
        }
        PlayersData playersData = PlayersData.Parser.ParseFrom(message);
        PlayerData localPlayerData = playersData.LocalPlayer;
        PlayerManager.Instance.SpawnPlayer(localPlayerData, new Vector3(localPlayerData.X, localPlayerData.Y, localPlayerData.Z), Quaternion.Euler(localPlayerData.RotationX, localPlayerData.RotationY, localPlayerData.RotationZ), true);
        foreach (PlayerData otherPlayerData in playersData.OtherPlayers)
        {
            PlayerManager.Instance.SpawnPlayer(otherPlayerData, new Vector3(otherPlayerData.X, otherPlayerData.Y, otherPlayerData.Z), Quaternion.Euler(otherPlayerData.RotationX, otherPlayerData.RotationY, otherPlayerData.RotationZ), false);
        }
    }
}
