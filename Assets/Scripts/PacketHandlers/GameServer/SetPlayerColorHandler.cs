using System;
using GameServerProto;

[MessageType("SetPlayerColor")]
public class SetPlayerColorHandler : IMessageHandler
{
  public void HandleMessage(byte[] message, Action<byte[]> sendMessage)
  {
    if (PlayerManager.Instance == null)
    {
      return;
    }
    SetPlayerColor data = SetPlayerColor.Parser.ParseFrom(message);
    PlayerManager.Instance.SetPlayerColor(data.Id, data.Color);
  }
}
