using ConnectProto;
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
    private GameObject _serverListGroup;
    private GameObject _serverList;
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
            RectTransform ServerListRectTransform = _serverList.GetComponent<RectTransform>();
            ServerListRectTransform.offsetMin = Vector2.zero;
            ServerListRectTransform.offsetMax = Vector2.zero;
            ServerListRectTransform.sizeDelta = new Vector2(193, 208);
        }
        // Delete all children of the server list.
        ClearServerList();

        GameObject ServerListGroupPrefab = Resources.Load<GameObject>(_serverListGroupPrefabPath);
        _serverListGroup = Instantiate(ServerListGroupPrefab, Vector3.zero, Quaternion.identity, Canvas.transform);

        // Set custom position, as it gets overrided by Unity bug...
        RectTransform ServerListGroupRectTransform = _serverListGroup.GetComponent<RectTransform>();
        ServerListGroupRectTransform.offsetMin = Vector2.zero;
        ServerListGroupRectTransform.offsetMax = Vector2.zero;

        GameObject ServerButtonPrefab = Resources.Load<GameObject>(_serverButtonPrefabPath);

        foreach (var serverGroup in serverGroups)
        {
            var serverGroupButtonPrefab = Instantiate(ServerButtonPrefab, Vector3.zero, Quaternion.identity, _serverListGroup.transform);
            // Set the button label
            var buttonText = serverGroupButtonPrefab.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = serverGroup.Name; // Display the server group name on the button
            }

            // Add onClick listener to the button
            var button = serverGroupButtonPrefab.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    // Delete all children of the server list.
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
                            serverLoadImage.fillAmount = float.Parse(server.LoadPercentage);

                        }

                        var serverButton = serverPrefab.GetComponent<Button>();
                        if (serverButton != null)
                        {
                            serverButton.onClick.AddListener(() =>
                            {
                                WebSocketClient.instance.ConnectToGameServer(server.Ip, server.Port, server.Id);
                            });

                            EventTrigger trigger = serverButton.gameObject.AddComponent<EventTrigger>();
                            EventTrigger.Entry PointerEnterEvent = new()
                            {
                                eventID = EventTriggerType.PointerEnter
                            };
                            PointerEnterEvent.callback.AddListener((data) =>
                            {
                                serverText.color = new Color32(255, 255, 217, 255);
                            });

                            EventTrigger.Entry PointerExitEvent = new()
                            {
                                eventID = EventTriggerType.PointerExit
                            };
                            PointerExitEvent.callback.AddListener((data) =>
                            {
                                serverText.color = new Color32(255, 180, 0, 255);
                            });
                            trigger.triggers.Add(PointerEnterEvent);
                            trigger.triggers.Add(PointerExitEvent);
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
    }
}
