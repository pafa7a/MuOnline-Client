using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using GameServerProto;
using Google.Protobuf;
using UnityEngine.UI;

public class ChatSystem : MonoBehaviour
{
  private GameObject chatViewWrapper;
  private GameObject chatInputWrapper;
  private TMP_InputField chatInput;
  private TMP_Text chatText;

  private bool isServerSelect = false;
  private bool isInputActive = false;
  private bool isInputFieldFocused = false;

  void Start()
  {
    chatViewWrapper = transform.Find("ChatViewWrapper").gameObject;
    chatInputWrapper = transform.Find("ChatInputWrapper").gameObject;
    UpdateChatVisibility(SceneManager.GetActiveScene().name);
    SceneManager.activeSceneChanged += OnActiveSceneChanged;
    chatInput = chatInputWrapper.GetComponentInChildren<TMP_InputField>();
    chatInput.onSelect.AddListener(OnInputFocused);
    chatInput.onDeselect.AddListener(OnInputUnfocused);
    chatText = chatViewWrapper.GetComponentInChildren<TMP_Text>();
  }

  void OnDestroy()
  {
    chatInput.onSelect.RemoveListener(OnInputFocused);
    chatInput.onDeselect.RemoveListener(OnInputUnfocused);
    SceneManager.activeSceneChanged -= OnActiveSceneChanged;
  }

  private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
  {
    UpdateChatVisibility(newScene.name);
  }

  private void UpdateChatVisibility(string sceneName)
  {
    isServerSelect = sceneName == "ServerSelect";
    isInputActive = false;  // Reset input state on scene change

    if (chatViewWrapper != null)
    {
      chatViewWrapper.SetActive(!isServerSelect);
      chatInputWrapper.SetActive(false);
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
    {
      HandleEnterKey();
    }
    else if (Input.GetKeyDown(KeyCode.Escape) && isInputActive)
    {
      chatInputWrapper.SetActive(false);
      isInputActive = false;
    }
  }

  private void OnInputFocused(string value)
  {
    isInputFieldFocused = true;
  }

  private void OnInputUnfocused(string value)
  {
    isInputFieldFocused = false;
  }

  private void HandleEnterKey()
  {
    if (isServerSelect) return;

    if (!isInputActive)
    {
      // First Enter - Show and focus
      chatInputWrapper.SetActive(true);
      chatInput.ActivateInputField();
      chatInput.Select();
      isInputActive = true;
    }
    else if (isInputFieldFocused)
    {
      // Second Enter - Submit and hide
      string currentText = chatInput.text;
      if (!string.IsNullOrWhiteSpace(currentText))
      {
        PlayerSendChatMessage chatMessage = new()
        {
          Message = currentText
        };
        Wrapper wrapper = new()
        {
          Type = "PlayerSendChatMessage",
          Payload = chatMessage.ToByteString()
        };
        WebSocketClient.Send(wrapper.ToByteArray());

        chatInput.SetTextWithoutNotify("");
      }
      chatInputWrapper.SetActive(false);
      isInputActive = false;
    }
  }

  public void AddChatMessage(string message)
  {
    if (chatText != null)
    {
      chatText.text += $"\n{message}";
      chatViewWrapper.GetComponent<Image>().color = new UnityEngine.Color(0, 0, 0, 0.588f);
    }
  }
}
