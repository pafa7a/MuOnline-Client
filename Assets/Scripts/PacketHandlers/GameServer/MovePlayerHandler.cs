using System;
using GameServerProto;
using UnityEngine;
using UnityEngine.SceneManagement;

[MessageType("MovePlayer")]
public class MovePlayerdHandler : IMessageHandler
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
        MovePlayer movePlayer = MovePlayer.Parser.ParseFrom(message);
        PlayerManager.Instance.MovePlayer(movePlayer.Id, new Vector3(movePlayer.X, movePlayer.Y, movePlayer.Z));
    }
}
