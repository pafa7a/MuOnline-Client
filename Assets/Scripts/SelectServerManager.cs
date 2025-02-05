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
    private string _registerWrapperPrefabPath = "Prefabs/RegisterWrapper";
    private GameObject _serverListGroup;
    private GameObject _serverList;
    private GameObject _loginWrapper;
    private GameObject _registerWrapper;

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
            DestroyImmediate(_registerWrapper);
        }
    }

    void Update()
    {
        if (PopUpMessage.IsActive) return;

        if (_loginWrapper != null && _loginWrapper.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OkButtonPressed();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _ = CancelButtonPressed();
            }
        }

        if (_registerWrapper != null && _registerWrapper.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OkButtonRegisterPressed();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelButtonRegisterPressed();
            }
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
                                Button createlButton = _loginWrapper.transform.Find("ButtonsWrapper/CreateButton")?.GetComponent<Button>();
                                Button cancelButton = _loginWrapper.transform.Find("ButtonsWrapper/CancelButton")?.GetComponent<Button>();

                                if (okButton != null)
                                {
                                    okButton.onClick.RemoveAllListeners();
                                    okButton.onClick.AddListener(() => OkButtonPressed());

                                    // Ensure the UI system sets focus on the OK button
                                    EventSystem.current.SetSelectedGameObject(okButton.gameObject);
                                }

                                if (createlButton != null)
                                {
                                    createlButton.onClick.RemoveAllListeners();
                                    createlButton.onClick.AddListener(() => CreateButtonPressed());
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
            PopUpMessage.Show("Enter your account name", PopUpMessage.ButtonsEnum.OK, () => _loginWrapper.GetComponent<InputManager>().SelectInputField(0));
            return;
        }
        if (string.IsNullOrEmpty(password))
        {
            PopUpMessage.Show("Enter your password", PopUpMessage.ButtonsEnum.OK, () => _loginWrapper.GetComponent<InputManager>().SelectInputField(1));
            return;
        }
        SendLoginRequest();
    }

    public async Task CancelButtonPressed()
    {
        DestroyImmediate(_loginWrapper);
        await WebSocketClient.instance.ConnectToConnectServer();
    }


    public void CreateButtonPressed()
    {
        _loginWrapper.SetActive(false);
        
        TMP_InputField loginUsernameInput = _loginWrapper.transform.Find("InputsWrapper/Username")?.GetComponent<TMP_InputField>();
        TMP_InputField loginPasswordInput = _loginWrapper.transform.Find("InputsWrapper/Password")?.GetComponent<TMP_InputField>();

        loginUsernameInput.text = "";
        loginPasswordInput.text = "";

        GameObject RegisterWrapperPrefab = Resources.Load<GameObject>(_registerWrapperPrefabPath);
        // Set the server name.
        _registerWrapper = Instantiate(RegisterWrapperPrefab, Vector3.zero, Quaternion.identity, Canvas.transform);

        RectTransform RegisterWrapperRectTransform = _registerWrapper.GetComponent<RectTransform>();
        RectTransform prefabTransform = RegisterWrapperPrefab.GetComponent<RectTransform>();
        RegisterWrapperRectTransform.offsetMin = prefabTransform.offsetMin;
        RegisterWrapperRectTransform.offsetMax = prefabTransform.offsetMax;
        RegisterWrapperRectTransform.sizeDelta = prefabTransform.sizeDelta;

        Button okButton = _registerWrapper.transform.Find("ButtonsWrapper/OkButton")?.GetComponent<Button>();
        Button cancelButton = _registerWrapper.transform.Find("ButtonsWrapper/CancelButton")?.GetComponent<Button>();

        if (okButton != null)
        {
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(() => OkButtonRegisterPressed());

            // Ensure the UI system sets focus on the OK button
            EventSystem.current.SetSelectedGameObject(okButton.gameObject);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => CancelButtonRegisterPressed());
        }
    }


    public void OkButtonRegisterPressed()
    {
        if (_registerWrapper == null)
        {
            Debug.LogError("❌ _registerWrapper is null!");
            return;
        }

        TMP_InputField registerUsernameInput = _registerWrapper.transform.Find("InputsWrapper/Username")?.GetComponent<TMP_InputField>();
        TMP_InputField registerEmailInput = _registerWrapper.transform.Find("InputsWrapper/Email")?.GetComponent<TMP_InputField>();
        TMP_InputField registerPasswordInput = _registerWrapper.transform.Find("InputsWrapper/Password")?.GetComponent<TMP_InputField>();
        TMP_InputField registerRepeatPasswordInput = _registerWrapper.transform.Find("InputsWrapper/RepeatPassword")?.GetComponent<TMP_InputField>();

        if (registerUsernameInput == null || registerPasswordInput == null || registerEmailInput == null || registerRepeatPasswordInput == null)
        {
            Debug.LogError("❌ Username or Password or Email or RepeatPassword input field not found in LoginWrapper!");
            return;
        }

        string username = registerUsernameInput.text;
        string email = registerEmailInput.text;
        string password = registerPasswordInput.text;
        string repeatPassword = registerRepeatPasswordInput.text;
        if (string.IsNullOrEmpty(username))
        {
            PopUpMessage.Show("Enter your account name", PopUpMessage.ButtonsEnum.OK, () => _registerWrapper.GetComponent<InputManager>().SelectInputField(0));
            return;
        }
        if (string.IsNullOrEmpty(email))
        {
            PopUpMessage.Show("Enter your email", PopUpMessage.ButtonsEnum.OK, () => _registerWrapper.GetComponent<InputManager>().SelectInputField(1));
            return;
        }
        if (string.IsNullOrEmpty(password))
        {
            PopUpMessage.Show("Enter your password", PopUpMessage.ButtonsEnum.OK, () => _registerWrapper.GetComponent<InputManager>().SelectInputField(2));
            return;
        }
        if (string.IsNullOrEmpty(repeatPassword))
        {
            PopUpMessage.Show("Repeat your password", PopUpMessage.ButtonsEnum.OK, () => _registerWrapper.GetComponent<InputManager>().SelectInputField(3));
            return;
        }
        if (password != repeatPassword)
        {
            PopUpMessage.Show("Passwords do not match", PopUpMessage.ButtonsEnum.OK, () => _registerWrapper.GetComponent<InputManager>().SelectInputField(2));
            return;
        }
        SendLoginRequest();
    }


    public void CancelButtonRegisterPressed()
    {
        DestroyImmediate(_registerWrapper);
        _loginWrapper.SetActive(true);
        _loginWrapper.GetComponent<InputManager>().SelectInputField(0);
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
