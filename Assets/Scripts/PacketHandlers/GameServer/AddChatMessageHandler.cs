using System;
using GameServerProto;

[MessageType("AddChatMessage")]
public class AddChatMessageHandler : IMessageHandler
{
  public void HandleMessage(byte[] message, Action<byte[]> sendMessage)
  {
    if (PlayerManager.Instance == null)
    {
      return;
    }
    AddChatMessage messageData = AddChatMessage.Parser.ParseFrom(message);

    ChatSystem chatSystem = UnityEngine.Object.FindFirstObjectByType<ChatSystem>();
    if (chatSystem != null)
    {
      PlayerManager.Instance.AddPlayerChatBubble(messageData.Id, messageData.Message);
      chatSystem.AddChatMessage($"{messageData.Username}: {messageData.Message}");
    }
  }
}
