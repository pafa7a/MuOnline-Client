using System.Threading.Tasks;
using ConnectProto;
using GameServerProto;
using Google.Protobuf;
using Google.Protobuf.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectServerManager : MonoBehaviour
{
    public static SelectServerManager Instance;
    private Canvas Canvas;
    private string _serverListGroupPrefabPath = "Prefabs/ServerListGroup";
    private string _serverButtonPrefabPath = "Prefabs/ServerButton";
    private string _serverListPrefabPath = "Prefabs/ServerList";
    private string _serverPrefabPath = "Prefabs/Server";
    private string _loginWrapperPrefabPath = "Prefabs/LoginWrapper";
    private GameObject _serverListGroup;
    private GameObject _serverList;
    private GameObject _loginWrapper;

    void Awake()
    {
        if (SceneManager.GetActiveScene().name == "ServerSelect")
        {
            Instance = this;
            Canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        }
        else
        {
            DestroyImmediate(_serverListGroup);
            DestroyImmediate(_serverList);
            DestroyImmediate(_loginWrapper);
        }
    }

    void Update()
    {
        if (_loginWrapper == null || PopUpMessage.IsActive) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OkButtonPressed();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _ = CancelButtonPressed();
        }
    }

    public void DisplayServersList(RepeatedField<ServerGroupInfo> serverGroups)
    {
        if (_serverListGroup != null)
        {
            DestroyImmediate(_serverListGroup);
        }

        if (_serverList == null)
        {
            GameObject ServerListPrefab = Resources.Load<GameObject>(_serverListPrefabPath);
            _serverList = Instantiate(ServerListPrefab, Vector3.zero, Quaternion.identity, Canvas.transform);
            RectTransform prefabTransform = ServerListPrefab.GetComponent<RectTransform>();
            RectTransform ServerListRectTransform = _serverList.GetComponent<RectTransform>();
            ServerListRectTransform.offsetMin = prefabTransform.offsetMin;
            ServerListRectTransform.offsetMax = prefabTransform.offsetMax;
            ServerListRectTransform.sizeDelta = prefabTransform.sizeDelta;
        }

        ClearServerList();

        GameObject ServerListGroupPrefab = Resources.Load<GameObject>(_serverListGroupPrefabPath);
        _serverListGroup = Instantiate(ServerListGroupPrefab, Vector3.zero, Quaternion.identity, Canvas.transform);

        RectTransform ServerListGroupRectTransform = _serverListGroup.GetComponent<RectTransform>();
        ServerListGroupRectTransform.offsetMin = Vector2.zero;
        ServerListGroupRectTransform.offsetMax = Vector2.zero;

        GameObject ServerButtonPrefab = Resources.Load<GameObject>(_serverButtonPrefabPath);

        foreach (var serverGroup in serverGroups)
        {
            var serverGroupButtonPrefab = Instantiate(ServerButtonPrefab, Vector3.zero, Quaternion.identity, _serverListGroup.transform);
            var buttonText = serverGroupButtonPrefab.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = serverGroup.Name;
            }

            var button = serverGroupButtonPrefab.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    ClearServerList();
                    foreach (var server in serverGroup.Servers)
                    {
                        GameObject ServerPrefab = Resources.Load<GameObject>(_serverPrefabPath);
                        var serverPrefab = Instantiate(ServerPrefab, Vector3.zero, Quaternion.identity, _serverList.transform);
                        var serverLoadImage = serverPrefab.transform.Find("ServerLoad")?.GetComponent<Image>();
                        var serverText = serverPrefab.GetComponentInChildren<TMP_Text>();

                        if (serverText != null)
                        {
                            serverText.text = server.Name;
                        }

                        if (serverLoadImage != null)
                        {
                            float serverLoad = 0f;
                            float.TryParse(server.LoadPercentage, out serverLoad);
                            serverLoadImage.fillAmount = serverLoad;
                        }

                        var serverButton = serverPrefab.GetComponent<Button>();
                        if (serverButton != null)
                        {
                            serverButton.onClick.AddListener(() =>
                            {
                                serverButton.onClick.RemoveAllListeners();
                                WebSocketClient.instance.ConnectToGameServer(server.Ip, server.Port, server.Id);

                                DestroyImmediate(_serverListGroup);
                                DestroyImmediate(_serverList);

                                GameObject LoginWrapperPrefab = Resources.Load<GameObject>(_loginWrapperPrefabPath);
                                // Set the server name.
                                LoginWrapperPrefab.transform.Find("ServerName").GetComponent<TextMeshProUGUI>().text = server.Name;
                                _loginWrapper = Instantiate(LoginWrapperPrefab, Vector3.zero, Quaternion.identity, Canvas.transform);

                                RectTransform LoginWrapperRectTransform = _loginWrapper.GetComponent<RectTransform>();
                                RectTransform prefabTransform = LoginWrapperPrefab.GetComponent<RectTransform>();
                                LoginWrapperRectTransform.offsetMin = prefabTransform.offsetMin;
                                LoginWrapperRectTransform.offsetMax = prefabTransform.offsetMax;
                                LoginWrapperRectTransform.sizeDelta = prefabTransform.sizeDelta;

                                // Find buttons inside the instantiated prefab and add their click listeners.
                                Button okButton = _loginWrapper.transform.Find("ButtonsWrapper/OkButton")?.GetComponent<Button>();
                                Button cancelButton = _loginWrapper.transform.Find("ButtonsWrapper/CancelButton")?.GetComponent<Button>();

                                if (okButton != null)
                                {
                                    okButton.onClick.RemoveAllListeners();
                                    okButton.onClick.AddListener(() => OkButtonPressed());

                                    // Ensure the UI system sets focus on the OK button
                                    EventSystem.current.SetSelectedGameObject(okButton.gameObject);
                                }

                                if (cancelButton != null)
                                {
                                    cancelButton.onClick.RemoveAllListeners();
                                    cancelButton.onClick.AddListener(() => _ = CancelButtonPressed());
                                }
                            });
                        }
                    }
                });
            }
        }
    }

    private void ClearServerList()
    {
        foreach (Transform child in _serverList.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void OnDestroy()
    {
        Destroy(gameObject);
        DestroyImmediate(_serverListGroup);
        DestroyImmediate(_serverList);
        DestroyImmediate(_loginWrapper);
    }

    public void OkButtonPressed()
    {
        if (_loginWrapper == null)
        {
            Debug.LogError("❌ _loginWrapper is null!");
            return;
        }

        TMP_InputField usernameInput = _loginWrapper.transform.Find("InputsWrapper/Username")?.GetComponent<TMP_InputField>();
        TMP_InputField passwordInput = _loginWrapper.transform.Find("InputsWrapper/Password")?.GetComponent<TMP_InputField>();

        if (usernameInput == null || passwordInput == null)
        {
            Debug.LogError("❌ Username or Password input field not found in LoginWrapper!");
            return;
        }

        string username = usernameInput.text;
        string password = passwordInput.text;
        if (string.IsNullOrEmpty(username))
        {
            PopUpMessage.Show("Enter your account name", PopUpMessage.ButtonsEnum.OK, () => InputManager.SelectInputField(0));
            return;
        }
        if (string.IsNullOrEmpty(password))
        {
            PopUpMessage.Show("Enter your password", PopUpMessage.ButtonsEnum.OK, () => InputManager.SelectInputField(1));
            return;
        }
        SendLoginRequest();
    }

    public async Task CancelButtonPressed()
    {
        DestroyImmediate(_loginWrapper);
        await WebSocketClient.instance.ConnectToConnectServer();
    }

    private void SendLoginRequest()
    {
        TMP_InputField usernameInput = _loginWrapper.transform.Find("InputsWrapper/Username")?.GetComponent<TMP_InputField>();
        TMP_InputField passwordInput = _loginWrapper.transform.Find("InputsWrapper/Password")?.GetComponent<TMP_InputField>();

        LoginRequest loginRequest = new()
        {
            Username = usernameInput.text,
            Password = passwordInput.text,
            Version = WebSocketClient.instance.Version,
            Serial = WebSocketClient.instance.Serial,
        };

        GameServerProto.Wrapper wrapper = new()
        {
            Type = "LoginRequest",
            Payload = loginRequest.ToByteString(),
        };
        WebSocketClient.Send(wrapper.ToByteArray());
    }
}
