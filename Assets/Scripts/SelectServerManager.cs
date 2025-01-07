using System.Collections.Generic;
using ConnectProto;
using Google.Protobuf.Collections;
using TMPro;
using UnityEngine;

public class SelectServerManager : MonoBehaviour
{
    public static SelectServerManager Instance;
    public Canvas Canvas;
    public GameObject ServerListGroupPrefab;
    public GameObject ServerButtonPrefab;
    private GameObject _serverListGroup;
    private List<GameObject> _serversInfoList = new();
    void Awake()
    {
        // Ensure this is a Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DisplayServersList(RepeatedField<ServerInfo> servers)
    {
        if (_serverListGroup != null)
        {
            DestroyImmediate(_serverListGroup);
        }
        if (_serversInfoList != null)
        {
            foreach (var serverInfo in _serversInfoList)
            {
                DestroyImmediate(serverInfo);
            }
        }

        _serverListGroup = Instantiate(ServerListGroupPrefab, Vector3.zero, Quaternion.identity, Canvas.transform);

        // Set custom position, as it gets overrided by Unity bug...
        RectTransform ServerListGroupRectTransform = _serverListGroup.GetComponent<RectTransform>();
        ServerListGroupRectTransform.offsetMin = Vector2.zero;
        ServerListGroupRectTransform.offsetMax = Vector2.zero;

        foreach (var server in servers) {
            var serverPrefab = Instantiate(ServerButtonPrefab, Vector3.zero, Quaternion.identity, _serverListGroup.transform);
                        // Set the button label
            var buttonText = serverPrefab.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = server.Name; // Display the server name on the button
            }
            if (serverPrefab) {
                _serversInfoList.Add(serverPrefab);
            }
        }
    }
}

